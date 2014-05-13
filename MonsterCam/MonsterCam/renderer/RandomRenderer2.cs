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
using System.ComponentModel;
using System.Windows.Media.Imaging;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System.Windows.Threading;

namespace MonsterCam.renderer
{
    public class RandomRenderer2 : Renderer
    {

        double randMax = 5.0;
        protected override void onGenerateDrawElements(UIElementCollection previewsShape)
        {
          

           
            
            int nb = 8;
            int w = 0;
            int h = 0;

            List<VertexPositionTexture> lp = new List<VertexPositionTexture>();
            System.Random rand = new System.Random();
            for (int y = 0; y < (int)Size.Height; y += ((int)Size.Height - 1 - y) >= nb ? nb : (((int)Size.Height - 1 - y) > 0 ? ((int)Size.Height - 1 - y) : 1))
            {

                ++h;
                w = 0;
                for (int x = 0; x < (int)Size.Width; x += ((int)Size.Width - 1 - x) >= nb ? nb : (((int)Size.Width - 1 - x) > 0 ? ((int)Size.Width - 1 - x) : 1))
                {
                    ++w;
                    lp.Add(new VertexPositionTexture(new Vector3(x + rand.Next((int)(2 * randMax)) - (int)randMax, y + rand.Next((int)(2 * randMax)) - (int) randMax, 0),
                                                new Vector2((float)(x / Size.Width), (float)(y /Size.Height)))
                                                );
                }
            }
            var sommet = lp.ToArray();
            List<short> lid = new List<short>();
            int nbtri = 0;
            for(int i = 0;i<h -1;++i)
                for( int j = 0;j<w-1;++j)
                {
                    lid.Add((short)(i       * w + j));
                    lid.Add((short)(i       * w + j +1));
                    lid.Add((short)((i + 1) * w + j));
                                   
                    lid.Add((short)(i       * w + j + 1));
                    lid.Add((short)((i + 1) * w + j + 1));
                    lid.Add((short)((i + 1) * w + j));
                    nbtri += 2;

                }
            var id = lid.ToArray();

            onUpdate = (b) =>
                {
                   
                        SharedGraphicsDeviceManager.Current.GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            tranformVertex( sommet), 0, sommet.Length,
                            id, 0, nbtri);

                };
          

        
        }

        Slider s = new Slider();
        public RandomRenderer2()
        {
            Name = "Cubisme";


            
            s.Minimum = 0;
            s.Maximum = 100;
            s.Value = randMax;
          //  s.Style = Application.Current.Resources["sliderStyle"] as Style;
            s.ValueChanged += (ss, e) =>
            {
                randMax = (float)e.NewValue;
                generateDrawElements();
            };
            control.Add(s);
        }

    }
}
