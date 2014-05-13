using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneApp1.Resources;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using PhoneApp1.watermarking;

namespace PhoneApp1
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var task = new PhotoChooserTask();
            task.Completed += task_Completed;
            task.Show();

        }

        void task_Completed(object sender, PhotoResult e)
        {
            if(e.TaskResult == TaskResult.OK)
            {

                var bmp = new WriteableBitmap(0,0);
                bmp.SetSource(e.ChosenPhoto);


                var bmpres = new WriteableBitmap(bmp.PixelWidth,bmp.PixelHeight);

                var txt = new WindowsPhoneControl1();
                txt.Text = txtInput.Text;
                txt.FontSize = 36 * bmpres.PixelHeight / 480;
                txt.Width = bmpres.PixelWidth;
                txt.Height = bmpres.PixelHeight;

                //should call Measure and  when uicomponent is not in visual tree
                txt.Measure(new Size(bmpres.PixelWidth, bmpres.PixelHeight));
                txt.Arrange(new Rect(0, 0, bmpres.PixelWidth, bmpres.PixelHeight));


                bmpres.Render(new Image() { Source = bmp }, null);
                bmpres.Render(txt, null);

                bmpres.Invalidate();
                display.Source = bmpres;

            }


        }

      
    }
}