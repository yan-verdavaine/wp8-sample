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
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;
using Microsoft.Phone.Controls;
using Coding4Fun.Toolkit.Controls;
using Microsoft.Phone.Shell;

namespace MonsterCam.renderer
{
    public class Manual : Renderer
    {

        List<Vector2> vP = new List<Vector2>();
        List<Vector2> vDelta = new List<Vector2>();

        Vector2 lastPos = new Vector2();

        int validX(int X)
        {
            return X < 0 ? 0 : (X >= w ? w - 1 : X);
        }
        int validY(int Y)
        {
            return Y < 0 ? 0 : (Y >= h ? h - 1 : Y);
        }
        void ApplyGesture(Vector2 P, Vector2 Delta)
        {
            try
            {
                int r = 100;
                P.Y = (float)Size.Height - P.Y;
                var pp = sommet[validY((int)(P.Y / nb)) * w + validX((int)(P.X / nb))];
                if (sommet != null)
                {
                    if (currentdeform == demorfType.touch)
                    {

                        var ppp = sommet[validY((int)((P.Y + Delta.Y) / nb)) * w +validX( (int)((P.X + Delta.X) / nb))];


                        var p = new Vector3(P.X, P.Y, 0);
                        int ideb = (int)(P.Y - r) / nb;
                        int ifin = ideb + 2 * r / nb;
                        if (ideb < 0) ideb = 0;
                        if (ifin > h) ifin = h - 1;

                        int jdeb = (int)(P.X - r) / nb;
                        int jfin = jdeb + 2 * r / nb;
                        if (jdeb < 0) jdeb = 0;
                        if (jfin > w) jfin = w - 1;

                        for (int i = ideb; i < ifin; ++i)
                            for (int j = jdeb; j < jfin; ++j)
                            {
                                var id = i * w + j;
                                var v = p - sommet[id].Position;
                                var l = v.Length();

                                /* if (l < 100.0f)
                                 {
                                     float f = (float)(Math.Cos(Math.PI / 2 * l / 100) * factor);
                                     sommet[id].Position.X += (float)(Delta.X  * f);
                                     sommet[id].Position.Y -= (float)(Delta.Y * f);
                                 }*/

                                if (l < r)
                                {
                                    float f = (float)(tab_cos[(int)(999.0 * l / r)] * factor)/2;
                                    sommet[id].TextureCoordinate.X -= (float)((ppp.TextureCoordinate.X - pp.TextureCoordinate.X) * f);
                                   sommet[id].TextureCoordinate.Y += (float)((ppp.TextureCoordinate.Y - pp.TextureCoordinate.Y) * f);

                                    sommet[id].TextureCoordinate.X -= (float)((Delta.X / Size.Width) * f);
                                    sommet[id].TextureCoordinate.Y += (float)((Delta.Y / Size.Height) * f);

                                }
                            }
                    }
                    else if (currentdeform == demorfType.expand)
                    {


                        var p = new Vector3(P.X, P.Y, 0);
                        int ideb = (int)(P.Y - r) / nb;
                        int ifin = ideb + 2 * r / nb;
                        if (ideb < 0) ideb = 0;
                        if (ifin > h) ifin = h - 1;

                        int jdeb = (int)(P.X - r) / nb;
                        int jfin = jdeb + 2 * r / nb;
                        if (jdeb < 0) jdeb = 0;
                        if (jfin > w) jfin = w - 1;

                        for (int i = ideb; i < ifin; ++i)
                            for (int j = jdeb; j < jfin; ++j)
                            {
                                var id = i * w + j;
                                var v = p - sommet[id].Position;
                                var l = v.Length();

                                v.X /= (float)Size.Width;
                                v.Y /= (float)Size.Height;
                                if (l < r)
                                {
                                    float f = (float)tab_cos[(int)(999.0 * l / r)] * 0.02f;
                                    /*  sommet[id].TextureCoordinate.X += v.X * f ;
                                      sommet[id].TextureCoordinate.Y += v.Y * f ;*/
                                    sommet[id].TextureCoordinate.X += (float)((pp.TextureCoordinate.X - sommet[id].TextureCoordinate.X) * f);
                                    sommet[id].TextureCoordinate.Y += (float)((pp.TextureCoordinate.Y - sommet[id].TextureCoordinate.Y) * f);

                                }
                            }




                    }
                    else if (currentdeform == demorfType.contract)
                    {

                        var p = new Vector3(P.X, P.Y, 0);
                        int ideb = (int)(P.Y - r) / nb;
                        int ifin = ideb + 2 * r / nb;
                        if (ideb < 0) ideb = 0;
                        if (ifin > h) ifin = h - 1;

                        int jdeb = (int)(P.X - r) / nb;
                        int jfin = jdeb + 2 * r / nb;
                        if (jdeb < 0) jdeb = 0;
                        if (jfin > w) jfin = w - 1;

                        for (int i = ideb; i < ifin; ++i)
                            for (int j = jdeb; j < jfin; ++j)
                            {
                                var id = i * w + j;
                                var v = sommet[id].Position - p;
                                var l = v.Length();
                                v.X /= (float)Size.Width;
                                v.Y /= (float)Size.Height;
                                if (l < r)
                                {
                                    float f = (float)tab_cos[(int)(999.0 * l / r)] * 0.02f;
                                    /*sommet[id].TextureCoordinate.X += v.X * f;
                                    sommet[id].TextureCoordinate.Y += v.Y * f;*/
                                    sommet[id].TextureCoordinate.X -= (float)((pp.TextureCoordinate.X - sommet[id].TextureCoordinate.X) * f);
                                    sommet[id].TextureCoordinate.Y -= (float)((pp.TextureCoordinate.Y - sommet[id].TextureCoordinate.Y) * f);
                                }
                            }
                    }
                }
            }
            catch (Exception exp)
            {
            }

        }


       
        public override void gesture(TouchLocationState state, Vector2 P, Vector2 Delta)
        {


            if (state == TouchLocationState.Moved && currentdeform == demorfType.touch)
            {
                vP.Add(P - Delta);
                vDelta.Add(Delta);
            }
            else if (state == TouchLocationState.Released || state == TouchLocationState.Moved)//|| Delta.X == 0 && Delta.Y == 0)
            {
                vP.Add(P);
                vDelta.Add(Delta);
            }
            
            

           
        }

