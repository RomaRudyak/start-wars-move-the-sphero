using Microsoft.Kinect;
using SWMS.Core;
using SWMS.Core.JediSphero;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using SWMS.Core.Helpers;
using System.Windows.Input;
using System.Diagnostics;

namespace SWMS.Configuration.ViewModels
{
    public class SceneViewModel : BindableBase, IDisposable
    {
        public String SpheroName
        {
            get { return _spheroName; }
            set
            {

                _spheroName = value;
                OnPropertyChanged();
            }
        }

        public WriteableBitmap ColorView
        {
            get { return _colorBitmap; }
            set
            {
                _colorBitmap = value;
                OnPropertyChanged();
            }
        }

        public Boolean KinecteIsAvailable {
            get { return _kinecteIsAvailable; }
            set
            {
                if (_kinecteIsAvailable == value)
                {
                    return;
                }
                _kinecteIsAvailable = value;
                OnPropertyChanged();
            }
        }

        public Point HeadXZProextion
        {
            get { return _headXZProjection; }
            set
            {
                _headXZProjection = value;
                OnPropertyChanged();
            }
        }

        public Point HandRigthXZProextion
        {
            get { return _handRigthXZProjection; }
            set
            {
                _handRigthXZProjection = value;
                OnPropertyChanged();
            }
        }

        public Point HandLeftXZProextion
        {
            get { return _handLeftXZProjection; }
            set
            {
                _handLeftXZProjection = value;
                OnPropertyChanged();
            }
        }

        public Point SpheroXZProextion
        {
            get { return _spheroXZProjection; }
            set
            {
                _spheroXZProjection = value;
                OnPropertyChanged();
            }
        }

        public Point ForceXZProextion
        {
            get { return _forceXZProjection; }
            set
            {
                _forceXZProjection = value;
                OnPropertyChanged();
            }
        }

        public Boolean IsSpheroConnected
        {
            get { return _sphero != null; }
        }

        public Boolean IsForceApplying
        {
            get { return _isForceApplying; }
            set
            {
                if (_isForceApplying == value)
                {
                    return;
                }
                _isForceApplying = value;
                OnPropertyChanged();
            }
        }

        public Boolean IsInConfigurationMode
        {
            get { return _isInConfigurationMode; }
            set {
                if (_isInConfigurationMode == value)
                {
                    return;
                }
                _isInConfigurationMode = value;
                OnPropertyChanged();
            }
        }

        public Int32 ConfigurationAngle
        {
            get { return _configurationAngle; }
            set
            {
                if (_configurationAngle == value)
                {
                    return;
                }
                _configurationAngle = value;
                OnPropertyChanged();
                if (IsInConfigurationMode)
                {
                    _sphero.SetConfigurationAngle(ConfigurationAngle);
                }
            }
        }

        public Double ConfiqurationSpeed
        {
            get { return _congiruationSpeed; }
            set
            {
                if (_congiruationSpeed == value)
                {
                    return;
                }
                _congiruationSpeed = value;
                OnPropertyChanged();
            }
        }



        public ICommand BeginConfigurationCommand
        {
            get
            {
                return _beginConfigurationCommand ?? (_beginConfigurationCommand = new Command(BeginConfiguration));
            }
        }

        public ICommand EndConfigurationCommand
        {
            get
            {
                return _endConfigurationCommand ?? (_endConfigurationCommand = new Command(EndConfiguration));
            }
        }

        public void Initialize()
        {
            _sensor = KinectSensor.GetDefault();

            _sensor.IsAvailableChanged += IsAvailableChanged;

            FrameDescription colorFrameDescription = _sensor.ColorFrameSource.CreateFrameDescription(ColorImageFormat.Bgra);

            ColorView = _colorBitmap = new WriteableBitmap(colorFrameDescription.Width, colorFrameDescription.Height, 96.0, 96.0, PixelFormats.Bgr32, null);

            _multiReader = _sensor.OpenMultiSourceFrameReader(FrameSourceTypes.Color | FrameSourceTypes.Depth | FrameSourceTypes.Body);

            _multiReader.MultiSourceFrameArrived += FrameArrived;

            _sensor.Open();

            KinecteIsAvailable = _sensor.IsAvailable;

            GetSphero();
        }

        public void Dispose()
        {
            Dispose(true);
        }
        
        public SceneViewModel()
        {
            _spheroPointTransform = new MatrixTransform()
            {
                Matrix = new Matrix
                {
                    M22 = -1
                }
            };

            ConfigurationAngle = 0;
            ConfiqurationSpeed = 155;
        }

        ~SceneViewModel()
        {
            Dispose(false);
            GC.SuppressFinalize(this);
        }


        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                RemoveGeustureRecognizerIfNeeded();

