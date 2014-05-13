using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneApp1.watermarking
{
    public partial class WindowsPhoneControl1 : UserControl
    {

        public string Text
        {
            get { return txt.Text; }
            set { txt.Text = value; txt2.Text = value; }
        }
        public double FontSize
        {
            get { return txt.FontSize; }
            set { txt.FontSize = value; txt2.FontSize = value; }
        }
        public WindowsPhoneControl1()
        {
            InitializeComponent();
        }
    }
}
