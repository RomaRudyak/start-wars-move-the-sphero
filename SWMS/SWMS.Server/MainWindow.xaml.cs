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
using System.Diagnostics;

namespace SWMS.Server
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

        public SWMS.Core.Sphero Device { get; set; }

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
        }

        // TODO create batter notification
        private async void GetSphero()
        {
            if (Device != null)
            {
                MessageBox.Show("Sphero already connected");
                return;
            }

            Device = await SpheroManager.GetSpheroAsync();
            if (Device == null)
            {
                MessageBox.Show("Sphero not found");
                return;
            }
            SpheroName.Content = String.Format("Connected: {0}", Device.Name);

            Device.BeginConfigure();
        }

        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiframe = e.FrameReference.AcquireFrame();
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

        private WriteableBitmap _colorBitmap;
        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private CoordinateMapper _coordinateMapper;
        private JediGestureRecognizer _jedi;
        private Boolean _isJediInitialization = false;

        // TODO move this methods up in file
        private void ConnectToSheroButton_OnClick(object sender, RoutedEventArgs e)
        {
            GetSphero();
        }

        private void ConfiguredButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (Device == null)
            {
                return;
            }

            double newValue = this.SpheroSpeed.Value / 255;
            Debug.WriteLine("Speed scale value: {0}", newValue);
            Device.ChangeSpeedScale(newValue);
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
        }

        private void JediForceDispel(object obj)
        {
            _isJediInitialization = false;
        }

        private void JediForceApplying(object arg1, PointF point)
        {
            if (_isJediInitialization)
	        {
                Device.SetConfigurePosition(point.X, point.Y);
                _isJediInitialization = true;
                return;
            }

            Device.MoveTo(point.X*5, point.Y*5);
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
    }
}
