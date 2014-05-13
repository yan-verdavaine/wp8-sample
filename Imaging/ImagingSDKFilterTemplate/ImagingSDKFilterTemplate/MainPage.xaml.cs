using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using Microsoft.Phone.Info;
using Microsoft.Phone.Tasks;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Threading.Tasks;
using ImagingSDKFIlterTemplate.Recipe;


namespace ImagingSDKFIlterTemplate
{
    public partial class MainPage : PhoneApplicationPage
    {


        #region Filter

        //filter/effect implementation
        async Task<WriteableBitmap> renderPipeline(IImageProvider source, WriteableBitmap bitmapOutput)
        {

            if (source == null || bitmapOutput == null) return null;

              var t = DateTime.Now;

           //using (var effect = new RecipeCSharpEffect(source, FilterParam.Value)) //Recipe with C# custom effect
           //using  (var effect = new RecipeCSharpFilter(source, FilterParam.Value))//Recipe with C# custom Filter
           // using (var effect = new RecipeCPPEffect(source, FilterParam.Value))   //Recipe with CPP custom effect   
          //  using (var effect = new RecipeCPPFilter(source, FilterParam.Value))     //Recipe with CPP custom effect  
            using (var effect = new RecipeDaisyChain(source, FilterParam.Value))     //Recipe Daysi chain 
            using (var renderer = new WriteableBitmapRenderer(effect,bitmapOutput))   
               {

                  var result = await renderer.RenderAsync();

                 var ms = DateTime.Now.Subtract(t).TotalMilliseconds;
                 previewResult = string.Format(
@"ImageSize = {0}x{1}
t = {1:F}

", bitmapOutput.PixelWidth, bitmapOutput.PixelHeight, ms); 
                


                return result;
               }



                  
                   



        }


        //GUI filter/effect parameters update
        private void FilterParam_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            requestProcessing();
        }
      
    #endregion


        #region Internal
        StreamImageSource HRImagesource = null;
        Windows.Foundation.Size ImageSize;
        WriteableBitmap LRImageSource;

        ImageBrush displayBackgroundBrush;
        WriteableBitmap bitmapDisplayed;
        WriteableBitmap bitmapTmp;
        string benchResult = "";
        string previewResult = "";

        private double _ScreenToPixelFactor = 0;
        private double ScreenToPixelFactor
        {
            get
            {
                if (_ScreenToPixelFactor == 0)
                {
                    try
                    {
                        _ScreenToPixelFactor = ((System.Windows.Size)DeviceExtendedProperties.GetValue("PhysicalScreenResolution")).Width / 480;
                    }
                    catch (Exception)
                    {
                        _ScreenToPixelFactor = System.Windows.Application.Current.Host.Content.ScaleFactor / 100.0;
                    }
                }
                return _ScreenToPixelFactor;
            }
        }

      


        // Constructor
        public MainPage()
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
                    displayInfo.Text = txt +previewResult+ benchResult;


                };
                timer.Interval = new TimeSpan(0, 0, 0, 0, 40);
                timer.Start();


               bitmapDisplayed = new WriteableBitmap((int)(display.ActualWidth * ScreenToPixelFactor), (int)(display.ActualHeight * ScreenToPixelFactor));
               bitmapTmp = new WriteableBitmap((int)(display.ActualWidth * ScreenToPixelFactor), (int)(display.ActualHeight * ScreenToPixelFactor));
               LRImageSource = new WriteableBitmap((int)(display.ActualWidth * ScreenToPixelFactor), (int)(display.ActualHeight * ScreenToPixelFactor));
               
