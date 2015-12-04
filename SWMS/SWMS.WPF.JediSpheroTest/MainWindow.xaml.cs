using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SWMS.Core;
using SWMS.Core.JediSphero;

namespace SWMS.WPF.JediSpheroTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty MousePointXProperty = DependencyProperty.Register(
           "MousePositionX", typeof(double), typeof(MainWindow), new PropertyMetadata(default(double)));
        public static readonly DependencyProperty MousePointYProperty = DependencyProperty.Register(
            "MousePositionY", typeof(double), typeof(MainWindow), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty SpherPositionXProperty = DependencyProperty.Register(
           "SpheroPositionX", typeof(double), typeof(MainWindow), new PropertyMetadata(default(double)));

        public static readonly DependencyProperty SpheroPositionYProperty = DependencyProperty.Register(
           "SpheroPositionY", typeof(double), typeof(MainWindow), new PropertyMetadata(default(double)));

        public double MousePositionX
        {
            get { return (double)GetValue(MousePointXProperty); }
            set { SetValue(MousePointXProperty, value); }
        }

        public double MousePositionY
        {
            get { return (double)GetValue(MousePointYProperty); }
            set { SetValue(MousePointYProperty, value); }
        }

        public double SpheroPositionX
        {
            get { return (double)GetValue(SpherPositionXProperty); }
            set { SetValue(SpherPositionXProperty, value); }
        }

        public double SpheroPositionY
        {
            get { return (double)GetValue(SpheroPositionYProperty); }
            set { SetValue(SpheroPositionYProperty, value); }
        }

        private JediSphero _device;
        public bool IsCaptured { get; set; }


        private void SearchAndConnectSpheroButton_OnClick(object sender, RoutedEventArgs e)
        {
            Task.Run(() => SearchSphero());
        }

        private int oldAngle;

        private void SpheroPositionGrid_OnMouseUp(object sender, MouseButtonEventArgs e)
        {
            IsCaptured = false;
        }

        private void SpheroPositionGrid_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!IsCaptured)
            {
                return;
            }

            var position = e.GetPosition(SpheroCanvas);
            MousePositionX = position.X;
            MousePositionY = position.Y;

            if (_device == null)
            {
                return;
            }

            // grid 500x500
            int scale = (500 / 2);

            if (!_device.IsInitialized)
            {
                _device.SetPosition(MousePositionX / scale, MousePositionY / scale);
                SpheroPositionX = _device.CurrentX * scale;
                SpheroPositionY = _device.CurrentY * scale;
                Canvas.SetLeft(CurrentPoint, MousePositionX);
                Canvas.SetTop(CurrentPoint, MousePositionY);
            }
            else
            {
                var currentPoint = _device.CurrentPoint;
                var nextPoint = _device.GetNextPoint();
                var destinationPoint = _device.GetDestinationPosition();

                Canvas.SetLeft(CurrentPoint, currentPoint.X * scale);
                Canvas.SetTop(CurrentPoint, currentPoint.Y * scale);

                Canvas.SetLeft(NextPoint, nextPoint.X * scale);
                Canvas.SetTop(NextPoint, nextPoint.Y * scale);

                Canvas.SetLeft(DestinationPoint, destinationPoint.X * scale);
                Canvas.SetTop(DestinationPoint, destinationPoint.Y * scale);
                
                _device.MoveTo(MousePositionX / scale, MousePositionY / scale);

                Canvas.SetLeft(MousePointEllipse, MousePositionX);
                Canvas.SetTop(MousePointEllipse, MousePositionY);
            }
        }

        private void SpheroPositionGrid_OnMouseDown(object sender, MouseButtonEventArgs e)
        {
            IsCaptured = true;
        }

        private void BeginConfigureButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.BeginConfiguration();
            }
        }

        private void AngleConfigure_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (_device != null)
            {
                int angle = (int)e.NewValue;
                _device.SetConfigurationAngle(angle);
            }
        }

        private void SpeedScaleConfigure_OnValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {

            if (_device != null)
            {
                double speedScale = e.NewValue / 255.0;
                _device.SetSpeedScale(speedScale);
            }
        }

        private void EndConfigureButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (_device != null)
            {
                _device.EndConfiguration();
            }
        }

        private async Task SearchSphero()
        {
            _device = await SpheroManager.GetSpheroAsync();

            if (_device != null)
            {
                MessageBox.Show("Sphero connected!");
            }
            else
            {
                MessageBox.Show("Sphero not found!");
            }
        }
    }
}
