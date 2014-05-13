using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using CameraInCpp.Resources;
using WindowsPhoneRuntimeComponent1;
using System.IO;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;

namespace CameraInCpp
{


    public partial class MainPage : PhoneApplicationPage
    {
        private WindowsPhoneRuntimeComponent _cameraComponent;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

            _cameraComponent = new WindowsPhoneRuntimeComponent();

            Loaded += MainPage_Loaded;
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            CaptureBtn.Visibility = System.Windows.Visibility.Collapsed;
            
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
           await _cameraComponent.InitCapture();
           CaptureBtn.Visibility = System.Windows.Visibility.Visible;
           StartCamBtn.Visibility = System.Windows.Visibility.Collapsed;
        }

        private async void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CaptureBtn.Visibility = System.Windows.Visibility.Collapsed;

            await _cameraComponent.CaptureImage(); 
            
            WriteableBitmap wb = new WriteableBitmap(capturedImage, null);

             await  _cameraComponent.render(new Bitmap(
                new Windows.Foundation.Size(wb.PixelWidth,wb.PixelHeight),
                ColorMode.Bgra8888,
                (uint)(4*wb.PixelWidth),
                wb.Pixels.AsBuffer())

                );
        
            capturedImage.Source = wb;

            CaptureBtn.Visibility = System.Windows.Visibility.Visible;

        }

    }
}