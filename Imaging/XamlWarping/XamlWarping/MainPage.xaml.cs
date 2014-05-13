using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneAppWarping.Resources;
using Windows.Phone.Media.Capture;
using System.Windows.Media.Imaging;

namespace PhoneAppWarping
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoCaptureDevice camera = null;
        WriteableBitmap previewtmp = null;
        WriteableBitmap preview = null;
        List<Triangle> triangles = new  List<Triangle>();

        async void IinitCamera()
        {
            canvas.Children.Clear();
            camera = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back).First());

            previewtmp = new WriteableBitmap((int)camera.PreviewResolution.Width, (int)camera.PreviewResolution.Height);
            preview = new WriteableBitmap((int)camera.PreviewResolution.Width,(int)camera.PreviewResolution.Height);
           

          

            var t1 = new Triangle(
                new Triangle.Point(0, 0, 0, 0),
                new Triangle.Point(0, 480, 0, 1),
                new Triangle.Point(240, 480, 0.5, 1));
            var t2 = new Triangle(
               new Triangle.Point(0, 0, 0, 0),
               new Triangle.Point(240, 480, 0.5, 1),
               new Triangle.Point(480, 0, 1, 0));

            var t3 = new Triangle(
              new Triangle.Point(480, 0,1, 0),
              new Triangle.Point(240, 480, 0.5, 1),
              new Triangle.Point(480, 480, 1, 1));


            t1.ImageSource = preview;
            t2.ImageSource = preview;
            t3.ImageSource = preview;
            canvas.Children.Add(t1.Polygon);
            canvas.Children.Add(t2.Polygon);
            canvas.Children.Add(t3.Polygon);

            camera.PreviewFrameAvailable += camera_PreviewFrameAvailable;
          


        }
        bool updateRunning = false;
        void camera_PreviewFrameAvailable(ICameraCaptureDevice sender, object args)
        {
            if (updateRunning) return;

            updateRunning = true;
            camera.GetPreviewBufferArgb(previewtmp.Pixels);
            Dispatcher.BeginInvoke(() => {
                previewtmp.Pixels.CopyTo(preview.Pixels, 0);
                preview.Invalidate(); 
                updateRunning = false; });
        }

        public MainPage()
        {
            InitializeComponent();

            
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            IinitCamera();
        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            if (camera != null)
            {
                camera.Dispose();
                camera = null;
            }
        }
    }
}