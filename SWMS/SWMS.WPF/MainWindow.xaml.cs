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
            this.Loaded += MainWindow_Loaded;
            this.Closed += MainWindow_Closed;
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _sensor.Close();
        }

        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeSceen();
        }

        private void InitializeSceen()
        {
            _sensor = KinectSensor.GetDefault();
            _sensor.Open();

            FrameDescription colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            // create the bitmap to display
            this._colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);
            this.Image.Source = this._colorBitmap;


            _jedi = new Jedi();

            // NOTE: RORU Hook up Sphero on the Jedi events
            _jedi.ForceActivated += _jedi_ForceActivated;
            // _jedi.ForceApplying += _jedi_ForceApplying;
            // _jedi.ForceDispel += _jedi_ForceDispel;

            _frameReader =  _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.Color);

            _frameReader.MultiSourceFrameArrived += _frameReader_MultiSourceFrameArrived;
        }

        #region For debbuging

        private void _jedi_ForceActivated(object sender, PointF point)
        {
            Out.Text = String.Format("Initial point: x={0:F2} y={1:F2}\n", point.X, point.Y);
            Canvas.SetLeft(this.epoint, 200 + point.X * 10);
            Canvas.SetTop(this.epoint, 200 + point.Y * 10);
        }


        #endregion For debbuging

        void _frameReader_MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            _jedi.ProcessMove(sender, e);
            Reader_ColorFrameArrived(sender, e);
        }

        private void Reader_ColorFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            // ColorFrame is IDisposable
            var mFrame = e.FrameReference.AcquireFrame();

            using (ColorFrame colorFrame = mFrame.ColorFrameReference.AcquireFrame())
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

        private KinectSensor _sensor;
        private MultiSourceFrameReader _frameReader;
        private Jedi _jedi;
        public WriteableBitmap _colorBitmap;

    }
}
