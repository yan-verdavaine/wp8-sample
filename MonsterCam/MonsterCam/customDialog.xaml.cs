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
using System.Windows.Controls.Primitives;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Phone.Tasks;

namespace MonsterCam
{
    public partial class CustomDialog : UserControl
    {
        //
        public delegate void closecallback();
        public closecallback Closed;

        public Popup  popup{
            get {
            if (Parent == null)
            {
                Popup _popup = new Popup();
                _popup.Child = this;
                _popup.Closed += (a, b) => { if (Closed != null) Closed(); };
            }
            
            return Parent as Popup;}
        }
        UIElementRenderer _elem = null;
        public UIElementRenderer elementRenderer
        {
             get
             {
                 if (_elem == null)
                 {
                     _elem = new UIElementRenderer(this, (int)480, (int)800);
                 }
                 return _elem;
            }
        }

        public bool accepted { get; private set; }
        bool firstChar = true;
        public CustomDialog(string s)
        {
            InitializeComponent();
            DataContext = this;
            
            message = s;

        }
        public string message
        {
            get { return (string)this.GetValue(sValueProperty); }
            set { this.SetValue(sValueProperty, value); }
        }
        public static readonly DependencyProperty sValueProperty = DependencyProperty.Register(
          "message", typeof(string), typeof(CustomDialog), new PropertyMetadata(""));

        

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            accepted = true;
            if (Parent is Popup)
                popup.IsOpen = false;
            else
            {
                if (Closed != null) Closed();
            }
        }

        private void button1_Click_1(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                marketplaceReviewTask.Show();
            }
            catch (Exception)
            {
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                MarketplaceDetailTask marketplaceReviewTask = new MarketplaceDetailTask();
                marketplaceReviewTask.ContentIdentifier = "8ac6c849-7b2f-4fa4-9be4-7e9d5f3e46a2";
                marketplaceReviewTask.ContentType = MarketplaceContentType.Applications;
                marketplaceReviewTask.Show();
            }
            catch (Exception)
            {
            }
        } 

    }
}
