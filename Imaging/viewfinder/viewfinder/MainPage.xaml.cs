using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Media.Imaging;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Devices;

namespace viewfinder
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();
            view.SensorLocation = Windows.Phone.Media.Capture.CameraSensorLocation.Back;


            Loaded += (a,b) => {
                if (! (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) && PhotoCamera.IsCameraTypeSupported(CameraType.Primary)))
                    ApplicationBar.Buttons.RemoveAt(0); 
            };

            view.start();
            CameraButtons.ShutterKeyHalfPressed += async (s, e) => 
            { 
            await view.FocusAsync();
            
            };
            CameraButtons.ShutterKeyPressed +=async (s, e) => 
            {    
                var file = await view.TakePicture();
                if (file == null)
                {
                    return;
                }
                var bmp = new BitmapImage();
                bmp.SetSource(file.AsStream());
                display.Source = bmp;
            };


        }

     

        private void ApplicationBarIconButton_Click(object sender, EventArgs e)
        {
            
            if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing) && PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                view.SensorLocation = view.SensorLocation == Windows.Phone.Media.Capture.CameraSensorLocation.Front ? Windows.Phone.Media.Capture.CameraSensorLocation.Back : Windows.Phone.Media.Capture.CameraSensorLocation.Front;
        }

        private void ApplicationBarIconButton_Click_1(object sender, EventArgs e)
        {
            view.Mode = view.Mode == control.ViewFinder.StrechMode.Uniform ? control.ViewFinder.StrechMode.UniformFill : control.ViewFinder.StrechMode.Uniform;
       
        }

        async private void ApplicationBarIconButton_Click_2(object sender, EventArgs e)
        {
            var file = await view.TakePicture();
            var bmp = new BitmapImage();
            bmp.SetSource(file.AsStream());
            display.Source = bmp;
        }

  
    }
}