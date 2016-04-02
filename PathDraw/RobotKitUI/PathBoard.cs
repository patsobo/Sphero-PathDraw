using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls; // for the InkCanvas
using Windows.UI.Input.Inking;  // for the InkAttributes
using Windows.UI.Xaml;
using Windows.UI.Xaml.Input;
using Windows.Foundation;
using System.Numerics;  // for the Vectors
using Windows.UI;   // for color
using System.Diagnostics;
using Windows.UI.Xaml.Media;

namespace PathDraw
{
    class PathBoard
    {
        // Keeps track of the path length for one delta between path nodes in list
        private const float MAX_DIFF = 2f;  // max difference in path nodesm, in pixels
        private float magDiff;

        // For keeping track of points when writing to and reading from the path list
        private Point currentPoint;
        private Point previousPoint;

        // For keeping track of path control cursor
        private Point initialPoint; 

        private DispatcherTimer timer;
        private int count;  // iterates through the list when sending the roll commands

        //! @brief	sphero to control
        private RobotKit.Sphero m_sphero;

        //! @brief  the last time a command was sent in milliseconds
        private long m_lastCommandSentTimeMs;

        //! @brief	translate transform for the puck
        private TranslateTransform m_translateTransform;

        // the board being drawn on.
        private Canvas m_board;

        // Controls color and pen width
        InkDrawingAttributes m_attr;

        // collects a number of points with corresponding colors
        List<Point> path;
        List<Color> colors;

        // cursor that controls the current path spot.  Can only draw when you're touching this element
        private FrameworkElement m_pathControl;

        private Windows.UI.Xaml.Shapes.Polyline inkStroke;

        public PathBoard(RobotKit.Sphero sphero, FrameworkElement pathControl, Canvas board, InkDrawingAttributes attr)
        {
            Debug.WriteLine("INITIALIZING CANVAS...");
            m_sphero = sphero;
            m_pathControl = pathControl;
            m_board = board;
            m_attr = attr;

            path = new List<Point>();

            timer = new DispatcherTimer();
            timer.Tick += timerTick;
            timer.Interval = new TimeSpan(0,0,0,0,20);   // 10 milliseconds, or 10 Hz
            count = 0;

            m_translateTransform = new TranslateTransform();
            m_pathControl.RenderTransform = m_translateTransform;

            m_pathControl.PointerMoved += PointerMoved;
            m_pathControl.PointerPressed += PointerPressed;
            m_pathControl.PointerReleased += PointerReleased;
        }

        private void timerTick(object sender, object e)
        {
            SendRollCommand();
            count++;    // count MUST be incremented after the command, because it is the iterator and must start at 0
            if (count >= path.Count)
            {
                // Reset values
                count = 0;
                path.Clear();
                timer.Stop();

                // Stop the sphero
                path.Add(new Point(0, 0));
                SendRollCommand();
                path.Clear();
            }
        }



        //! @brief  handle the user trying to draw somethingt
        private void PointerPressed(object sender, PointerRoutedEventArgs args)
        {
            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(null);
            if (pointer.Properties.IsLeftButtonPressed)
            {
                initialPoint = new Point(pointer.RawPosition.X - m_translateTransform.X, pointer.RawPosition.Y - m_translateTransform.Y);
                args.Handled = true;
                m_pathControl.CapturePointer(args.Pointer);

                // initialize the ink stroke
                inkStroke = new Windows.UI.Xaml.Shapes.Polyline()
                {
                    Stroke = new SolidColorBrush(Colors.Blue),
                    StrokeThickness = 5
                };
                inkStroke.Points.Add(pointer.Position);
                m_board.Children.Add(inkStroke);
            }
        }

        //! @brief  handle the user driving
        private void PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(null);

            // Move the path cursor
            if (pointer.Properties.IsLeftButtonPressed)
            {
                Point newPoint = pointer.RawPosition;
                Point delta = new Point(
                    newPoint.X - initialPoint.X,
                    newPoint.Y - initialPoint.Y);

                m_translateTransform.X = delta.X;
                m_translateTransform.Y = delta.Y;
                args.Handled = true;
            }

            // update the path list
            if (previousPoint == null) previousPoint = pointer.Position;

            float x = (float)(pointer.Position.X - previousPoint.X);
            float y = (float)(pointer.Position.Y - previousPoint.Y);

            magDiff += calculateMagnitude(x, y);

            if (!path.Contains(pointer.Position) && magDiff > MAX_DIFF)
            {
                path.Add(pointer.Position);
                magDiff = 0;
                //colors.Add()
            }

            // draw the ink, and make sure you've pressed down firmly
            if (inkStroke != null) inkStroke.Points.Add(pointer.Position);

            previousPoint = pointer.Position;
        }

        private void PointerReleased(object sender, PointerRoutedEventArgs args)
        {
            m_pathControl.ReleasePointerCapture(args.Pointer);
            args.Handled = true;

            timer.Start();  // Temp...this will get moved to the play button

            inkStroke.Points.Add(args.GetCurrentPoint(null).Position);
        }

        /*!
         * @brief	sends a roll command to the sphero given the current translation
         */
        private void SendRollCommand()
        {
            if (path.Count == 0)
            {
                return;
            }
            currentPoint = path.ElementAt(count);
            if (count == 0) previousPoint = currentPoint;
            else previousPoint = path.ElementAt(count - 1);

            // Luckily, the iterator DOES retain the order

            float x = (float)(currentPoint.X - previousPoint.X);
            float y = (float)(currentPoint.Y - previousPoint.Y);

            float distance = calculateMagnitude(x, y);
            int degreesCapped = calculateDegrees(x, y);

            float speed = .25f;    // speed of 1.0 is 7 ft/sec
            if (distance < .5) speed = 0.0f;

            // Send roll commands and limit to 10 Hz
            //long milliseconds = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
                
            m_sphero.Roll(degreesCapped, speed);

            //float x = (float)m_translateTransform.X;
            //float y = (float)m_translateTransform.Y;

            //m_lastHeading = degreesCapped;
        }

        private float calculateMagnitude (float x, float y)
        {
            float distance = x * x + y * y;
            distance = (distance == 0) ? 0 : (float)Math.Sqrt(distance);
            return distance;
        }

        private int calculateDegrees(float x, float y)
        {
            double rad = Math.Atan2((double)y, (double)x);
            rad += Math.PI / 2.0;
            double degrees = rad * 180.0 / Math.PI;
            return (((int)degrees) + 360) % 360;
        }
    }
}