                if (_sphero != null)
                {
                    _sphero.Disconnect();
                    _sphero = null;
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
            }
        }


        private void GetSphero()
        {
            if (_sphero != null)
            {
                return;
            }

            Task.Factory.StartNew(async () =>
            {
                while (!IsSpheroConnected)
                {
                    _sphero = await SpheroManager.GetSpheroAsync();
                    if (_sphero == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    SpheroName =_sphero.Name;
                    OnPropertyChanged(this.GetPropertyName(x => x.IsSpheroConnected));
                    _sphero.SetPosition(1, -1);
                }
            });
        }

        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiframe = e.FrameReference.AcquireFrame();
            UpdateCameraView(multiframe);
            UpdateBodyPartsPosition(multiframe);
        }

        private void IsAvailableChanged(object sender, IsAvailableChangedEventArgs e)
        {
            KinecteIsAvailable = e.IsAvailable;
        }

        private void UpdateCameraView(MultiSourceFrame multiframe)
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

        private void UpdateBodyPartsPosition(MultiSourceFrame multiframe)
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


                

                HeadXZProextion = GetXZProjection(posHead.X, posHead.Z);
                HandRigthXZProextion = GetXZProjection(posHandRight.X, posHandRight.Z);
                HandLeftXZProextion = GetXZProjection(posHandLeft.X, posHandLeft.Z);
                
                if (_sphero != null)
                {
                    SpheroXZProextion = _spheroPointTransform.Transform(_sphero.CurrentPoint);
                }
            }
        }

        private Point GetXZProjection(Double x, Double y)
        {
            return new Point(x, y);
        }


        private void BeginConfiguration()
        {
            if (!IsSpheroConnected)
            {
                return;
            }

            IsInConfigurationMode = true;
            RemoveGeustureRecognizerIfNeeded();

            _sphero.BeginConfiguration();
            _sphero.SetConfigurationAngle(0);
        }

        private void EndConfiguration()
        {
            double newValue = ConfiqurationSpeed / 255;
            _sphero.SetSpeedScale(newValue);
            _sphero.EndConfiguration();
            InitializeJediTracking();
            IsInConfigurationMode = false;
        }

        private void InitializeJediTracking()
        {
            RemoveGeustureRecognizerIfNeeded();
            _jediGeustureRecognizer = new JediGestureRecognizer(_sensor);

            _jediGeustureRecognizer.ForceApplying += JediForceApplying;
            _jediGeustureRecognizer.ForceDispel += JediForceDispel;
            _isSpheroGrabed = true;
        }

        private void RemoveGeustureRecognizerIfNeeded()
        {
            if (_jediGeustureRecognizer != null)
            {
                _jediGeustureRecognizer.ForceApplying -= JediForceApplying;
                _jediGeustureRecognizer.ForceDispel -= JediForceDispel;
                _jediGeustureRecognizer.Dispose();
                _jediGeustureRecognizer = null;
            }
        }


        private void JediForceDispel(object obj)
        {

            if (_isSpheroGrabed)
            {
                _sphero.StopMove();                
            }
            _isSpheroGrabed = false;
            IsForceApplying = false;
            Debug.WriteLine("Force end");
        }

        private void JediForceApplying(object arg1, Point forcePoint)
        {
            Debug.WriteLine("Force Start");
            IsForceApplying = true;

            var p = _spheroPointTransform.Transform(forcePoint);

            if (!_isSpheroGrabed)
            {
                _sphero.SetPosition(p);
                _lastPoint = forcePoint;
                _isSpheroGrabed = true;
                return;
            }

            if (CoordinateHelper.GetDistance(_lastPoint, forcePoint) >= 0.05F)
            {
                _sphero.MoveTo(p.X, p.Y);
            }

            ForceXZProextion = GetXZProjection(forcePoint.X, forcePoint.Y);
            _lastPoint = forcePoint;
        }

        private Point _headXZProjection;
        private Point _handRigthXZProjection;
        private Point _handLeftXZProjection;
        private Point _spheroXZProjection;

        private Boolean _kinecteIsAvailable;
        private String _spheroName;
        private Boolean _isForceApplying;
        private Boolean _isInConfigurationMode;
        
        private Int32 _configurationAngle;
        private Double _congiruationSpeed;

        private Point _lastPoint;
        private Boolean _isSpheroGrabed = true;

        
        private WriteableBitmap _colorBitmap;

        private ICommand _beginConfigurationCommand;
        private ICommand _endConfigurationCommand;

        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private JediSphero _sphero;
        private MatrixTransform _spheroPointTransform;
        private JediGestureRecognizer _jediGeustureRecognizer;
        private Point _forceXZProjection;

    }
}
