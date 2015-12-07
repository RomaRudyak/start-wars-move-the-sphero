using Microsoft.Kinect;
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
using SWMS.Core;
using SWMS.Core.JediSphero;
using System.Diagnostics;
using SWMS.Core.Helpers;
using SWMS.Configuration.ViewModels;

namespace SWMS.Configuration
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            _sceneViewModel = new SceneViewModel();
            this.DataContext = _sceneViewModel;
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;

            _pointUIScaleTransform = new MatrixTransform()
            {
                Matrix = new Matrix
                {
                    M11 = 37.5,
                    M22 = 37.5,
                    OffsetX = 150,
                    OffsetY = 150
                }
            };

            _spheroPointTransform = new MatrixTransform()
            {
                Matrix = new Matrix
                {
                    M22 = -1
                }
            };
        }

        public JediSphero Device { get; set; }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {

            _multiReader.MultiSourceFrameArrived += FrameArrived;

            
        }

        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiframe = e.FrameReference.AcquireFrame();
            HandBodyPartsOnGrid(multiframe);
        }

        private void HandBodyPartsOnGrid(MultiSourceFrame multiframe)
        {
            using (var bodyFrame = multiframe.BodyFrameReference.AcquireFrame())
            {
                if (bodyFrame == null)
                {
                    return;
                }

                var bodies = new Body[bodyFrame.BodyCount];
                bodyFrame.GetAndRefreshBodyData(bodies);

                var body = bodies.FirstOrDefault(b => b.IsTracked);
                if (body == null)
                {
                    return;
                }

                var posHead = body.Joints[JointType.Head].Position;
                var posHandRight = body.Joints[JointType.HandRight].Position;
                var posHandLeft = body.Joints[JointType.HandLeft].Position;

                SetPosition(ProectionHead, posHead.X, posHead.Z);
                SetPosition(ProectionHandRight, posHandRight.X, posHandRight.Z);
                SetPosition(ProectionHandLeft, posHandLeft.X, posHandLeft.Z);
                if (Device != null)
                {
                    SetPosition(SpheroPoint, Device.CurrentX, -Device.CurrentY);
                }
            }
        }

        private void SetPosition(UIElement obj, Double x, Double y)
        {
            var p = _pointUIScaleTransform.Transform(new Point(x, y));
            Canvas.SetLeft(obj, p.X);
            Canvas.SetTop(obj, p.Y);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Device != null)
            {
                Device.Disconnect();
            }
            if (_multiReader != null)
            {
                _multiReader.Dispose();
                _multiReader = null;
            }
            if (_sensor != null)
            {
                _sensor.Close();
                _sensor = null;
            }
            if (_jedi != null)
            {
                _jedi.Dispose();
                _jedi = null;
            }
        }

        private void BeginConfigurationButton_OnClick(object sender, RoutedEventArgs e)
        {
            RemoveGeustureRecognizerIfNeeded();
            if (Device == null)
            {
                return;
            }

            Device.BeginConfiguration();
            Device.SetConfigurationAngle(0);
        }

        private void ConfiguredButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            double newValue = this.SpheroSpeed.Value / 255;
            Debug.WriteLine("Speed scale value: {0}", newValue);
            Device.SetSpeedScale(newValue);
            Device.EndConfiguration();
            InitializeJediTracking();
        }

        private void InitializeJediTracking()
        {
            RemoveGeustureRecognizerIfNeeded();
            _jedi = new JediGestureRecognizer(_sensor);

            _jedi.ForceApplying += JediForceApplying;
            _jedi.ForceDispel += JediForceDispel;
            _isSpheroGrabed = true;
        }

        private void RemoveGeustureRecognizerIfNeeded()
        {
            if (_jedi != null)
            {
                _jedi.Dispose();
                _jedi = null;
            }
        }

        private void JediForceDispel(object obj)
        {
            if (Device != null)
            {
                Device.StopMove();
            }
            _isSpheroGrabed = true;
            ProectionPoint.Visibility = Visibility.Hidden;
        }

        private void JediForceApplying(object arg1, Point forcePoint)
        {
            ProectionPoint.Visibility = Visibility.Visible;

            var p = _spheroPointTransform.Transform(forcePoint);

            if (_isSpheroGrabed)
            {
                Device.SetPosition(p);
                _lastPoint = forcePoint;
                _isSpheroGrabed = false;
                return;
            }

            if (CoordinateHelper.GetDistance(_lastPoint, forcePoint) >= 0.04F)
            {
                Device.MoveTo(p.X, p.Y);
            }
            
            SetPosition(ProectionPoint, forcePoint.X, forcePoint.Y);
            _lastPoint = forcePoint;
        }

        private void SpheroAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Device == null)
            {
                return;
            }
            var angleValue = e.NewValue % 360;
            Debug.WriteLine("Speed angle value: {0}", angleValue);
            Device.SetConfigurationAngle((int)angleValue);
        }

        private Point _lastPoint;
        private WriteableBitmap _colorBitmap;
        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private CoordinateMapper _coordinateMapper;
        private JediGestureRecognizer _jedi;
        private Boolean _isSpheroGrabed = true;
        private MatrixTransform _pointUIScaleTransform;
        private MatrixTransform _spheroPointTransform;
        private SceneViewModel _sceneViewModel;
    }
}
