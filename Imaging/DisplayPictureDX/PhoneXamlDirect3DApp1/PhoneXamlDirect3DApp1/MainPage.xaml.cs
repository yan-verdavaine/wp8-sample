using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneXamlDirect3DApp1Comp;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Threading;
using System.Windows.Resources;
using System.IO;
using  System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Xna.Framework.Media;

namespace PhoneXamlDirect3DApp1
{
    public partial class MainPage : PhoneApplicationPage
    {
        private Direct3DInterop m_d3dInterop = new Direct3DInterop();

        // Constructor
        public MainPage()
        {
            InitializeComponent();
        }

        private void DrawingSurface_Loaded(object sender, RoutedEventArgs e)
        {
            // Set window bounds in dips
            m_d3dInterop.WindowBounds = new Windows.Foundation.Size(
                (float)DrawingSurface.ActualWidth,
                (float)DrawingSurface.ActualHeight
                );

            // Set native resolution in pixels
            m_d3dInterop.NativeResolution = new Windows.Foundation.Size(
                (float)Math.Floor(DrawingSurface.ActualWidth * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f),
                (float)Math.Floor(DrawingSurface.ActualHeight * Application.Current.Host.Content.ScaleFactor / 100.0f + 0.5f)
                );

            // Set render resolution to the full native resolution
            m_d3dInterop.RenderResolution = m_d3dInterop.NativeResolution;

            // Hook-up native component to DrawingSurface
            DrawingSurface.SetContentProvider(m_d3dInterop.CreateContentProvider());
            DrawingSurface.SetManipulationHandler(m_d3dInterop);


            Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri("Assets/tux.png", UriKind.Relative));
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.SetSource(resourceInfo.Stream);
                    WriteableBitmap bmp = new WriteableBitmap(bitmap);
                    m_d3dInterop.CreateTexture(bmp.Pixels, bmp.PixelWidth, bmp.PixelHeight);
                });


        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask photoChooserTask           = new PhotoChooserTask();
                photoChooserTask.Completed += (ee,s)=>
                {
                    //DirectX context should be recreate before cereate the texture
                    Dispatcher.BeginInvoke(() =>
                   {
                       WriteableBitmap bmp = new WriteableBitmap(1,1);
                       bmp.SetSource(s.ChosenPhoto);
  
                       m_d3dInterop.CreateTexture(bmp.Pixels, bmp.PixelWidth, bmp.PixelHeight);
                       MessageBox.Show("Picture loaded with c#");
                   });


                };

                photoChooserTask.Show();

            }
            catch(Exception exp)
                {
                }
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask photoChooserTask = new PhotoChooserTask();
                photoChooserTask.Completed += (ee, s) =>
                {

                    var m = new MemoryStream();
                    s.ChosenPhoto.CopyTo(m);
                    //DirectX context should be recreate before cereate the texture
                    Dispatcher.BeginInvoke(async () =>
                    {
                        await m_d3dInterop.CreateTextureFromFileAsync(m.GetBuffer().AsBuffer(),1000,1000);
                        MessageBox.Show("Picture loaded with Imaging SDK");
                    });


                };

                photoChooserTask.Show();

            }
            catch (Exception exp)
            {
            }
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {

            WriteableBitmap bmp = new WriteableBitmap((int)DrawingSurface.ActualWidth, (int)DrawingSurface.ActualHeight);
            m_d3dInterop.TakeSnapShot(bmp.Pixels, (int)DrawingSurface.ActualWidth, (int)DrawingSurface.ActualHeight);
            MemoryStream stream = new MemoryStream();
            bmp.SaveJpeg(stream,1000,1000,0,90);
            stream.Position = 0;
            using (var media = new MediaLibrary())
            {
                media.SavePicture("snapshot", stream);


            }
            MessageBox.Show("snapshot with c# saved");


        }
        async private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            var file = await m_d3dInterop.TakeSnapShotAsync((int)DrawingSurface.ActualWidth, (int)DrawingSurface.ActualHeight);
            using (var media = new MediaLibrary())
            {
                media.SavePicture("snapshot_ImagingSDK", file.AsStream());
            }
            MessageBox.Show("snapshot with Imaging SDK saved");

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            m_d3dInterop.StartPreviewCamera();
        }
    }
}