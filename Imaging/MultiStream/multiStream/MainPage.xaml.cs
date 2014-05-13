using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using multiStream.Resources;
using Windows.Phone.Media.Capture;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using System.Windows.Threading;
using Microsoft.Phone.Info;



namespace multiStream
{
    public partial class MainPage : PhoneApplicationPage
    {
        PhotoCaptureDevice camera = null;
        PhotoCaptureDevice camera2 = null;
        WriteableBitmap b1;
        WriteableBitmap b2;
        WriteableBitmap b3;
        WriteableBitmap b4;
        WriteableBitmapRenderer renderer1;
        WriteableBitmapRenderer renderer2;
        WriteableBitmapRenderer renderer3;
        WriteableBitmapRenderer renderer4;

        byte[] buffer;

        public MainPage()
        {
            InitializeComponent();

            DispatcherTimer timer = new DispatcherTimer();
            timer.Tick += (ss, ee) =>
            {
                const string total = "DeviceTotalMemory";
                const string current = "ApplicationCurrentMemoryUsage";
                const string peak = "ApplicationPeakMemoryUsage";

                var currentBytes = ((long)DeviceExtendedProperties.GetValue(current)) / 1024.0 / 1024.0;


                var txt = string.Format("Memory  = {0,5:F} MB\n", currentBytes);
                displayInfo.Text = txt;
                //    MemoryInfo.Text = string.Format("Memory  = {0,5:F} MB", currentBytes);



            };
            timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            timer.Start();


            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }


        async void startDisplay()
        {
            camera = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, PhotoCaptureDevice.GetAvailableCaptureResolutions(CameraSensorLocation.Back).First());
            int layersize = (int)(camera.PreviewResolution.Width * camera.PreviewResolution.Height);
            int layersizeuv = layersize / 2;
            buffer = new byte[layersize + layersizeuv];
            b1 = new WriteableBitmap(300, 300);
            b2 = new WriteableBitmap(300, 300);
            b3 = new WriteableBitmap(300, 300);
            b4 = new WriteableBitmap(300, 300);

            d1.Source = b1;
            d2.Source = b2;
            d3.Source = b3;
            d4.Source = b4;


            /*  var previewBitmap = new Bitmap(
                  camera.PreviewResolution,
                  ColorMode.Bgra8888,
                  (uint)(4*camera.PreviewResolution.Width),
                  buffer.AsBuffer());*/

            var bbbb = buffer.AsBuffer(layersize, layersizeuv);
            var previewBitmap = new Bitmap(
                camera.PreviewResolution,
                ColorMode.Yuv420Sp,
                new uint[] { (uint)camera.PreviewResolution.Width, (uint)camera.PreviewResolution.Width },
                new IBuffer[] { buffer.AsBuffer(0, layersize), bbbb });



            {
                var input = new BitmapImageSource(previewBitmap);
                var effet = new FilterEffect(input);
                effet.Filters = new IFilter[] { new RotationFilter(90) };
                renderer1 = new WriteableBitmapRenderer(effet, b1);
            }
            {
                var input = new BitmapImageSource(previewBitmap);
                var effet = new FilterEffect(input);
                effet.Filters = new IFilter[] { new RotationFilter(90), new CartoonFilter() , new HueSaturationFilter()};
                renderer2 = new WriteableBitmapRenderer(effet, b2);
            }
            {
                var input = new BitmapImageSource(previewBitmap);
                var effet = new FilterEffect(input);
                effet.Filters = new IFilter[] { new RotationFilter(90), new MagicPenFilter(), new NegativeFilter() };
                renderer3 = new WriteableBitmapRenderer(effet, b3);
            }
            {
                var input = new BitmapImageSource(previewBitmap);
                var effet = new FilterEffect(input);
                effet.Filters = new IFilter[] { new RotationFilter(90), new SketchFilter() };
                renderer4 = new WriteableBitmapRenderer(effet, b4);
            }


            camera.PreviewFrameAvailable += camera_PreviewFrameAvailable;

        }



         protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

                startDisplay();

         


        }
        bool rendering = false;
        async void camera_PreviewFrameAvailable(ICameraCaptureDevice sender, object args)
        {
            if (rendering) return;

            try
            {
                rendering = true;
                //camera.GetPreviewBufferArgb(buffer);
                camera.GetPreviewBufferYCbCr( buffer);

                if (false)
                {
                    await renderer1.RenderAsync();
                    await renderer2.RenderAsync();
                    await renderer3.RenderAsync();
                    await renderer4.RenderAsync();
                }
                else
                {
                    await Task.WhenAll(new Task[]
                             {
                                 renderer1.RenderAsync().AsTask(),
                                 renderer2.RenderAsync().AsTask(),
                                 renderer3.RenderAsync().AsTask(),
                                 renderer4.RenderAsync().AsTask()
                             });
                }

                Dispatcher.BeginInvoke( () =>
                    {
                        
                        b1.Invalidate();
                        b2.Invalidate();
                        b3.Invalidate();
                        b4.Invalidate();
                        rendering = false;
                    });




            }
            catch(Exception e)
            {
                rendering = false;
            }


        }
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            if (camera != null)
                camera.Dispose();
            camera = null;
        }
    }
}