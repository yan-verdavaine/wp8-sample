using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ImagingSDKFIlterTemplate.Recipe;
using Windows.Phone.Media.Capture;
using System.Threading;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate
{
    public partial class RealTime : PhoneApplicationPage
    {
        private void FilterParam_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //update factory param value with slider value
            RecipeFactory.Current.Param = e.NewValue;
            UpdateEffect();

        }


        #region internal
        public RealTime()
        {
            InitializeComponent();
            Loaded += (s, e) =>
           {


               DispatcherTimer timer = new DispatcherTimer();
               timer.Tick += (ss, ee) =>
               {
                   const string total = "DeviceTotalMemory";
                   const string current = "ApplicationCurrentMemoryUsage";
                   const string peak = "ApplicationPeakMemoryUsage";
                   var currentBytes = ((long)DeviceExtendedProperties.GetValue(current)) / 1024.0 / 1024.0;
                   var txt = string.Format("Memory  = {0,5:F} MB / {1,5:F} MB\n", currentBytes, DeviceStatus.ApplicationMemoryUsageLimit / 1024 / 1024);
                   displayInfo.Text = txt + _fps;


               };
               timer.Interval = new TimeSpan(0, 0, 0, 0, 40);
               timer.Start();
           };

        }

        private MediaElement _mediaElement = null;
        private PhotoCaptureDevice _photoCaptureDevice = null;
        private CameraStreamSource _cameraStreamSource = null;
        private Semaphore _cameraSemaphore = new Semaphore(1, 1);
        private CameraSensorLocation _cameraLocation = CameraSensorLocation.Back;
        private String _fps;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            FilterParam.Value = RecipeFactory.Current.Param;
            Initialize();
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            while (!_cameraSemaphore.WaitOne(100)) ;

            Uninitialize();

            _cameraSemaphore.Release();
        }

        protected override void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            base.OnOrientationChanged(e);

            AdjustOrientation();
        }

        private void AdjustOrientation()
        {
            if (_photoCaptureDevice != null)
            {
                double canvasAngle;

                if (Orientation.HasFlag(PageOrientation.LandscapeLeft))
                {
                    canvasAngle = _photoCaptureDevice.SensorRotationInDegrees - 90;
                }
                else if (Orientation.HasFlag(PageOrientation.LandscapeRight))
                {
                    canvasAngle = _photoCaptureDevice.SensorRotationInDegrees + 90;
                }
                else // PageOrientation.PortraitUp
                {
                    canvasAngle = _photoCaptureDevice.SensorRotationInDegrees;
                }



                var tmptransform = new RotateTransform() { Angle = canvasAngle };
                var previewSize = tmptransform.TransformBounds(
                    new System.Windows.Rect(
                        new System.Windows.Point(),
                        new System.Windows.Size(_photoCaptureDevice.PreviewResolution.Width, _photoCaptureDevice.PreviewResolution.Height)
                        )
                );



                double s1 = viewfinderCanvas.ActualWidth / previewSize.Width;
                double s2 = viewfinderCanvas.ActualHeight / previewSize.Height;

                //video center match Viewfinder canvas center center
                BackgroundVideoBrush.AlignmentX = AlignmentX.Center;
                BackgroundVideoBrush.AlignmentY = AlignmentY.Center;

                //Don't use a strech strategie.
                BackgroundVideoBrush.Stretch = Stretch.None;


                double scale = Math.Max(s1, s2); //UniformFill
                // double scale = Math.Min(s1, s2); // Uniform


                if (_photoCaptureDevice.SensorLocation == CameraSensorLocation.Back)
                {
                    BackgroundVideoBrush.Transform = new CompositeTransform() { Rotation = canvasAngle, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2, ScaleX = scale, ScaleY = scale };
                }
                else
                {
                    //Front viewfinder need to be flipped
                    BackgroundVideoBrush.Transform = new CompositeTransform() { Rotation = canvasAngle, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2, ScaleX = -scale, ScaleY = scale };
                }


                _photoCaptureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, canvasAngle);
            }
        }




        private void UpdateEffect()
        {
            if (_cameraStreamSource != null)
                _cameraStreamSource.UpdateEffect();
        }

        private async Task Initialize()
        {


            //  var camera = CameraSensorLocation.Back;
            //  var camera = CameraSensorLocation.Front;

            var resolution = PhotoCaptureDevice.GetAvailablePreviewResolutions(_cameraLocation).Last();

            _photoCaptureDevice = await PhotoCaptureDevice.OpenAsync(_cameraLocation, resolution);

            await _photoCaptureDevice.SetPreviewResolutionAsync(resolution);



            _cameraStreamSource = new CameraStreamSource(_photoCaptureDevice, resolution);
            _cameraStreamSource.FrameRateChanged += CameraStreamSource_FPSChanged;

            _mediaElement = new MediaElement();
            _mediaElement.BufferingTime = new TimeSpan(0);
            _mediaElement.SetSource(_cameraStreamSource);

            // Using VideoBrush in XAML instead of MediaElement, because otherwise
            // CameraStreamSource.CloseMedia() does not seem to be called by the framework:/

            BackgroundVideoBrush.SetSource(_mediaElement);



            AdjustOrientation();
        }

        private void Uninitialize()
        {


            if (_mediaElement != null)
            {
                _mediaElement.Source = null;
                _mediaElement = null;
            }
            
            if (_cameraStreamSource != null)
            {
                _cameraStreamSource.FrameRateChanged -= CameraStreamSource_FPSChanged;
                _cameraStreamSource = null;
            }



            if (_photoCaptureDevice != null)
            {
                _photoCaptureDevice.Dispose();
                _photoCaptureDevice = null;
            }
        }

        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/AboutPage.xaml", UriKind.Relative));
        }



        private void CameraStreamSource_FPSChanged(object sender, int e)
        {
           _fps = String.Format("FPS : {0}", e);
        }

        private async void LayoutRoot_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_cameraSemaphore.WaitOne(100))
            {
                await _photoCaptureDevice.FocusAsync();

                _cameraSemaphore.Release();
            }
        }

        private async void ApplicationBarIconButton_Switch(object sender, EventArgs e)
        {

            if (PhotoCaptureDevice.AvailableSensorLocations.Contains(CameraSensorLocation.Front))
            {
                if (_cameraSemaphore.WaitOne(100))
                {
                    _cameraLocation = _cameraLocation == CameraSensorLocation.Back ? CameraSensorLocation.Front : CameraSensorLocation.Back;
                    Uninitialize();
                   await  Initialize();

                    _cameraSemaphore.Release();
                }
            }
        }

        private void ApplicationBarIconButton_Info(object sender, EventArgs e)
        {
            displayInfo.Visibility = displayInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }
        #endregion
    }
}