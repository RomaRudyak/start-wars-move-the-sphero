﻿using Microsoft.Kinect;
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

namespace SWMS.Configuration.ViewModels
{
    internal class SceneViewModel : BindableBase
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
            get { return _headXZProection; }
            set { _headXZProection = value; }
        }

        public Point HandRigthXZProextion
        {
            get { return _handRigthXZProection; }
            set { _handRigthXZProection = value; }
        }

        public Point HandLeftXZProextion
        {
            get { return _handLeftXZProection; }
            set { _handLeftXZProection = value; }
        }

        public Point SpheroXZProextion
        {
            get { return _spheroXZProection; }
            set { _spheroXZProection = value; }
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


        private void GetSphero()
        {
            if (_sphero != null)
            {
                return;
            }

            Task.Factory.StartNew(async () =>
            {
                while (_sphero == null)
                {
                    _sphero = await SpheroManager.GetSpheroAsync();
                    if (_sphero == null)
                    {
                        await Task.Delay(1000);
                        continue;
                    }
                    SpheroName =_sphero.Name;
                }
            });
        }


        private void FrameArrived(object sender, MultiSourceFrameArrivedEventArgs e)
        {
            var multiframe = e.FrameReference.AcquireFrame();
            UpdateCameraView(multiframe);
            HandBodyPartsOnGrid(multiframe);
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


                

                HeadXZProextion = GetXZProection(posHead.X, posHead.Z);
                HandRigthXZProextion = GetXZProection(posHandRight.X, posHandRight.Z);
                HandLeftXZProextion = GetXZProection(posHandLeft.X, posHandLeft.Z);
                
                if (_sphero != null)
                {
                    SpheroXZProextion = GetXZProection(_sphero.CurrentX, -_sphero.CurrentY);
                }
            }
        }

        private Point GetXZProection(Double x, Double y)
        {
            return new Point(x, y);
        }


        private Point _headXZProection;
        private Point _handRigthXZProection;
        private Point _handLeftXZProection;
        private Point _spheroXZProection;

        private Boolean _kinecteIsAvailable;
        private String _spheroName;
        
        private WriteableBitmap _colorBitmap;

        private KinectSensor _sensor;
        private MultiSourceFrameReader _multiReader;
        private JediSphero _sphero;

    }
}
