using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PictureNavigation.Resources;
using Microsoft.Phone.Tasks;
using System.IO;
using System.Windows.Threading;
using Microsoft.Phone.Info;

namespace PictureNavigation
{
    public partial class MainPage : PhoneApplicationPage
    {
       
        public MainPage()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Method1/PageMethod1.xaml", UriKind.Relative));
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new Uri("/Method2/PageMethod2.xaml", UriKind.Relative));
        }
       
        
    }
}