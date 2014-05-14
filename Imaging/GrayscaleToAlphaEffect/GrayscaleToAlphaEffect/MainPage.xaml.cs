﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using GrayscaleToAlphaEffect.Resources;
using Nokia.Graphics.Imaging;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;

namespace GrayscaleToAlphaEffect
{
    public partial class MainPage : PhoneApplicationPage
    {

        IImageProvider  picture;
        IImageProvider alpha;

        // Constructor
        public MainPage()
        {
            InitializeComponent();

          
        }

        // Load data for the ViewModel Items
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
           
        }

        bool rendering = false;
        async void GeneratePicture()
        {
            if (picture == null || rendering)
                return;

            var bmp = new WriteableBitmap(300, 300);
            try
            {
                rendering = true;
              
                using (var effect = new CustomEffect.GrayscaleToAlphaEffect(picture, alpha))
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



        private void Button_Click_Picture(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                       picture = new StreamImageSource( res.ChosenPhoto);
                       
                        GeneratePicture();
                    }

                };
                task.Show();
            }
            catch (Exception)
            {

               
            }
        }

        private void Button_Click_Alpha(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        alpha = new StreamImageSource(res.ChosenPhoto);
                    }
                    else
                    {
                        alpha = null;
                    }
                    GeneratePicture();

                };
                task.Show();
            }
            catch (Exception)
            {


            }
        }

     
    }
}