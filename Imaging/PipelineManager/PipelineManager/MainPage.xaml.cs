using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PipelineManager.Manager;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using Nokia.Graphics.Imaging;

namespace PipelineManager
{
    public partial class MainPage : PhoneApplicationPage
    {
        Bitmap input;
        BitmapImageSource picture;
        PipelineManager.Manager.PipelineManager manager;
        // Constructor
        public MainPage()
        {
            InitializeComponent();
             input = new Bitmap(new Windows.Foundation.Size(480, 800), ColorMode.Bgra8888);
             picture = new BitmapImageSource(input);
            // Sample code to localize the ApplicationBar
            //BuildLocalizedApplicationBar();
        }

        bool rendering = false;

        async void GeneratePicture()
        {
            if (rendering) return;


            ProgressIndicator prog = new ProgressIndicator();
            prog.IsIndeterminate = true;
            prog.Text = "Rendering";
            prog.IsVisible = true;
            SystemTray.SetProgressIndicator(this, prog);



            var bmp = new WriteableBitmap(480, 800);
            try
            {
                rendering = true;

             
              
                using (var renderer = new WriteableBitmapRenderer(manager, bmp, OutputOption.PreserveAspectRatio))
                {
                    display.Source = await renderer.RenderAsync();
                }

                SystemTray.SetProgressIndicator(this, null);
                rendering = false;
            }
            finally
            {
               
            }


        }

        private  void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += async (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                       

                       using( var source  = new StreamImageSource(res.ChosenPhoto))
                        using(var renderer = new BitmapRenderer(source,input))
                        {
                            await renderer.RenderAsync();
                        }
                        if(manager != null)
                        {
                            manager.Dispose();
                            manager = null;
                        }
                        manager = new PipelineManager.Manager.PipelineManager(picture);
                        GeneratePicture();
                    }

                };
                task.Show();
            }
            catch (Exception)
            {


            }
        }

        private void Button_filter(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;

            manager.Add(new NegativeFilter());
            GeneratePicture();

        }

        private void Button_ICustomfilter(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Add(new CPP.MyFilter(1.1));
            GeneratePicture();
        }

        private void Button_CustomfilterBase(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Add(new CSharp.MyFilter(1.1));
            GeneratePicture();
        }

        private void Button_Effect(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Add(new HdrEffect());
            GeneratePicture();
        }

        private void Button_ICustomEffect(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Add(new CPP.MyEffect(1.1));
            GeneratePicture();
        }

        private void Button_CustomEffectBase(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Add(new CSharp.MyEffect(manager.End(), 1.1));
            GeneratePicture();
        }

        private void Button_Undo(object sender, RoutedEventArgs e)
        {
            if (manager == null || rendering)
                return;
            manager.Undo();
            GeneratePicture();
        }
    }
}