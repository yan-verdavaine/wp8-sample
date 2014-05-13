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

namespace MonsterCam.renderer
    {
        public class Renderer : INotifyPropertyChanged
        {
            public event PropertyChangedEventHandler PropertyChanged;
            protected void OnPropertyChanged(string name)
            {
                PropertyChangedEventHandler handler = PropertyChanged;
                if (handler != null)
                {
                    handler(this, new PropertyChangedEventArgs(name));
                }
            }

            public bool UseGesture
            {
                get;
                protected set;
            }

            public virtual void gesture(TouchLocationState state, Vector2 P, Vector2 Delta)
            {
              

            }

            public string Name
            { get; set; }

            List<Shape> previewElements_ = new List<Shape>();
            public List<Shape> previewElements
            {
                get { return previewElements_; }
                protected set { previewElements_ = value; OnPropertyChanged("previewElements"); }
            }


            
            List<UIElement> _control = new List<UIElement>();
            public List<UIElement> control
            {
                get{return _control;}
                set { _control = value; OnPropertyChanged("control"); }
                
            }

            protected Size _size = new Size();
            public Size Size
            {
                get { return _size; }
                set
                {
                    if (value != null)
                    {
                        _size = value;
                        if (running)
                        {
                            Stop(); 
                            Start();
                        }
                    }
                }
            }

            public Transform Transform
            {
                get;
                set;
            }



            public delegate void updatext(WriteableBitmap b);
            public updatext update;
            protected updatext onUpdate;
            protected updatext onPreviewUpdate;
            public RenderTarget2D renderTarget
            {
                get;
                private set;
            }

            public RenderTarget2D previewTarget
            {
                get;
                private set;
            }

            Texture2D               texture;
            BasicEffect             effect;
            protected WriteableBitmap _renderTexture;
           

            protected WriteableBitmap _previewBitmap;
            public WriteableBitmap previewBitmap
            {
                get { return _previewBitmap; }
                protected set
                {
                    if (value != null)
                    {
                        _previewBitmap = value;
                        OnPropertyChanged("previewBitmap");
                    }
                }
            }

            bool                    running = false;
            public void Start()
            {

                running = true;
                if (_size.Width == 0 && _size.Height == 0) return;

                texture = null; 

                renderTarget = new RenderTarget2D(SharedGraphicsDeviceManager.Current.GraphicsDevice, (int)_size.Width, (int)_size.Height, false, SurfaceFormat.Color, DepthFormat.None);
                previewTarget = new RenderTarget2D(SharedGraphicsDeviceManager.Current.GraphicsDevice, (int)_size.Width, (int)_size.Height, false, SurfaceFormat.Color, DepthFormat.None);
                
                effect = new BasicEffect(SharedGraphicsDeviceManager.Current.GraphicsDevice);
                effect.TextureEnabled = true;
                effect.Parameters["Texture"].SetValue(texture);
                
                
                generateDrawElements();
            }
            public void Stop()
            {

                running = false;
                if(renderTarget != null)
                    renderTarget.Dispose();
                renderTarget = null;

                if (previewTarget != null)
                    previewTarget.Dispose();
                previewTarget = null;

                if (effect != null)
                    effect.Dispose();
                effect = null;

               
            }

            VertexPositionTexture[] buffer;
            protected VertexPositionTexture[] tranformVertex(VertexPositionTexture[] sommet,bool checkBorne = true)
            {
               
                if(Transform == null)
                {
                    return sommet;
                }
                if (buffer == null || buffer.Length != sommet.Length)
                {
                    buffer = new VertexPositionTexture[sommet.Length];
                }

                var tt = Transform.Inverse;

                for (int i = 0; i < sommet.Length; ++i)
                {
                    var p = tt.Transform(new System.Windows.Point(sommet[i].TextureCoordinate.X,1- sommet[i].TextureCoordinate.Y));

                    buffer[i].Position = sommet[i].Position;

                    if (checkBorne)
                    {
                        if (p.X < 0) p.X = -0.2 * p.X;
                        if (p.Y < 0) p.Y = -0.2 * p.Y;

                        if (p.X > 1) p.X = (1 - 0.2 * (p.X - 1));
                        if (p.Y > 1) p.Y = (1 - 0.2 * (p.Y - 1));
                    }
                    buffer[i].TextureCoordinate.X = (float)p.X;
                    buffer[i].TextureCoordinate.Y =  (float)p.Y;
                }


                return buffer;
            }

            public Brush previewBrush
            {
                get;
                set;
            }
            protected virtual void onGenerateDrawElements(UIElementCollection previewsShape)
            {

            }

            protected   void generateDrawElements()
            {

                float width = (int)_size.Width;
                float height = (int)_size.Height;

                float h = (height-2) / 2.0f / (float)Math.Tan((double)MathHelper.ToRadians(22.5f));

                Vector3 cameraPosition = new Vector3(width / 2.0f, height / 2.0f, h);
                Vector3 cameraTarget = new Vector3(width / 2.0f, height / 2.0f, 0);
                Vector3 cameraUp = Vector3.UnitY;
                float nearClippingDistance = 0.01f;
                float farClippingDistance = 1000f;
                float fieldOfView = MathHelper.ToRadians(45.0f);
                float aspectRatio =  (float)width/(float)height ;
                Microsoft.Xna.Framework.Matrix world = Microsoft.Xna.Framework.Matrix.Identity;
                Microsoft.Xna.Framework.Matrix view = Microsoft.Xna.Framework.Matrix.CreateLookAt(cameraPosition, cameraTarget, cameraUp);
                Microsoft.Xna.Framework.Matrix projection = Microsoft.Xna.Framework.Matrix.CreatePerspectiveFieldOfView(fieldOfView, aspectRatio, nearClippingDistance, farClippingDistance);

                var proj = world * view * projection;

                try
                {
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(previewTarget);
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Transparent);
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(null);
                }
                catch (Exception exp)
                {
                }




                
                update = (_bitmap) =>
                {
                    try
                    {

                        if (texture == null || texture.Width != _bitmap.PixelWidth || texture.Height != _bitmap.PixelHeight)
                        {


                            texture = new Texture2D(SharedGraphicsDeviceManager.Current.GraphicsDevice,
                                      (int)_bitmap.PixelWidth,
                                      (int)_bitmap.PixelHeight);

                        }



                        SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(renderTarget);

                        RasterizerState rs = new RasterizerState();
                        rs.CullMode = CullMode.None;
                        SharedGraphicsDeviceManager.Current.GraphicsDevice.RasterizerState = rs;

                       
                       

                         
                       
                        
                        
                      


                        
                       texture.SetData<int>(_bitmap.Pixels);
                      

                        // Clear the renderTarget. By default it's all a bright purple color. I like to use Color.Transparent to
                        // enable easy alpha blending.


                        SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);

                        
                        

                        effect.Texture = texture;
                        SharedGraphicsDeviceManager.Current.GraphicsDevice.SamplerStates[0] = SamplerState.AnisotropicClamp;

                       

                        effect.Parameters["WorldViewProj"].SetValue(proj);
                        
                        

                        foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            if (onUpdate != null)
                                onUpdate(_bitmap);

                    
                        }
                        
                    }
                    catch (Exception exp)
                    {
                    }
                    SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(null);
                   // renderTarget.GetData<int>(_bitmap.Pixels);
                   // _renderTexture.Pixels[0] = _bitmap.Pixels[0];

                   // _renderTexture.Invalidate();
                    if (onPreviewUpdate != null)
                    {
                        SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(previewTarget);
                        onPreviewUpdate(null);

                        SharedGraphicsDeviceManager.Current.GraphicsDevice.SetRenderTarget(null);
                    }

                };

                
                var canvas = new Canvas();
                 onGenerateDrawElements(canvas.Children);
                if (canvas.Children.Count > 0)
                {
                    canvas.Width = Size.Width;
                    canvas.Height = Size.Height;
                    canvas.Measure(Size);
                    canvas.Arrange(new Rect(new System.Windows.Point(0, 0), Size));
                    var tmp = new WriteableBitmap((int)Size.Width,(int) Size.Height);
                                    
                    tmp.Render(canvas, null);
                    tmp.Invalidate();
                    previewTarget.SetData<int>(tmp.Pixels);
                }

            }


            public Renderer()
            {
                previewBrush = new SolidColorBrush(System.Windows.Media.Color.FromArgb(50, 0, 0, 255));
                Name = "titi";
                UseGesture = false;
            }
            public override string ToString()
            {
                return Name;
            }

        }
    }

