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
using Windows.Storage.Streams;
using System.IO;
using Nokia.Graphics.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework.Media;

namespace ImagingSDKFIlterTemplate
{
    public partial class RealTime : PhoneApplicationPage
    {
        private void FilterParam_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            //update factory param value with slider value
            RecipeFactory.Current.Param = e.NewValue;
            //idication recipe should be updated
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
                   if (_cameraStreamSource != null)
                       txt += string.Format("FPS  = {0,5:F}\n", _cameraStreamSource.FPS);
                   displayInfo.Text = txt;

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
                var t = new TransformGroup();

                if (_cameraLocation == CameraSensorLocation.Front)
                {
                    t.Children.Add(new CompositeTransform() { Rotation = -canvasAngle, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2, ScaleX = scale, ScaleY = scale });
                    t.Children.Add(new ScaleTransform() { ScaleX = -1, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2 });
                }
                else
                {
                    t.Children.Add(new CompositeTransform() { Rotation = canvasAngle, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2, ScaleX = scale, ScaleY = scale });
                }
                BackgroundVideoBrush.Transform = t;

            }
        }




        private void UpdateEffect()
        {
            if (_cameraStreamSource != null)
                _cameraStreamSource.UpdateEffect();
        }

        private async Task Initialize()
        {


            var resolution = PhotoCaptureDevice.GetAvailableCaptureResolutions(_cameraLocation).First();

            _photoCaptureDevice = await PhotoCaptureDevice.OpenAsync(_cameraLocation, resolution);

            Windows.Foundation.Size PreviewResolution;
            foreach (var res in PhotoCaptureDevice.GetAvailablePreviewResolutions(_cameraLocation).ToArray().Reverse())
            {
                try
                {
                    await _photoCaptureDevice.SetPreviewResolutionAsync(res);
                    PreviewResolution = res;
                    break;

                }
                catch (Exception e)
                {
                }
            }



            _cameraStreamSource = new CameraStreamSource(_photoCaptureDevice, PreviewResolution);

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

        private async void canvas_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (_cameraSemaphore.WaitOne(100))
            {

                try
                {
                    //compute vector between preview picture center and Inverted transformation center
                    var tmp = BackgroundVideoBrush.Transform.Inverse.TransformBounds(new Rect(new Point(), viewfinderCanvas.RenderSize));
                    var dx = _photoCaptureDevice.PreviewResolution.Width / 2 - (tmp.X + tmp.Width / 2);
                    var dy = _photoCaptureDevice.PreviewResolution.Height / 2 - (tmp.Y + tmp.Height / 2);

                    //invert tap position
                    var p = e.GetPosition(this);
                    var pInPreview = BackgroundVideoBrush.Transform.Inverse.Transform(p);

                    //transform inverted position to picture reference
                    double X = pInPreview.X + dx;
                    double Y = pInPreview.Y + dy;

                    if (X < 0) X = 0;
                    if (X >= _photoCaptureDevice.PreviewResolution.Width) X = _photoCaptureDevice.PreviewResolution.Width - 1;

                    if (Y >= _photoCaptureDevice.PreviewResolution.Height) Y = _photoCaptureDevice.PreviewResolution.Height - 1;
                    if (Y < 0) Y = 0;

                    _photoCaptureDevice.FocusRegion = new Windows.Foundation.Rect(
                        new Windows.Foundation.Point(X, Y),
                        new Windows.Foundation.Size());


                }
                catch (Exception ee)
                {

                }

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
                    await Initialize();

                    _cameraSemaphore.Release();
                }
            }
        }

        private void ApplicationBarIconButton_Info(object sender, EventArgs e)
        {
            displayInfo.Visibility = displayInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }


        public async Task<IBuffer> TakePicture()
        {
            if (_photoCaptureDevice == null && _cameraSemaphore.WaitOne(100))
                return null;

            if (_cameraSemaphore.WaitOne(100))
            {
                try
                {
                    int angle = 0;

                    if (Orientation.HasFlag(PageOrientation.LandscapeLeft))
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees - 90;
                    }
                    else if (Orientation.HasFlag(PageOrientation.LandscapeRight))
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees + 90;
                    }
                    else // PageOrientation.PortraitUp
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees;
                    }


                    if (angle < 0) angle += 360;
                    if (_cameraLocation == CameraSensorLocation.Back)
                    {
                        _photoCaptureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, angle);
                    }
                    else
                    {
                        _photoCaptureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, -angle);
                    }
                    _photoCaptureDevice.SetProperty(KnownCameraGeneralProperties.SpecifiedCaptureOrientation, 0);


                    var cameraCaptureSequence = _photoCaptureDevice.CreateCaptureSequence(1);
                    var stream = new MemoryStream();
                    cameraCaptureSequence.Frames[0].CaptureStream = stream.AsOutputStream();
                    await _photoCaptureDevice.PrepareCaptureSequenceAsync(cameraCaptureSequence);
                    await cameraCaptureSequence.StartCaptureAsync();


                    IBuffer capturedPicture;
                    if (_cameraLocation == CameraSensorLocation.Back)
                    {
                        capturedPicture = stream.GetWindowsRuntimeBuffer();
                    }
                    else
                    {
                        capturedPicture = await JpegTools.FlipAndRotateAsync(stream.GetWindowsRuntimeBuffer(), FlipMode.Horizontal, Rotation.Rotate0, JpegOperation.AllowLossy);
                    }


                    using (var source = new StreamImageSource(capturedPicture.AsStream()))
                    {
                        var recipe = RecipeFactory.Current.CreatePipeline(source);
                        using (var renderer = new JpegRenderer(recipe))
                        {
                            capturedPicture = await renderer.RenderAsync();
                        }
                        if (recipe is IDisposable)
                            (recipe as IDisposable).Dispose();
                    }
                    return capturedPicture;
                }
                finally
                {
                    _cameraSemaphore.Release();
                }
            }

            return null;
        }



        public async Task<IBuffer> TakePictureFast()
        {
            if (_photoCaptureDevice == null && _cameraSemaphore.WaitOne(100))
                return null;

            if (_cameraSemaphore.WaitOne(100))
            {
                try
                {
                    int angle = 0;

                    if (Orientation.HasFlag(PageOrientation.LandscapeLeft))
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees - 90;
                    }
                    else if (Orientation.HasFlag(PageOrientation.LandscapeRight))
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees + 90;
                    }
                    else // PageOrientation.PortraitUp
                    {
                        angle = (int)_photoCaptureDevice.SensorRotationInDegrees;
                    }

                    int layersize = (int)(_photoCaptureDevice.PreviewResolution.Width * _photoCaptureDevice.PreviewResolution.Height);
                    int layersizeuv = layersize / 2;
                    var buffer = new byte[layersize + layersizeuv];
                    _photoCaptureDevice.GetPreviewBufferYCbCr(buffer);
                    IBuffer capturedPicture;
                    using (var cameraBitmap = new Bitmap(
                    _photoCaptureDevice.PreviewResolution,
                    ColorMode.Yuv420Sp,
                    new uint[] { (uint)_photoCaptureDevice.PreviewResolution.Width, (uint)_photoCaptureDevice.PreviewResolution.Width },
                    new IBuffer[] { buffer.AsBuffer(0, layersize), buffer.AsBuffer(layersize, layersizeuv) }))
                    using (var source = new BitmapImageSource(cameraBitmap))
                    using (var orientationffect = new FilterEffect(source))
                    {
                        if(_cameraLocation == CameraSensorLocation.Back)
                        {
                            orientationffect.Filters = new IFilter[]{new RotationFilter(angle)};

                        }
                        else
                        {
                            orientationffect.Filters = new IFilter[] { new RotationFilter(-angle), new FlipFilter(FlipMode.Horizontal) };
                        }

                        var recipe = RecipeFactory.Current.CreatePipeline(orientationffect);
                        using (var renderer = new JpegRenderer(recipe))
                        {



                            capturedPicture = await renderer.RenderAsync();
                        }
                        if (recipe is IDisposable)
                            (recipe as IDisposable).Dispose();
                }

                    return capturedPicture;
                }
                finally
                {
                    _cameraSemaphore.Release();
                }
            }

            return null;
        }


        private async void ApplicationBarIconButton_Shot(object sender, EventArgs e)
        {
            var imageInMemory = await TakePicture();
            if (imageInMemory == null)
                return;

            using (MediaLibrary mediaLibrary = new MediaLibrary())
                mediaLibrary.SavePicture(String.Format("image {0:yyyyMMdd-HHmmss}", DateTime.Now), imageInMemory.AsStream());

            MessageBox.Show("Image saved");
        }

        private async void ApplicationBarIconButton_ShotFast(object sender, EventArgs e)
        {
            var imageInMemory = await TakePictureFast();
            if (imageInMemory == null)
                return;

            using (MediaLibrary mediaLibrary = new MediaLibrary())
                mediaLibrary.SavePicture(String.Format("image {0:yyyyMMdd-HHmmss}", DateTime.Now), imageInMemory.AsStream());

            MessageBox.Show("Image saved");
        }
        #endregion

    }
}