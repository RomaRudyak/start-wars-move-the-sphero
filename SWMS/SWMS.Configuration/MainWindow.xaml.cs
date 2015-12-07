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

            _sceneViewModel.PropertyChanged += SceneViewModelPropertyChanged;

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
        }
        
        private void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            _sceneViewModel.Initialize();
        }

        private void SceneViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == _sceneViewModel.GetPropertyName(x => x.SpheroXZProextion))
            {
                SetPosition(SpheroPoint, _sceneViewModel.SpheroXZProextion);
            }
            else if (e.PropertyName == _sceneViewModel.GetPropertyName(x => x.ForceXZProextion)) {
                SetPosition(ProjectionPoint, _sceneViewModel.ForceXZProextion);
            }
            else if (e.PropertyName == _sceneViewModel.GetPropertyName(x => x.HeadXZProextion))
            {
                SetPosition(ProjectionHead, _sceneViewModel.HeadXZProextion);
            }
            else if (e.PropertyName == _sceneViewModel.GetPropertyName(x => x.HandRigthXZProextion))
            {
                SetPosition(ProjectionHandRight, _sceneViewModel.HandRigthXZProextion);
            }
            else if (e.PropertyName == _sceneViewModel.GetPropertyName(x => x.HandLeftXZProextion))
            {
                SetPosition(ProjectionHandLeft, _sceneViewModel.HandLeftXZProextion);
            }
            else if (e.PropertyName == _sceneViewModel.GetPropertyName(x=> x.IsForceApplying))
            {
                ProjectionPoint.Visibility = _sceneViewModel.IsForceApplying
                ? Visibility.Visible
                : Visibility.Hidden;
            }
        }
        
        private void SetPosition(UIElement obj, Point point)
        {
            var p = _pointUIScaleTransform.Transform(point);
            Canvas.SetLeft(obj, p.X);
            Canvas.SetTop(obj, p.Y);
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            _sceneViewModel.Dispose();
        }


        private SceneViewModel _sceneViewModel;
        private MatrixTransform _pointUIScaleTransform;
    }
}
