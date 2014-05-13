using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using System.Text;

namespace MonsterCam
{
    public partial class about : PhoneApplicationPage
    {
        public about()
        {
            try
            {
                InitializeComponent();
                Uri uri = new Uri("data/EULA.txt", UriKind.Relative);
                var resource = App.GetResourceStream(uri);
                String str = "";

                using (System.IO.StreamReader reader = new System.IO.StreamReader(resource.Stream))
                {
                    str = reader.ReadToEnd();
                }
                LongText longText = new LongText(str);
                lbEula.ItemsSource = longText.Texts;
            }
            catch (Exception)
            {
            }
          



        }

      

        private void Hyperlink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                EmailComposeTask email = new EmailComposeTask();
                email.Subject = "Monster Cam";
                email.To = "";

                email.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Hyperlink_Click_1(object sender, RoutedEventArgs e)
        {
            try{
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri( "http://www.pedrolamas.com/2011/03/12/wp7-application-bar-icons/");
                web.Show();
            }
            catch(Exception)
            {
            }
            
        }

        private void Hyperlink_Click_2(object sender, RoutedEventArgs e)
        {
            try{
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://www.syncfusion.com/downloads/metrostudio");
                web.Show();
            }
            catch(Exception)
            {
            }
            
        }

        private void Hyperlink_Click_3(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://www.flurry.com/");
                web.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Hyperlink_Click_4(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://www.codeproject.com/Articles/36342/ExifLib-A-Fast-Exif-Data-Extractor-for-NET-2-0");
                web.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Hyperlink_Click_5(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://wp7adrotator.codeplex.com/");
                web.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Hyperlink_Click_6(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://silverlight.codeplex.com/");
                web.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Hyperlink_Click_7(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://templarian.com/project_windows_phone_icons/");
                web.Show();
            }
            catch (Exception)
            {
            }
        }
        private void Hyperlink_Click_8(object sender, RoutedEventArgs e)
        {
            try
            {
                WebBrowserTask web = new WebBrowserTask();
                web.Uri = new Uri("http://coding4fun.codeplex.com/");
                web.Show();
            }
            catch (Exception)
            {
            }
        }
       
    }
}