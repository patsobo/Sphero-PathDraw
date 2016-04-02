using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI;
using Windows.UI.Input.Inking;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

using RobotKit;

namespace PathDraw
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        Sphero m_robot = null;

        // Controls attritubutes of the InkBoard like color, size, etc.
        InkDrawingAttributes attr = new InkDrawingAttributes();

        //! @brief  the color wheel to control m_robot color
        private ColorWheel m_colorwheel;

        // The special InkBoard
        private PathBoard m_board;

        public MainPage()
        {
            this.InitializeComponent();

            attr.Color = Colors.Red;
            attr.IgnorePressure = true;
            attr.PenTip = PenTipShape.Circle;
            attr.Size = new Size(10, 10);
            
            //attr.PenTipTransform = System.Numerics.Matrix3x2.CreateRotation((float)(70 * Math.PI / 180));
            //InkBoard.InkPresenter.UpdateDefaultDrawingAttributes(attr);

            // testing out using a cursor;
            //InkPresenterPredefinedConfiguration config = new InkPresenterPredefinedConfiguration();

            // let inkBoard be used with any input type
            //InkBoard.InkPresenter.InputDeviceTypes = Windows.UI.Core.CoreInputDeviceTypes.Mouse |
            //    Windows.UI.Core.CoreInputDeviceTypes.Touch | Windows.UI.Core.CoreInputDeviceTypes.Pen;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            SetupRobotConnection();
            Application app = Application.Current;
            app.Suspending += OnSuspending;
        }

        /*!
         * @brief   handle the user launching this page in the application
         * 
         *  connects to sphero and sets up the ui
         */
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            ShutdownRobotConnection();
            ShutdownControls();

            Application app = Application.Current;
            app.Suspending -= OnSuspending;
        }

        //! @brief  handle the application entering the background
        private void OnSuspending(object sender, SuspendingEventArgs args)
        {
            ShutdownRobotConnection();
        }

        //  just color wheel for now
        private void SetupControls()
        {
            Debug.WriteLine("Setting up controls...");
            m_colorwheel = new ColorWheel(ColorPuck);
            m_board = new PathBoard(m_robot, PathCursor, InkBoard, attr);
        }

        //! @brief  shuts down the various sphero controls
        private void ShutdownControls()
        {
            m_colorwheel = null;
            m_board = null;
        }

        //! @brief  search for a robot to connect to
        private void SetupRobotConnection()
        {
            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            provider.NoRobotsEvent += OnNoRobotsEvent;
            provider.ConnectedRobotEvent += OnRobotConnected;
            provider.FindRobots();
        }

        //! @brief  disconnect from the robot and stop listening
        private void ShutdownRobotConnection()
        {
            if (m_robot != null)
            {
                m_robot.SensorControl.StopAll();
                m_robot.Sleep();

                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.DiscoveredRobotEvent -= OnRobotDiscovered;
                provider.NoRobotsEvent -= OnNoRobotsEvent;
                provider.ConnectedRobotEvent -= OnRobotConnected;
            }
        }

        //! @brief  when a robot is discovered, connect!
        private void OnRobotDiscovered(object sender, Robot robot)
        {
            Debug.WriteLine(string.Format("Discovered \"{0}\"", robot.BluetoothName));

            if (m_robot == null)
            {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                m_robot = (Sphero)robot;
            }
        }

        //! @brief  when a robot is connected, get ready to drive!
        private void OnRobotConnected(object sender, Robot robot)
        {
            Debug.WriteLine(string.Format("Connected to {0}", robot));

            SetupControls();

            // these two are probably useless for my app
            m_robot.SensorControl.Hz = 10;
            m_robot.CollisionControl.StartDetectionForWallCollisions();

        }

        private void OnNoRobotsEvent(object sender, EventArgs e)
        {
            MessageDialog dialog = new MessageDialog("No Sphero Paired");
            //...
        }
    }
}