        VertexPositionTexture[] sommet;
        int nb = 5;
        int w = 0;
        int h = 0;
        Size oldSize = new Size();

        protected override void onGenerateDrawElements(UIElementCollection previewsShape)
        {

            if (sommet!=null && oldSize.Width == Size.Width && oldSize.Height == Size.Height)
                return;

            oldSize = Size; 

            int nb = 5;
            
            w = 0;
            h = 0;

            List<VertexPositionTexture> lp = new List<VertexPositionTexture>();




            for (int y = 0; y < (int)Size.Height; y += ((int)Size.Height - 1 - y) >= nb ? nb : (((int)Size.Height - 1 - y) > 0 ? ((int)Size.Height - 1 - y) : 1))
            {

                ++h;
                w = 0;
                for (int x = 0; x < (int)Size.Width; x += ((int)Size.Width - 1 - x) >= nb ? nb : (((int)Size.Width - 1 - x) > 0 ? ((int)Size.Width - 1 - x) : 1))
                {
                    ++w;
                   


                    lp.Add(new VertexPositionTexture(new Vector3(x , y , 0),
                                                new Vector2((float)x / (float)Size.Width, (float)y / (float)Size.Height))
                                                );




                }
            }
            sommet= lp.ToArray();
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
                for (int i = 0; i < vP.Count; ++i)
                    ApplyGesture(vP[i], vDelta[i]);

                vP.Clear();
                vDelta.Clear();

                SharedGraphicsDeviceManager.Current.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        tranformVertex(sommet), 0, sommet.Length,
                        id, 0, nbtri);

            };
           /* onPreviewUpdate = (b) =>
            {
                try
                {
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
                    batch.Begin();
                    batch.Draw(target, new Vector2(lastPos.X - 50, lastPos.Y - 50), Microsoft.Xna.Framework.Color.White);
                }
                catch (Exception exp)
                {


                }
                batch.End();

            };*/









        }

        enum demorfType
        {
            touch,
            expand,
            contract

        };


        demorfType currentdeform;
       
        double factor = .2;
        float[] tab_cos = new float[1000];
        public Manual()
        {
            Name = "Expressionism";

            for (int i = 0; i < tab_cos.Length; ++i)
                tab_cos[i] = (1+(float)Math.Cos(Math.PI  * i / (tab_cos.Length - 1.0)))/2; 



            UseGesture = true;

            Grid panel = new Grid();
            panel.Margin = new Thickness(0, 0, 0, 5);
            panel.RowDefinitions.Add(new RowDefinition());
            panel.RowDefinitions.Add(new RowDefinition());

            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            panel.ColumnDefinitions.Add(new ColumnDefinition());
            control.Add(panel);
            

            RoundToggleButton bCrop = new RoundToggleButton();
            RoundToggleButton bTouch = new RoundToggleButton();
            RoundToggleButton bExpand = new RoundToggleButton();
            RoundToggleButton bContract = new RoundToggleButton();
            Slider s = new Slider();

            {
                RoundToggleButton tt =  bCrop;
                tt.ImageSource = new BitmapImage(new Uri("data/appbar.crop.png", UriKind.RelativeOrAbsolute));
                
                tt.HorizontalAlignment = HorizontalAlignment.Left;
                 tt.Click += (a, aa) => {
                     UseGesture = false;
                     bCrop.IsChecked = true;
                    bTouch.IsChecked = false;
                    bExpand.IsChecked = false;
                    bContract.IsChecked = false;
                    s.IsEnabled= false;
                };
                tt.Content = "Position";

                panel.Children.Add(tt);
            }




            {
                RoundToggleButton b = bTouch;
                b.Content = "Touch";
                b.ImageSource = new BitmapImage(new Uri("data/appbar.cursor.hand.png", UriKind.RelativeOrAbsolute));

                b.Click += (a, aa) => {
                    UseGesture = true;
                    currentdeform = demorfType.touch;
                    bCrop.IsChecked = false;
                    bTouch.IsChecked = true;
                    bExpand.IsChecked = false;
                    bContract.IsChecked = false;
                    s.IsEnabled = true;
                };
                panel.Children.Add(b);

                Grid.SetColumn(b, 1);
            }
            {
                RoundToggleButton b = bExpand;
                b.Content = "Expand";
                b.ImageSource = new BitmapImage(new Uri("data/appbar.arrow.expand.png", UriKind.RelativeOrAbsolute));

                b.Click += (a, aa) => {
                    UseGesture = true;
                    currentdeform = demorfType.expand;
                    bCrop.IsChecked = false;
                    bTouch.IsChecked = false;
                    bExpand.IsChecked = true;
                    bContract.IsChecked = false;
                    s.IsEnabled = false;
                
                
                };
                panel.Children.Add(b);

                Grid.SetColumn(b, 2);
            }
            {
                RoundToggleButton b = bContract;
                b.Content = "Contract";
                b.ImageSource = new BitmapImage(new Uri("data/appbar.arrow.collapsed.png", UriKind.RelativeOrAbsolute));

                b.Click += (a, aa) =>
                {
                    UseGesture = true;
                    currentdeform = demorfType.contract;
                    bCrop.IsChecked = false;
                    bTouch.IsChecked = false;
                    bExpand.IsChecked = false;
                    bContract.IsChecked = true;
                    s.IsEnabled = false;


                };
                panel.Children.Add(b);

                Grid.SetColumn(b, 3);
            }


            UseGesture = true;
            bTouch.IsChecked = true;


            {
                RoundButton b = new RoundButton();
                b.Content = "reset";
                b.ImageSource = new BitmapImage(new Uri("data/appbar.refresh.rest.png", UriKind.RelativeOrAbsolute));

                b.Click += (a, aa) => { sommet = null; generateDrawElements(); };
                panel.Children.Add(b);

                Grid.SetColumn(b, 4);
            }
            {
               
                s.Minimum = 0.1;
                s.Maximum = 0.7;
                s.SmallChange = 0.1;
                s.LargeChange = 0.1;
                s.HorizontalAlignment = HorizontalAlignment.Stretch;
                s.Value = factor;
                s.Margin = new Thickness(0,0,0,-20);
                s.ValueChanged += (aaaa, bbbbb) =>
                    {
                        factor = bbbbb.NewValue;
                    };
                Grid.SetColumnSpan(s, 5);
                Grid.SetRow(s, 1);
                panel.Children.Add(s);
            }

            


        }
    }
}
