using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace MonsterCam
{
  

    public class LocalizedStrings
    {
        public LocalizedStrings()
        {
        }

        private static MonsterCam.text localizedResources = new MonsterCam.text();

        public MonsterCam.text LocalizedResources { get { return localizedResources; } }
    }
}
