using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using HighlightAlpha.Resources;
using Nokia.Graphics.Imaging;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Tasks;

namespace HighlightAlpha
{
    public partial class MainPage : PhoneApplicationPage
    {
        IImageProvider picture;
        // Constructor
        public MainPage()
        {
            InitializeComponent();

           
        }

        bool rendering = false;
        async void GeneratePicture()
        {
            var bmp = new WriteableBitmap(480,800);
            try
            {
                rendering = true;

                using (var effect = new CustomEffect.HighlightAlpha(picture))
                using (var renderer = new WriteableBitmapRenderer(effect, bmp))
                {
                    display.Source = await renderer.RenderAsync();
                }
            }
            finally
            {
                rendering = false;
            }


        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        picture = new StreamImageSource(res.ChosenPhoto);

                        GeneratePicture();
                    }

                };
                task.Show();
            }
            catch (Exception)
            {


            }
        }

        
    }
}