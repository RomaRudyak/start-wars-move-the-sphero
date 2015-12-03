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
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        public JediSphero Device { get; set; }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _sensor = KinectSensor.GetDefault();

            _sensor.IsAvailableChanged += IsAvailableChanged;

            FrameDescription colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            KinectViewSpace.Source = _colorBitmap;

            _multiReader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Body);

            _multiReader.MultiSourceFrameArrived += FrameArrived;

            _sensor.Open();

            UpdateKinectStaus(_sensor.IsAvailable);

            GetSphero();
        }

        private void GetSphero()
        {
            if (Device != null)
            {
                return;
            }

            Task.Factory.StartNew(async () =>
            {
                while (Device == null)
                {
                    Device = await SpheroManager.GetSpheroAsync();
                    if (Device == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    Dispatcher.Invoke(() => SpheroName.Content = String.Format("Connected: {0}", Device.Name));
                }
            });
        }

        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiframe = e.FrameReference.AcquireFrame();
            DrawCameraView(multiframe);
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
            var scale = 30;
            var offset = 145;
            Canvas.SetLeft(obj, offset + x * scale);
            Canvas.SetTop(obj, offset + y * scale);
        }

        private void DrawCameraView(MultiSourceFrame multiframe)
        {
            using (ColorFrame colorFrame = multiframe.ColorFrameReference.AcquireFrame())
            {
                if (colorFrame != null)
                {
                    FrameDescription colorFrameDescription = colorFrame.FrameDescription;

                    using (KinectBuffer colorBuffer = colorFrame.LockRawImageBuffer())
                    {
                        this._colorBitmap.Lock();

                        // verify data and write the new color frame data to the display bitmap
                        if ((colorFrameDescription.Width == this._colorBitmap.PixelWidth) && (colorFrameDescription.Height == this._colorBitmap.PixelHeight))
                        {
                            colorFrame.CopyConvertedFrameDataToIntPtr(
                                this._colorBitmap.BackBuffer,
                                (uint)(colorFrameDescription.Width * colorFrameDescription.Height * 4),
                                ColorImageFormat.Bgra);

                            this._colorBitmap.AddDirtyRect(new Int32Rect(0, 0, this._colorBitmap.PixelWidth, this._colorBitmap.PixelHeight));
                        }

                        this._colorBitmap.Unlock();
                    }
                }
            }
        }

        private void IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            UpdateKinectStaus(e.IsAvailable);
        }

        private void UpdateKinectStaus(Boolean isAvailable)
        {
            KinectStatus.Content = isAvailable
                ? "Connected"
                : "Disconnected";
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            if (Device != null)
            {
                // TODO disconnect sphero
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
            if (Device == null)
            {
                return;
            }

            Device.BeginConfigure();
            Device.SetConfigureAngle(0);
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
            Device.EndConfigure();
            InitializeJedi();
        }

        private void InitializeJedi()
        {
            if (_jedi != null)
            {
                _jedi.Dispose();
                _jedi = null;
            }
            _jedi = new JediGestureRecognizer(_sensor);

            _jedi.ForceApplying += JediForceApplying;
            _jedi.ForceDispel += JediForceDispel;
            _isInitializationState = true;
        }

        private void JediForceDispel(object obj)
        {
        }

        private void JediForceApplying(object arg1, PointF point)
        {
            // For reversing matrix
            point.Y = -point.Y;

            if (_isInitializationState)
            {
                Device.SetConfigurePosition(point.X, point.Y);
                _lastPoint = point;
                _isInitializationState = false;
                return;
            } 
            
            if (CoordinateHelper.GetDistance(_lastPoint, point) >= 0.05F)
            {
                Device.MoveTo(point.X, point.Y);
            }
            
            SetPosition(ProectionPoint, point.X, -point.Y);
            _lastPoint = point;
        }

        private void SpheroAngle_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (Device == null)
            {
                return;
            }
            var angleValue = e.NewValue % 360;
            Debug.WriteLine("Speed angle value: {0}", angleValue);
            Device.SetConfigureAngle((int)angleValue);
        }

        private PointF _lastPoint;
        private WriteableBitmap _colorBitmap;
        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private CoordinateMapper _coordinateMapper;
        private JediGestureRecognizer _jedi;
        private Boolean _isInitializationState = true;
    }
}
