using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PipelineManager.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;

namespace PipelineManager
{
    public partial class MainPage : PhoneApplicationPage
    {
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        bool rendering = false;

        async void GeneratePicture()
        {
            if (rendering) return;
            var bmp = new WriteableBitmap(480, 800);
            try
            {
                rendering = true;

                //  using (var effect = new CustomEffect.HighlightAlpha(picture,13))
              
               // using (var renderer = new WriteableBitmapRenderer(effect, bmp, OutputOption.PreserveAspectRatio))
              //  {
               //     display.Source = await renderer.RenderAsync();
              //  }
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
                      //  picture = new StreamImageSource(res.ChosenPhoto);

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