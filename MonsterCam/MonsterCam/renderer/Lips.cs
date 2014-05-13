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
    public class Lips : Renderer
    {
        float radius = 50.0f;
        float flip = 1.5f;
        protected override void onGenerateDrawElements(UIElementCollection previewsShape)
        {
            int nb = 5;
            int w = 0;
            int h = 0;

            List<VertexPositionTexture> lp = new List<VertexPositionTexture>();

            

            int YY = (int)( 2*Size.Height/5);
            int XX = (int)(Size.Width / 2);
            var Target1 = new System.Windows.Point(XX , YY);


            for (int y = 0; y < (int)Size.Height; y += ((int)Size.Height - 1 - y) >= nb ? nb : (((int)Size.Height - 1 - y) > 0 ? ((int)Size.Height - 1 - y) : 1))
            {

                ++h;
                w = 0;
                for (int x = 0; x < (int)Size.Width; x += ((int)Size.Width - 1 - x) >= nb ? nb : (((int)Size.Width - 1 - x) > 0 ? ((int)Size.Width - 1 - x) : 1))
                {
                    ++w;


                    float dx = (float)(x - Target1.X )/2.0f;
                    float dy = (float)(y - Target1.Y);
                    float f = (float)Math.Sqrt(dx * dx + dy * dy)/radius;





                    if (f > 1)
                    {
                        f = 1;
                    }
                    else
                    {
                        if (flip < 1)
                        {
                            f *= f * f;
                            f = flip * (1 - f) + f;
                        }
                        else
                        {
                            f = (float)Math.Pow(f, 0.33);

                            f = 2 * flip * (1 - f) + f;
                        }
                    }





                    lp.Add(new VertexPositionTexture(new Vector3(x * f + (float)Target1.X * (1.0f - f), y * f + (float)Target1.Y * (1.0f - f), 0),
                                                new Vector2((float)x / (float)Size.Width, (float)y / (float)Size.Height))
                                                );




                }
            }
            var sommet = lp.ToArray();
            List<short> lid = new List<short>();
            int nbtri = 0;
            for (int i = 0; i < h - 1; ++i)
                for (int j = 0; j < w - 1; ++j)
                {
                    lid.Add((short)(i * w + j));
                    lid.Add((short)(i * w + j + 1));
                    lid.Add((short)((i + 1) * w + j));

                    lid.Add((short)(i * w + j + 1));
                    lid.Add((short)((i + 1) * w + j + 1));
                    lid.Add((short)((i + 1) * w + j));
                    nbtri += 2;

                }
            var id = lid.ToArray();

            onUpdate = (b) =>
            {
                SharedGraphicsDeviceManager.Current.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        tranformVertex(sommet), 0, sommet.Length,
                        id, 0, nbtri);

            };




          

           
            {
                var elipse = new Ellipse();
                elipse.Width = 4 *radius;
                elipse.Height = 2 * radius;
                Canvas.SetTop(elipse, Size.Height - Target1.Y - elipse.Height / 2);
                Canvas.SetLeft(elipse, Target1.X - elipse.Width/2);

                elipse.Fill = previewBrush;
                previewsShape.Add(elipse);
            }

            




        }


        public Lips()
        {
            Name = "Lips & Nose";

            Grid panel = new Grid();
            panel.Width = 480;

            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            control.Add(panel);
            {
                Slider s = new Slider();
                s.Minimum = 0;
                s.Maximum = 100;
                s.Value = radius;
                s.ValueChanged += (ss, e) =>
                {
                    radius = (float)e.NewValue;
                    generateDrawElements();
                };
                panel.Children.Add(s);
            }

            {
                Slider s = new Slider();
                s.Minimum = 0;
                s.Maximum = 2;
                s.Value = flip;

                s.ValueChanged += (ss, e) =>
                {
                    flip = (float)e.NewValue;
                    generateDrawElements();
                };
                panel.Children.Add(s);
                Grid.SetColumn(s, 1);
            }
        }

    }
}
