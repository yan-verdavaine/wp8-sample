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
using System.Windows.Media.Imaging;
using System.IO;
using Microsoft.Xna.Framework.Media;
using Microsoft.Devices;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PictureNavigation.Method1
{
    public partial class PageMethod1 : PhoneApplicationPage
    {

        Method1Filter method = new Method1Filter();
        // Constructor
        public PageMethod1()
        {
            InitializeComponent();

            CameraButtons.ShutterKeyPressed += (s, e) =>
            {
                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    var bmp = new WriteableBitmap(displayImage, null);
                    var sout = new MemoryStream();
                    bmp.SaveJpeg(sout, bmp.PixelWidth, bmp.PixelHeight, 0, 70);
                    sout.Seek(0, SeekOrigin.Begin);

                    using (MediaLibrary mediaLibrary = new MediaLibrary())
                        mediaLibrary.SavePicture(String.Format("PictureNavigation Method1 {0:yyyyMMdd-HHmmss}", DateTime.Now), sout);
                });




            };


            Loaded += (s, e) =>
            {
                method.Output = displayImage;


                DispatcherTimer timer = new DispatcherTimer();
                timer.Tick += (ss, ee) =>
                {
                    const string total = "DeviceTotalMemory";
                    const string current = "ApplicationCurrentMemoryUsage";
                    const string peak = "ApplicationPeakMemoryUsage";

                    var currentBytes = ((long)DeviceExtendedProperties.GetValue(current)) / 1024.0 / 1024.0;


                    var txt = string.Format("Memory  = {0,5:F} MB / {1,5:F} MB\n", currentBytes, DeviceStatus.ApplicationMemoryUsageLimit / 1024 / 1024);
                    displayInfo.Text = txt + method.Info();




                };
                timer.Interval = new TimeSpan(0, 0, 0, 0, 40);
                timer.Start();


            };
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            base.OnBackKeyPress(e);
            Application.Current.Terminate();
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        method.Input = res.ChosenPhoto;
                    }

                };
                task.Show();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            GC.Collect();
        }

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            displayInfo.Visibility = displayInfo.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

 
        async private void ApplicationBarIconButton_Click_3(object sender, EventArgs e)
        {

            using (MediaLibrary mediaLibrary = new MediaLibrary())
                mediaLibrary.SavePicture(String.Format("PictureNavigation_{0:yyyyMMdd-HHmmss}", DateTime.Now), (await  method.GenerateReframingPicture()).ToArray());
            MessageBox.Show("Reframing picture saved");
        }
        


    }

}