                displayBackgroundBrush = new ImageBrush();
                display.Background = displayBackgroundBrush;
                displayBackgroundBrush.Stretch = Stretch.Uniform;

            };
        }


        enum STATE
        {
            WAIT,
            APPLY,
            SCHEDULE
        };
        //Current State
        STATE currentState = STATE.WAIT;


        void requestProcessing()
        {
            switch (currentState)
            {
                //State machine transition : WAIT -> APPLY 
                case STATE.WAIT:
                    currentState = STATE.APPLY;
                    //enter in APPLY STATE => apply the filter
                    processRendering();
                    break;

                //State machine transition : APPLY -> SCHEDULE
                case STATE.APPLY:
                    currentState = STATE.SCHEDULE;
                    break;

                //State machine transition : SCHEDULE -> SCHEDULE
                case STATE.SCHEDULE:
                    currentState = STATE.SCHEDULE;
                    break;
            }
        }
        void processFinished()
        {
            switch (currentState)
            {
                //State machine transition : APPLY -> WAIT.  
                case STATE.APPLY:
                    currentState = STATE.WAIT;
                    break;
                //State machine transition : SCHEDULE -> APPLY. 
                case STATE.SCHEDULE:
                    currentState = STATE.APPLY;
                    //enter in APPLY STATE => apply the filter
                    processRendering();
                    break;
            }
        }

        async void processRendering()
        {

            try
            {
                using (var source = new BitmapImageSource(LRImageSource.AsBitmap()))
                {
                    await renderPipeline(source, bitmapTmp);
                }
                bitmapTmp.Invalidate();

                var tmp = bitmapTmp;
                bitmapTmp = bitmapDisplayed;
                bitmapDisplayed = tmp;

                bitmapDisplayed.Invalidate();
                displayBackgroundBrush.ImageSource = bitmapDisplayed;
            }
            catch (Exception)
            {
            }
            finally
            {
                processFinished();
            }


        }







        private void ApplicationBarIconButton_Image(object sender, EventArgs e)
        {

            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += async (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        if (HRImagesource != null)
                        {
                            HRImagesource.Dispose();
                            HRImagesource = null;
                        }
                        HRImagesource = new StreamImageSource(res.ChosenPhoto);
                        var info = await HRImagesource.GetInfoAsync();
                        ImageSize = info.ImageSize;
 
                        //create LR image
                        using (var renderer = new WriteableBitmapRenderer(HRImagesource, LRImageSource))
                            await renderer.RenderAsync();

                        requestProcessing();
                    }

                };
                task.Show();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ApplicationBarIconButton_Info(object sender, EventArgs e)
        {
            displayInfo.Visibility = displayInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }


        bool benchRuning = false;

        async private void ApplicationBarIconButton_Bench(object sender, EventArgs e)
        {

            if (currentState != STATE.WAIT || HRImagesource == null) return;
            try
            {
                benchResult = "BENCH RUNING...";
                IsEnabled = false;
                benchRuning = true;
                ApplicationBar.IsVisible = false;

                displayInfo.Visibility = Visibility.Visible;

                var bitmap = new WriteableBitmap((int)ImageSize.Width, (int)ImageSize.Height);
                int nbTest = 0;
                double tmin = double.PositiveInfinity;
                double tmax = 0;
                double tacc = 0;
                for (; nbTest < 100 && benchRuning; ++nbTest)
                {
                    var t = DateTime.Now;
                    await renderPipeline(HRImagesource, bitmap);
                    var ms = DateTime.Now.Subtract(t).TotalMilliseconds;
                    tacc += ms;
                    if (ms < tmin) tmin = ms;
                    if (ms > tmax) tmax = ms;
                    benchResult = string.Format(
@"BENCH RUNING...
Nb rendering = {0}
TMin = {1:F}
TMax = {2:F}
TMean = {3:F}
ImageSize = {4}x{5}
", (1 + nbTest), tmin, tmax, tacc / (1 + nbTest), (int)ImageSize.Width, (int)ImageSize.Height); 
                }

                benchResult = string.Format(
@"BENCH FINISHED
Nb rendering = {0}
TMin = {1:F}
TMax = {2:F}
TMean = {3:F}
ImageSize = {4}x{5}
", (1 + nbTest), tmin, tmax, tacc / (1 + nbTest), (int)ImageSize.Width, (int)ImageSize.Height); 
            }
            finally
            {
                benchRuning = false;
                IsEnabled = true;
                ApplicationBar.IsVisible = true;
            }


        }

        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            if (benchRuning)
            {
                benchRuning = false;
                e.Cancel = true;
            }
            else
                base.OnBackKeyPress(e);
        }

         #endregion


    }
}