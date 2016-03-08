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

        //! @brief  the color wheel to control m_robot color
        private ColorWheel m_colorwheel;

        public MainPage()
        {
            this.InitializeComponent();
        }

        private void setupRobot()
        {
            RobotProvider provider = RobotProvider.GetSharedProvider();
            provider.DiscoveredRobotEvent += OnRobotDiscovered;
            provider.NoRobotsEvent += OnNoRobotsEvent;
            //provider.ConnectedRobotEvent += OnRobotConnected;
            provider.FindRobots();
        }

        private void OnRobotDiscovered(object sender, Robot robot)
        {
            Debug.WriteLine(string.Format("Discovered \"{0}\"", robot.BluetoothName));

            if (m_robot == null)
            {
                RobotProvider provider = RobotProvider.GetSharedProvider();
                provider.ConnectRobot(robot);
                //...
            }
        }

        private void OnNoRobotsEvent(object sender, EventArgs e)
        {
            MessageDialog dialog = new MessageDialog("No Sphero Paired");
            //...
        }

        public void changeSpheroColor(int red, int green, int blue)
        {
            m_robot.SetRGBLED(red, green, blue);
        }
    }
}
