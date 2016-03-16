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

namespace PathDraw.RobotKitUI
{
    class PathBoard
    {
        const int TIMER = 10;

        // the board being drawn on.
        private InkCanvas m_board;

        // collects a number of points with corresponding colors
        List<Point> path;
        List<Color> colors;
        int count;

        // cursor that controls the current path spot.  Can only draw when you're touching this element
        private FrameworkElement m_pathControl;

        public PathBoard(InkCanvas board)
        {
            m_board = board;
            count = 0;

            m_board.PointerMoved += PointerMoved;
            m_board.PointerPressed+= PointerPressed;
            m_board.PointerReleased+= PointerReleased;
            //m_board.ReleasePointerCapture();
            //m_board.PointerCa
        }

        //! @brief  handle the user trying to draw somethingt
        private void PointerPressed(object sender, PointerRoutedEventArgs args)
        {
            count = 0;
            // when pressed, check to see if you're on the cursor.  If so, then allow drawing.

            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(null);
            if (pointer.Properties.IsLeftButtonPressed)
            {
                //m_initialPoint = new Point(pointer.RawPosition.X - m_translateTransform.X, pointer.RawPosition.Y - m_translateTransform.Y);
                //args.Handled = true;
                //m_puckControl.CapturePointer(args.Pointer);
            }
        }

        //! @brief  handle the user driving
        private void PointerMoved(object sender, PointerRoutedEventArgs args)
        {
            Windows.UI.Input.PointerPoint pointer = args.GetCurrentPoint(null);

            count++;
            if (count == TIMER)
            {
                path.Add(pointer.Position);
                count = 0;
            }
        }

        private void PointerReleased(object sender, PointerRoutedEventArgs args)
        {
            count = 0;
        }
    }
}
