using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;
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

namespace MonsterCam.renderer
{



    public class MirorRenderer : Renderer
    {


        bool left = true;


        protected override void onGenerateDrawElements(UIElementCollection previewsShape)
        {
  
            float f = left ? 0 : 1;

            var sommet = new VertexPositionTexture[]
                {
                   new VertexPositionTexture( new Vector3(0.0f                  ,0.0f   ,0.0f), new Vector2(f,0.0f)),
                   new VertexPositionTexture( new Vector3((float)Size.Width/2   ,0.0f   ,0.0f), new Vector2(0.5f,0.0f)),
                   new VertexPositionTexture( new Vector3((float)Size.Width     ,0.0f   ,0.0f), new Vector2(f,0.0f)),

                   new VertexPositionTexture( new Vector3(0.0f,                 (float) Size.Height, 0.0f), new Vector2(f,1.0f)),
                   new VertexPositionTexture( new Vector3((float)Size.Width/2,  (float) Size.Height, 0.0f), new Vector2(0.5f,1.0f)),
                   new VertexPositionTexture( new Vector3((float)Size.Width,    (float) Size.Height, 0.0f), new Vector2(f,1.0f))

                };

            var id = new short[]
            {
                0,1,3,
                1,4,3,
                1,2,4,
                2,5,4
            };
            int nbtri = id.Length / 3;

            onUpdate = (b) =>
            {

                SharedGraphicsDeviceManager.Current.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    tranformVertex(sommet,false), 0, sommet.Length,
                    id, 0, nbtri);

            };


          

            var poly = new Polygon();
            if (left)
            {
                poly.Points.Add(new System.Windows.Point(Size.Width / 2, 0));
                poly.Points.Add(new System.Windows.Point(Size.Width, 0));
                poly.Points.Add(new System.Windows.Point(Size.Width, Size.Height));
                poly.Points.Add(new System.Windows.Point(Size.Width / 2, Size.Height));
            }
            else
            {
                poly.Points.Add(new System.Windows.Point(Size.Width / 2, 0));
                poly.Points.Add(new System.Windows.Point(0, 0));
                poly.Points.Add(new System.Windows.Point(0, Size.Height));
                poly.Points.Add(new System.Windows.Point(Size.Width / 2, Size.Height));
            }
            poly.Fill = previewBrush;

            previewsShape.Add(poly);

       

            
           
        }


        public MirorRenderer()
        {
            Name = "Miror";

            ToggleSwitch tt = new ToggleSwitch();
            tt.IsChecked = !left;

            tt.Checked += (s, e) => { left = false; tt.Content = "Right"; generateDrawElements(); };
            tt.Unchecked += (s, e) => { left = true; tt.Content = "Left"; generateDrawElements(); };
            tt.Content = "Left";


            control.Add(tt);
        }

    }
}
