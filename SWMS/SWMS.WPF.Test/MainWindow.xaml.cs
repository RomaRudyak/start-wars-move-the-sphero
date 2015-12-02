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

namespace SWMS.WPF.Test
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


            _jedi = new Jedi(_sensor);
            _jedi.ForceApplying += ForceApplying;
            _jedi.ForceDispel += ForceDispel;

            _frameReader =  _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Body | FrameSourceTypes.Depth | FrameSourceTypes.Color);

            _frameReader.MultiSourceFrameArrived +=MultiSourceFrameArrived;
        }

        private void ForceDispel(object obj)
        {
            this.epointXZ.Visibility = Visibility.Hidden;
        }

        private void MultiSourceFrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            using (var bodyFrame = e.FrameReference.AcquireFrame().BodyFrameReference.AcquireFrame())
            {
                var bodys = new Body[6];

                if (bodyFrame == null)
                {
                    return;
                }

                bodyFrame.GetAndRefreshBodyData(bodys);

                var b = bodys.FirstOrDefault(body => body.IsTracked);
                if (b == null)
                {
                    return;
                }

                var head = b.Joints[JointType.Head];
                var hand = b.Joints[JointType.HandRight];

                SetpointXZ(this.headXZ, head);
                SetpointXZ(this.heandXZ, hand);

                SetpointZY(headZY, head);
                SetpointZY(heand1ZY, hand);
            }
        }

        private void SetpointXZ(UIElement el, Joint head)
        {
            Canvas.SetLeft(el, 195 + head.Position.X * 40);
            Canvas.SetTop(el, 195 + head.Position.Z * 40);
        }

        private void SetpointZY(UIElement el, Joint head)
        {
            Canvas.SetLeft(el, 195 + head.Position.Z * 40);
            Canvas.SetTop(el, 395 + head.Position.Y * -40);
        }

        private void ForceApplying(object sender, PointF point)
        {
            Out.Text = Out.Text.Insert(0, String.Format("x={0:F2} y={1:F2}\n", point.X, point.Y));
            // XZ
            this.epointXZ.Visibility = Visibility.Visible;
            Canvas.SetLeft(this.epointXZ, 195 + point.X * 40);
            Canvas.SetTop(this.epointXZ, 195 + point.Y * 40);
        }



        private KinectSensor _sensor;
        private MultiSourceFrameReader _frameReader;
        private Jedi _jedi;
        public WriteableBitmap _colorBitmap;

    }
}
