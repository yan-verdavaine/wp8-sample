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
    public class Toto : Renderer
    {

        float radius = 100.0f;
        float c = 0;

        protected override void onGenerateDrawElements(UIElementCollection previewsShape)
        {
     


            int nb = 5;
            int w = 0;
            int h = 0;

            List<VertexPositionTexture> lp = new List<VertexPositionTexture>();
            System.Random rand = new System.Random();
            var target = new Microsoft.Xna.Framework.Point((int)Size.Width / 2, (int)Size.Height / 2);

            for (int y = 0; y < (int)Size.Height; y += ((int)Size.Height - 1 - y) >= nb ? nb : (((int)Size.Height - 1 - y) > 0 ? ((int)Size.Height - 1 - y) : 1))
            {

                ++h;
                w = 0;
                for (int x = 0; x < (int)Size.Width; x += ((int)Size.Width - 1 - x) >= nb ? nb : (((int)Size.Width - 1 - x) > 0 ? ((int)Size.Width - 1 - x) : 1))
                {
                    ++w;
                    float f = (float)Math.Sqrt((float)(target.X - x) * (target.X - x) + (float)(target.Y - y) * (target.Y - y)) / radius;

                    if (f > 1)
                    {
                        f = 1;
                    }
                    else
                    {
                        if (c < 1)
                        {
                            f *= f * f;
                            f = c * (1 - f) + f;
                        }
                        else
                        {
                            f = (float)Math.Pow(f, 0.33);

                            f = 2*c * (1 - f) + f;
                        }
                    }
                    //  f = f * f;
                  //  f = (float)Math.Cos(Math.PI / 2.0 * f);
                    lp.Add(new VertexPositionTexture(new Vector3(x * f + target.X * (1.0f - f), y * f + target.Y * (1.0f - f), 0),
                                                new Vector2((float)x / (float)Size.Width, (float)y / (float)Size.Height))
                                                );
                }
            }
            var sommet = lp.ToArray();
            var sommet2 = lp.ToArray();


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

        

           

            

      


            var elipse = new Ellipse();
            elipse.Width = 2 * radius;
            elipse.Height = 2 * radius;

            Canvas.SetTop(elipse, Size.Height - target.Y - elipse.Height / 2);
            Canvas.SetLeft(elipse, target.X - elipse.Width / 2);

            elipse.Fill = previewBrush;
            previewsShape.Add(elipse);


            
         





        }


        public Toto()
        {
            Name = "Black Hole";

            Grid panel = new Grid();
            panel.Width = 480;
    
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            //panel.Orientation = Orientation.Horizontal;
            {
                Slider s = new Slider();
                s.Minimum = 0;
                s.Maximum = 300;
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
                s.Value = c;

                s.ValueChanged += (ss, e) =>
                {
                    c = (float)e.NewValue;
                    generateDrawElements();
                };
                Grid.SetColumn(s,1);
                panel.Children.Add(s);
            }
            control.Add(panel);
        }

    }
}
