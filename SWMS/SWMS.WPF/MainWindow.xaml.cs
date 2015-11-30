using Microsoft.Kinect;
using SWMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SWMS.WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            InitializeSceen();
        }

        private void InitializeSceen()
        {
            _sensor = KinectSensor.GetDefault();
            _jedi = new Jedi();

            // NOTE: RORU Hook up Sphero on the Jedi events
            // _jedi.ForceActivated += _jedi_ForceActivated;
            // _jedi.ForceApplying += _jedi_ForceApplying;
            // _jedi.ForceDispel += _jedi_ForceDispel;

            _frameReader =  _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Depth);

            _frameReader.MultiSourceFrameArrived += _jedi.ProcessMove;
        }

        void _frameReader_MultiSourceFrameArrived()
        {
            
        }

        private KinectSensor _sensor;
        private MultiSourceFrameReader _frameReader;
        private Jedi _jedi;
    }
}
