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
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Devices;
using System.Windows.Threading;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Microsoft.Xna.Framework.Media;
using System.IO;
using Facebook;
using Microsoft.Xna.Framework.Content;
using MonsterCam.renderer;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using MonsterCam.renderer;
using Microsoft.Xna.Framework.Input.Touch;
using System.Threading;
using ExifLib;
using System.Windows.Controls.Primitives;
using Microsoft.Phone.UserData;
using System.IO.IsolatedStorage;

//using Microsoft.Xna.Framework.Media;
namespace MonsterCam
{
    public partial class MainPage : PhoneApplicationPage
    {

        Microsoft.Devices.PhotoCamera Camera = null;
        VideoBrush CameraBrush = null;
        DispatcherTimer CameraTimer = null;
        WriteableBitmap CameraBitmap = null;
        WriteableBitmap FullCameraBitmap = null;


        TransformGroup inputTransform = new TransformGroup();
        CompositeTransform gestureTransform = new CompositeTransform();


        CameraType currentCamera;
        bool running = true;

        List<Renderer> listRender = new List<Renderer>();

        UIElementRenderer elementRenderer;
        ContentManager contentManager;
        GameTimer timer;
        SpriteBatch spriteBatch;
        CustomDialog displayPopup;
        void XNARendering_LayoutUpdated(object sender, EventArgs e)
        {
            // make sure page size is valid
            if (ActualWidth == 0 || ActualHeight == 0)
                return;

            // see if we already have the right sized renderer
            if (elementRenderer != null &&
                elementRenderer.Texture != null &&
                elementRenderer.Texture.Width == (int)ActualWidth &&
                elementRenderer.Texture.Height == (int)ActualHeight)
            {
                return;
            }

            // dispose the current renderer
            if (elementRenderer != null)
                elementRenderer.Dispose();

            // create the renderer
            elementRenderer = new UIElementRenderer(this, (int)ActualWidth, (int)ActualHeight);
        }



        double initialAngle;
        double initialScale;
        Vector2 p1;
        Vector2 p2;
        bool gestureRunning = false;
        bool simulateGesture = false;
        bool displayPreview = false;
        DispatcherTimer stopTimer;
        GeneralTransform oldTransformInv;
        TouchCollection LasttouchCollection = new TouchCollection();
        private void OnUpdate(object sender, GameTimerEventArgs e)
        {

            if (stopTimer == null)
            {
                stopTimer = new DispatcherTimer();
                stopTimer.Tick += (a, b) =>
                {
                    /* if (running)
                         ShotImage(null, null);
                     else*/
                    displayPreview = !displayPreview;
                    stopTimer.Stop();
                };
                stopTimer.Interval = TimeSpan.FromSeconds(.5);
            }
            // TODO: Add your update logic here
            int h = (int)Header.ActualHeight;

            if ((RootPivot.SelectedItem as Renderer).UseGesture)
            {


                TouchCollection touchCollection = TouchPanel.GetState();
                Texture2D t = (RootPivot.SelectedItem as Renderer).renderTarget;
                if (t == null) return;
                double fact = 480.0 / t.Width;
                double fact2 = (800.0 - 72.0 - h) / t.Height;
                var rect = fact < fact2 ?
                    new Microsoft.Xna.Framework.Rectangle(0, (int)h + (int)(800 - 72 - h - (t.Height * fact)) / 2, (int)(t.Width * fact), (int)(t.Height * fact)) :
                    new Microsoft.Xna.Framework.Rectangle((int)(480 - (t.Width * fact2)) / 2, (int)h, (int)(t.Width * fact2), (int)(t.Height * fact2));

                foreach (TouchLocation tl in touchCollection)
                {


                    var P = tl.Position;

                    if (P.Y < h)
                        continue;

                    P.Y -= rect.Y;
                    P.X -= rect.X;
                    var f = fact < fact2 ? (float)fact : (float)fact2;
                    P.X /= f;
                    P.Y /= f;
                    TouchLocation old;
                    var delta = new Vector2();
                    if (LasttouchCollection.FindById(tl.Id, out old))
                    {

                        delta.X = (tl.Position.X - old.Position.X) / f;
                        delta.Y = (tl.Position.Y - old.Position.Y) / f;
                    }

                    (RootPivot.SelectedItem as Renderer).gesture(tl.State, P, delta);

                }
                LasttouchCollection = touchCollection;
            }

            while (TouchPanel.IsGestureAvailable)
            {
                GestureSample gs = TouchPanel.ReadGesture();

                if ((RootPivot.SelectedItem as Renderer).UseGesture)
                {
                    continue;

                }


                if (Microsoft.Devices.Environment.DeviceType != DeviceType.Device)
                {
                    switch (gs.GestureType)
                    {
                        case GestureType.FreeDrag:
                            if (gs.Position.Y < h)
                                continue;

                            // move the poem screen vertically by the drag delta
                            // amount.




                            if (simulateGesture)
                            {
                                if (gestureRunning == false)
                                {

                                    gestureRunning = true;
                                    initialAngle = gestureTransform.Rotation;
                                    initialScale = gestureTransform.ScaleX;
                                    p1 = gs.Position;
                                    p2 = new Vector2(240, 400) + (new Vector2(240, 400) - gs.Position);
                                }
                                else
                                {
                                    var f1 = (p1 - p2).Length();

                                    var pp1 = gs.Position;
                                    var pp2 = new Vector2(240, 400) + (new Vector2(240, 400) - gs.Position);

                                    var f2 = (pp1 - pp2).Length();

                                    gestureTransform.ScaleX = gestureTransform.ScaleY = initialScale * f2 / f1;

                                    var v1 = (p1 - p2);
                                    var v2 = (pp1 - pp2);
                                    v1.Normalize();
                                    v2.Normalize();
                                    var tmp = v1.X * v2.Y - v1.Y * v2.X;
                                    float angle = (float)Math.Acos(v1.X * v2.X + v1.Y * v2.Y);
                                    angle = MathHelper.ToDegrees((float)angle);
                                    if (angle > 360) angle -= 360;
                                    if (angle < -360) angle += 360;
                                    gestureTransform.Rotation = initialAngle + (tmp > 0 ? angle : -angle);

                                }

                            }
                            else
                            {

                                gestureRunning = true;

                                gestureTransform.TranslateY += gs.Delta.Y / 800;
                                gestureTransform.TranslateX += gs.Delta.X / 480;
                            }
                            break;
                        case GestureType.DragComplete:
                            gestureRunning = false;
                            break;
                        case GestureType.Tap:
                            if (gs.Position.Y < h)
                                continue;
                            stopTimer.Start();

                            break;
                        case GestureType.DoubleTap:
                            if (gs.Position.Y < h)
                                continue;
                            stopTimer.Stop();
                            // move the poem screen vertically by the drag delta
                            // amount.
                            simulateGesture = !simulateGesture;


                            break;


                    }






                }
                else
                {
                    switch (gs.GestureType)
                    {
                        case GestureType.FreeDrag:
                            if (gs.Position.Y < h)
                                continue;



                            Texture2D tt = (RootPivot.SelectedItem as Renderer).renderTarget;
                            if (tt == null) continue;
                            gestureRunning = true;
                            double factt = 480.0 / tt.Width;
                            double fact2t = (800.0 - 72.0 - h) / tt.Height;
                            var rectt = factt < fact2t ?
                                new Microsoft.Xna.Framework.Rectangle(0, (int)h + (int)(800 - 72 - h - (tt.Height * factt)) / 2, (int)(tt.Width * factt), (int)(tt.Height * factt)) :
                                new Microsoft.Xna.Framework.Rectangle((int)(480 - (tt.Width * fact2t)) / 2, (int)h, (int)(tt.Width * fact2t), (int)(tt.Height * fact2t));

                            var ft = factt < fact2t ? (float)factt : (float)fact2t;
                            var delta = gs.Delta;

                            delta.X /= ft;
                            delta.Y /= ft;
                            delta.X /= tt.Width;
                            delta.Y /= tt.Height;



                            gestureTransform.TranslateX += delta.X;
                            gestureTransform.TranslateY += delta.Y; ;
                            break;

                        case GestureType.DoubleTap:
                            if (gs.Position.Y < h)
                                continue;
                            stopTimer.Stop();
                            // move the poem screen vertically by the drag delta
                            // amount.
                            gestureTransform.Rotation = 0;
                            gestureTransform.ScaleX = gestureTransform.ScaleY = 1.0;
                            gestureTransform.TranslateX = gestureTransform.TranslateY = 0;
                            gestureTransform.CenterX = 0.5;
                            gestureTransform.CenterY = 0.5;
                            break;

                        case GestureType.Tap:
                            if (gs.Position.Y < h)
                                continue;
                            stopTimer.Start();

                            break;

                        case GestureType.Pinch:
                            if (true)
                            {
                                if (gestureRunning == false)
                                {
                                    if (gs.Position.Y < h)
                                        continue;


                                    Texture2D t = (RootPivot.SelectedItem as Renderer).renderTarget;
                                    if (t == null) continue;

                                    gestureRunning = true;


                                    initialAngle = gestureTransform.Rotation;
                                    initialScale = gestureTransform.ScaleX;
                                    p1 = gs.Position;
                                    p2 = gs.Position2;

                                    double fact = 480.0 / t.Width;
                                    double fact2 = (800.0 - 72.0 - h) / t.Height;
                                    var rect = fact < fact2 ?
                                        new Microsoft.Xna.Framework.Rectangle(0, (int)h + (int)(800 - 72 - h - (t.Height * fact)) / 2, (int)(t.Width * fact), (int)(t.Height * fact)) :
                                        new Microsoft.Xna.Framework.Rectangle((int)(480 - (t.Width * fact2)) / 2, (int)h, (int)(t.Width * fact2), (int)(t.Height * fact2));

                                    var f = fact < fact2 ? (float)fact : (float)fact2;

                                    p1.Y -= rect.Y;
                                    p1.X -= rect.X;
                                    p1.X /= f;
                                    p1.Y /= f;
                                    p1.X /= t.Width;
                                    p1.Y /= t.Height;

                                    p2.Y -= rect.Y;
                                    p2.X -= rect.X;
                                    p2.X /= f;
                                    p2.Y /= f;
                                    p2.X /= t.Width;
                                    p2.Y /= t.Height;

                                    var toto = gestureTransform.Inverse.Transform(new System.Windows.Point(p1.X, p1.Y));
                                    gestureTransform.CenterX = toto.X;
                                    gestureTransform.CenterY = toto.Y;
                                    gestureTransform.TranslateX = p1.X - toto.X;
                                    gestureTransform.TranslateY = p1.Y - toto.Y;

                                    oldTransformInv = gestureTransform.Inverse;

                                }
                                else
                                {
                                    var pp1 = gs.Position;
                                    var pp2 = gs.Position2;

                                    Texture2D t = (RootPivot.SelectedItem as Renderer).renderTarget;
                                    if (t == null) continue;
                                    double fact = 480.0 / t.Width;
                                    double fact2 = (800.0 - 72.0 - h) / t.Height;
                                    var rect = fact < fact2 ?
                                        new Microsoft.Xna.Framework.Rectangle(0, (int)h + (int)(800 - 72 - h - (t.Height * fact)) / 2, (int)(t.Width * fact), (int)(t.Height * fact)) :
                                        new Microsoft.Xna.Framework.Rectangle((int)(480 - (t.Width * fact2)) / 2, (int)h, (int)(t.Width * fact2), (int)(t.Height * fact2));

                                    var f = fact < fact2 ? (float)fact : (float)fact2;

                                    pp1.Y -= rect.Y;
                                    pp1.X -= rect.X;
                                    pp1.X /= f;
                                    pp1.Y /= f;
                                    pp1.X /= t.Width;
                                    pp1.Y /= t.Height;

                                    pp2.Y -= rect.Y;
                                    pp2.X -= rect.X;
                                    pp2.X /= f;
                                    pp2.Y /= f;
                                    pp2.X /= t.Width;
                                    pp2.Y /= t.Height;

                                    var toto1 = oldTransformInv.Transform(new System.Windows.Point(pp1.X, pp1.Y));
                                    var toto2 = oldTransformInv.Transform(new System.Windows.Point(pp2.X, pp2.Y));
                                    var titi1 = oldTransformInv.Transform(new System.Windows.Point(p1.X, p1.Y));
                                    var titi2 = oldTransformInv.Transform(new System.Windows.Point(p2.X, p2.Y));
                                    // gestureTransform.CenterX = toto.X;
                                    // gestureTransform.CenterY = toto.Y;
                                    gestureTransform.TranslateX = pp1.X - gestureTransform.CenterX;
                                    gestureTransform.TranslateY = pp1.Y - gestureTransform.CenterY;

                                    gestureTransform.ScaleX = gestureTransform.ScaleY = (pp1 - pp2).Length()
                                         /
                                         (new Vector2((float)titi1.X, (float)titi1.Y) - new Vector2((float)titi2.X, (float)titi2.Y)).Length();

                                    var v1 = (pp1 - pp2);
                                    v1.Normalize();
                                    var v2 = (new Vector2((float)titi1.X, (float)titi1.Y) - new Vector2((float)titi2.X, (float)titi2.Y));
                                    v2.Normalize();

                                    var tmp = v1.X * v2.Y - v1.Y * v2.X;
                                    float angle = (float)Math.Acos(v1.X * v2.X + v1.Y * v2.Y);
                                    angle = MathHelper.ToDegrees((float)angle);
                                    if (angle > 360) angle -= 360;
                                    if (angle < -360) angle += 360;
                                    gestureTransform.Rotation = (tmp < 0 ? angle : -angle);










                                }
                            }
                            else
                            {
                                if (gestureRunning == false)
                                {
                                    if (gs.Position.Y < h)
                                        continue;
                                    gestureRunning = true;

                                    initialAngle = gestureTransform.Rotation;
                                    initialScale = gestureTransform.ScaleX;
                                    p1 = gs.Position;
                                    p2 = gs.Position2;

                                    var toto = gestureTransform.Inverse.Transform(new System.Windows.Point(0.5, 0.5));
                                    gestureTransform.CenterX = toto.X;
                                    gestureTransform.CenterY = toto.Y;
                                    gestureTransform.TranslateX = 0.5 - toto.X;
                                    gestureTransform.TranslateY = 0.5 - toto.Y;
                                }
                                else
                                {
                                    var f1 = (p1 - p2).Length();
                                    var f2 = (gs.Position - gs.Position2).Length();

                                    gestureTransform.ScaleX = gestureTransform.ScaleY = initialScale * f2 / f1;

                                    var v1 = (p1 - p2);
                                    var v2 = (gs.Position - gs.Position2);
                                    v1.Normalize();
                                    v2.Normalize();
                                    var tmp = v1.X * v2.Y - v1.Y * v2.X;
                                    float angle = (float)Math.Acos(v1.X * v2.X + v1.Y * v2.Y);
                                    angle = MathHelper.ToDegrees((float)angle);
                                    if (angle > 360) angle -= 360;
                                    if (angle < -360) angle += 360;
                                    gestureTransform.Rotation = initialAngle + (tmp > 0 ? angle : -angle);
                                }
                            }

                            break;
                        case GestureType.PinchComplete:

                            gestureRunning = false;
                            break;
                        case GestureType.DragComplete:
                            gestureRunning = false;
                            break;
                    }
                }
            }



        }

        /// <summary>
        /// Allows the page to draw itself.
        /// </summary>
        private void OnDraw(object sender, GameTimerEventArgs e)
        {


            if (Camera != null)
            {
                try
                {

                    Camera.GetPreviewBufferArgb32(CameraBitmap.Pixels);

                    var buff = CameraBitmap.Pixels;
                    for (int i = 0; i < buff.Length; ++i)
                    {

                        buff[i] = (int)((uint)buff[i] & 0xFF00FF00)
                                        |
                                        (buff[i] >> 16 & 0xFF)
                                        |
                                        (buff[i] & 0xFF) << 16;
                    }

                    CameraBitmap.Invalidate();


                }
                catch (Exception exp)
                {
                }
            }


            elementRenderer.Render();


            try
            {

                if ((RootPivot.SelectedItem as Renderer).update != null && CameraBitmap != null)
                    (RootPivot.SelectedItem as Renderer).update(CameraBitmap);
            }
            catch (Exception exp)
            {
            }
            bool darkTheme = ((Visibility)Application.Current.Resources["PhoneDarkThemeVisibility"] == Visibility.Visible);
            if (darkTheme)
                SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.Black);
            else
                SharedGraphicsDeviceManager.Current.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.White);


            spriteBatch.Begin();
            try
            {
                if (RootPivot.SelectedItem != null && (RootPivot.SelectedItem as Renderer).renderTarget != null)
                {
                    int h = (int)Header.ActualHeight;
                    Texture2D t = (RootPivot.SelectedItem as Renderer).renderTarget;
                    double fact = 480.0 / t.Width;
                    double fact2 = (800.0 - 72.0 - h) / t.Height;
                    var rect = fact < fact2 ?
                        new Microsoft.Xna.Framework.Rectangle(0, (int)h + (int)(800 - 72 - h - (t.Height * fact)) / 2, (int)(t.Width * fact), (int)(t.Height * fact)) :
                        new Microsoft.Xna.Framework.Rectangle((int)(480 - (t.Width * fact2)) / 2, (int)h, (int)(t.Width * fact2), (int)(t.Height * fact2));


                    spriteBatch.Draw(t, rect, Microsoft.Xna.Framework.Color.White);

                    if ((/*running ||*/ gestureRunning || displayPreview) && (RootPivot.SelectedItem as Renderer).previewTarget != null)
                    {
                        spriteBatch.Draw((RootPivot.SelectedItem as Renderer).previewTarget, rect, Microsoft.Xna.Framework.Color.White);

                    }



                }
                spriteBatch.Draw(elementRenderer.Texture, Vector2.Zero, Microsoft.Xna.Framework.Color.White);
            }
            catch (Exception exp)
            {

            }
            spriteBatch.End();

            // TODO: Add your drawing code here
        }


        // Constructor
        public MainPage()
        {
            InitializeComponent();
            contentManager = (Application.Current as App).Content;

            // Create a timer for this page
            timer = new GameTimer();

            timer.UpdateInterval = TimeSpan.FromTicks(333333);
            timer.Update += OnUpdate;
            timer.Draw += OnDraw;
            LayoutUpdated += new EventHandler(XNARendering_LayoutUpdated);


            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary))
                currentCamera = CameraType.Primary;
            else if (PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                currentCamera = CameraType.FrontFacing;


            CameraButtons.ShutterKeyHalfPressed += FocusCapture;

            // The event is fired when the shutter button receives a full press.



            CameraButtons.ShutterKeyPressed += (s, e) =>
                {
                    ShotImage(s, e);

                };

            // The event is fired when the shutter button is released.
            CameraButtons.ShutterKeyReleased += CancelFocusCapture;


            FirstMenu();

            if (App.IsTrial)
            {
                displayPopup = new CustomDialog("");
                displayPopup.Visibility = System.Windows.Visibility.Collapsed;
                toto.Children.Add(displayPopup);
                displayPopup.Closed += () =>
                {
                    displayPopup.Visibility = System.Windows.Visibility.Collapsed;
                };
            }


            Loaded += (s, e) =>
                {
                    if (running)
                    {
                        StartCamera();
                        CaptureMenu();
                    }

                };

            TouchPanel.EnabledGestures = GestureType.Tap | GestureType.DoubleTap | GestureType.FreeDrag | GestureType.DragComplete | GestureType.Pinch | GestureType.PinchComplete;

        }




        private void UninitilizeCamera()
        {
            if (PreviewCanvas.Background == CameraBrush)
                PreviewCanvas.Background = null;
            CameraBrush = null;
            if (Camera != null)
                Camera.Dispose();
            Camera = null;
        }

        private void InitilizeCamera()
        {
            try
            {
                if (Camera != null)
                {
                    UninitilizeCamera();
                }
                if (currentCamera == CameraType.Primary || currentCamera == CameraType.FrontFacing)
                {
                    Camera = new PhotoCamera(currentCamera);
                }
                else
                {
                    MessageBox.Show("Cannot find a camera on this device");
                    return;
                }
                CameraBrush = new VideoBrush();
                CameraBrush.SetSource(Camera);
                PreviewCanvas.Background = CameraBrush;
                CameraBrush.RelativeTransform = inputTransform;

                Camera.Initialized += (s2, evv) =>
                {
                    Deployment.Current.Dispatcher.BeginInvoke(delegate()
                    {
                        if (Camera == null)
                            return;
                        try
                        {

                            running = true;
                            var bitmap = new WriteableBitmap((int)Camera.PreviewResolution.Width, (int)Camera.PreviewResolution.Height);
                            var bpix = bitmap.Pixels;
                            for (int i = 0; i < bpix.Length; ++i)
                                bpix[i] = 0;

                            if (currentCamera == CameraType.Primary && Camera.PreviewResolution.Width > Camera.PreviewResolution.Height)
                            {
                                RotateTransform rotate = new RotateTransform();
                                rotate.CenterX = 0.5;
                                rotate.CenterY = 0.5;
                                rotate.Angle = 90;
                                setImage(bitmap, rotate, false);
                            }
                            else if (currentCamera == CameraType.FrontFacing && Camera.PreviewResolution.Width > Camera.PreviewResolution.Height)
                            {
                                RotateTransform rotate = new RotateTransform();
                                rotate.CenterX = 0.5;
                                rotate.CenterY = 0.5;
                                rotate.Angle = -90;
                                setImage(bitmap, rotate, false);
                            }
                            else
                            {
                                setImage(bitmap, null, false);
                            }
                        }
                        catch (Exception exp)
                        {
                        }
                    }
                );
                };
            }
            catch (Exception)
            {
            }
        }


        private void FocusCamera()
        {
            try
            {
                Camera.Focus();
            }
            catch (Exception)
            {

            }

        }


        private void CancelFocusCamera()
        {
            try
            {
                Camera.CancelFocus();
            }
            catch (Exception)
            {

            }

        }


        private void StartCamera()
        {
            try
            {

                InitilizeCamera();


                // CameraTimer.Start();
                FocusCamera();

            }
            catch (Exception e)
            {

            }

        }
        private void StopCamera()
        {
            try
            {
                running = false;
                UninitilizeCamera();


                //  PreviewCanvas.Background = brush;


            }
            catch (Exception)
            {

            }
        }


        void FirstMenu()
        {


            // Create a new menu item with the localized string from AppResources.

            ApplicationBarMenuItem appBarMenuItemReset = new ApplicationBarMenuItem("Reset position");
            appBarMenuItemReset.Click += (e, s) =>
            {
                gestureTransform.Rotation = 0;
                gestureTransform.ScaleX = gestureTransform.ScaleY = 1.0;
                gestureTransform.TranslateX = gestureTransform.TranslateY = 0;
                gestureTransform.CenterX = 0.5;
                gestureTransform.CenterY = 0.5;

            };
            ApplicationBar.MenuItems.Add(appBarMenuItemReset);
            ApplicationBarMenuItem appBarMenuItemToogle = new ApplicationBarMenuItem("Toggle preview display");
            appBarMenuItemToogle.Click += (e, s) =>
            {
                displayPreview = !displayPreview;
            };
            ApplicationBar.MenuItems.Add(appBarMenuItemToogle);





            ApplicationBarMenuItem appBarMenuItemRank = new ApplicationBarMenuItem("Rank");
            appBarMenuItemRank.Click += (e, s) =>
            {
                try
                {
                    MarketplaceReviewTask marketplaceReviewTask = new MarketplaceReviewTask();
                    marketplaceReviewTask.Show();
                }
                catch (Exception)
                {
                }
            };
            ApplicationBar.MenuItems.Add(appBarMenuItemRank);





            // Create a new menu item with the localized string from AppResources.
            ApplicationBarMenuItem appBarMenuItemAbout = new ApplicationBarMenuItem("ABout");
            appBarMenuItemAbout.Click += (e, s) =>
            {
                try
                {
                    NavigationService.Navigate(new Uri("/about.xaml", UriKind.Relative));
                }
                catch (Exception)
                {
                }
            };
            ApplicationBar.MenuItems.Add(appBarMenuItemAbout);




        }

        void CaptureMenu()
        {
            ApplicationBar.Buttons.Clear();

            ApplicationBarIconButton appStartButton = new ApplicationBarIconButton(new Uri("data/appbar.feature.camera.rest.png", UriKind.Relative));
            appStartButton.Text = "Shot";
            appStartButton.Click += (e, s) =>
            {
                try
                {
                    ShotImage(null, null);

                }
                catch (Exception)
                {
                }
            };
            ApplicationBar.Buttons.Add(appStartButton);





            ApplicationBarIconButton appStartButtonSwitch = new ApplicationBarIconButton(new Uri("data/appbar.os.chromium.png", UriKind.Relative));
            appStartButtonSwitch.Text = "Switch";
            appStartButtonSwitch.Click += (e, s) =>
            {
                try
                {
                    if (currentCamera == CameraType.FrontFacing)
                    {
                        currentCamera = CameraType.Primary;
                        UninitilizeCamera();
                        InitilizeCamera();

                    }
                    else if (currentCamera == CameraType.Primary)
                    {
                        currentCamera = CameraType.FrontFacing;
                        UninitilizeCamera();
                        InitilizeCamera();
                    }

                }
                catch (Exception)
                {
                }
            };
            if (PhotoCamera.IsCameraTypeSupported(CameraType.Primary) && PhotoCamera.IsCameraTypeSupported(CameraType.FrontFacing))
                ApplicationBar.Buttons.Add(appStartButtonSwitch);

            ApplicationBarIconButton appStartimage = new ApplicationBarIconButton(new Uri("data/appbar.image.select.png", UriKind.Relative));
            appStartimage.Text = "Image";
            appStartimage.Click += (e, s) =>
            {
                try
                {

                    var toto = new PhotoChooserTask();
                    toto.ShowCamera = true;
                    toto.Completed += (res, aa) =>
                    {
                        if (aa.TaskResult == TaskResult.OK)
                        {


                            Dispatcher.BeginInvoke(() =>
                            {
                                loadStream(aa.ChosenPhoto);
                            });
                        }



                    };
                    toto.Show();
                }
                catch (Exception exp)
                {


                }
            };
            ApplicationBar.Buttons.Add(appStartimage);




            ApplicationBarIconButton appFocusButton = new ApplicationBarIconButton(new Uri("data/appbar.map.position.rest.png", UriKind.Relative));
            appFocusButton.Text = "Focus";
            appFocusButton.Click += (e, s) =>
            {
                try
                {
                    FocusCapture(null, null);
                }
                catch (Exception)
                {
                }
            };

            ApplicationBar.Buttons.Add(appFocusButton);

        }
        void updateLiveTiles(ShellTile tile, Picture p, Picture p2)
        {
            if (tile != null && p != null)
            {
                var name = "/Shared/ShellContent/testtile.jpg";
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var filename = name;
                    using (var st = new IsolatedStorageFileStream(filename, FileMode.Create, FileAccess.Write, store))
                    {
                        p.GetImage().CopyTo(st);
                    }
                }

                var nameBack = "/Shared/ShellContent/testtileBack.jpg";
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var filename = nameBack;
                    using (var st = new IsolatedStorageFileStream(filename, FileMode.Create, FileAccess.Write, store))
                    {
                        p2.GetImage().CopyTo(st);
                    }
                }
                StandardTileData NewTileData = new StandardTileData
                {
                    BackgroundImage = new Uri("isostore:" + name, UriKind.Absolute),
                    Count = 0,
                    BackTitle = "Monster Cam",
                    BackBackgroundImage = new Uri("isostore:" + nameBack, UriKind.Absolute),
                    BackContent = ""
                };

                // Update the Application Tile
                tile.Update(NewTileData);


            }
        }
        void updateLiveTiles(Picture p)
        {
            try
            {
                using (MediaLibrary mediaLibrary = new MediaLibrary())
                {
                    var L = mediaLibrary.Pictures.Where((t) => { return t.Name.Contains("MonsterCam"); });
                    var rand = new Random();
                    int count = 0;
                    foreach (var tile in ShellTile.ActiveTiles)
                    {


                        var cp = p;
                        if (L.Count() > 0)
                        {
                            int id = rand.Next(L.Count());
                            cp = L.ElementAt(id);

                        }

                        updateLiveTiles(tile, p, cp);

                        count++;
                    };
                }
            }
            catch (Exception)
            {
            }
        }

        void AfterCaptureMenu()
        {
            ApplicationBar.Buttons.Clear();

            ApplicationBarIconButton appStartButton = new ApplicationBarIconButton(new Uri("data/appbar.feature.video.rest.png", UriKind.Relative));
            appStartButton.Text = "Shot";
            appStartButton.Click += (e, s) =>
            {
                try
                {
                    StartCapture(null, null);
                }
                catch (Exception)
                {
                }
            };

            ApplicationBar.Buttons.Add(appStartButton);

            ApplicationBarIconButton appStartimage = new ApplicationBarIconButton(new Uri("data/appbar.image.select.png", UriKind.Relative));
            appStartimage.Text = "Image";
            appStartimage.Click += (e, s) =>
            {
                try
                {
                    var toto = new PhotoChooserTask();
                    toto.ShowCamera = true;
                    toto.Completed += (res, aa) =>
                    {
                        if (aa.TaskResult == TaskResult.OK)
                        {


                            Dispatcher.BeginInvoke(() =>
                            {

                                loadStream(aa.ChosenPhoto);
                            });
                        }



                    };
                    toto.Show();
                }
                catch (Exception exp)
                {


                }
            };
            ApplicationBar.Buttons.Add(appStartimage);


            ApplicationBarIconButton appSaveButton = new ApplicationBarIconButton(new Uri("data/appbar.save.rest.png", UriKind.Relative));
            appSaveButton.Text = "Save";
            appSaveButton.Click += (e, s) =>
            {


                

                try
                {

                    if ((RootPivot.SelectedItem as Renderer).update != null && FullCameraBitmap != null)
                        (RootPivot.SelectedItem as Renderer).update(FullCameraBitmap);


                    using (MemoryStream stream = new MemoryStream())
                    {
                        Texture2D text = (RootPivot.SelectedItem as Renderer).renderTarget;
                        text.SaveAsJpeg(stream, text.Width, text.Height);

                        stream.Seek(0, SeekOrigin.Begin);
                        Picture toto;
                        using (MediaLibrary mediaLibrary = new MediaLibrary())
                            toto = mediaLibrary.SavePictureToCameraRoll(String.Format("MonsterCam {0:yyyyMMdd-HHmmss}.png", DateTime.Now), stream);
                        updateLiveTiles(toto);
                    }
                    if (displayPopup != null)
                    {
                        displayPopup.message = "Picture saved...";
                        displayPopup.Visibility = System.Windows.Visibility.Visible;
                    }
                    else
                    {
                        MessageBox.Show("Picture saved...");
                    }



                }
                catch (Exception exp)
                {
                }
            };
            ApplicationBar.Buttons.Add(appSaveButton);

            ApplicationBarIconButton appShareButton = new ApplicationBarIconButton(new Uri("data/MS_0000s_0009_facebook.png", UriKind.Relative));
            appShareButton.Text = "Facebook";
            appShareButton.Click += (e, s) =>
            {

                try
                {
                    if ((RootPivot.SelectedItem as Renderer).update != null && FullCameraBitmap != null)
                        (RootPivot.SelectedItem as Renderer).update(FullCameraBitmap);

                    using (MemoryStream stream = new MemoryStream())
                    {

                        Texture2D text = (RootPivot.SelectedItem as Renderer).renderTarget;
                        text.SaveAsJpeg(stream, text.Width, text.Height);
                        stream.Seek(0, SeekOrigin.Begin);
                        App.AddOrUpdateValue("image_selected", stream.ToArray());

                    }


                    NavigationService.Navigate(new Uri("/Facebook.xaml", UriKind.Relative));

                }
                catch (Exception)
                {
                }
            };

            ApplicationBar.Buttons.Add(appShareButton);

        }

        void setImage(WriteableBitmap bitmap, Transform t = null, bool updatePreview = true)
        {

            inputTransform.Children.Clear();
            if (t != null)
                inputTransform.Children.Add(t);
            inputTransform.Children.Add(gestureTransform);

            var P1 = t != null ? t.Transform(new System.Windows.Point()) : new System.Windows.Point();
            var P2 = t != null ? t.Transform(new System.Windows.Point(bitmap.PixelWidth, bitmap.PixelHeight)) : new System.Windows.Point(bitmap.PixelWidth, bitmap.PixelHeight);
            var outputSize = new Size(Math.Abs(P1.X - P2.X), Math.Abs(P1.Y - P2.Y));
            CameraBitmap = bitmap;

            FullCameraBitmap = null;
            var buff = CameraBitmap.Pixels;
            for (int i = 0; i < buff.Length; ++i)
            {

                buff[i] = (int)((uint)buff[i] & 0xFF00FF00)
                                |
                                (buff[i] >> 16 & 0xFF)
                                |
                                (buff[i] & 0xFF) << 16;
            }




            if (updatePreview)
            {
                ImageBrush brush = new ImageBrush();
                brush.ImageSource = CameraBitmap;
                brush.RelativeTransform = inputTransform;
                PreviewCanvas.Background = brush;
            }
            double max = outputSize.Width > outputSize.Height ? outputSize.Width : outputSize.Height;

            double fact = max > 600 ? 600.0 / max : 1;
            outputSize = new Size(fact * outputSize.Width, fact * outputSize.Height);



            PreviewCanvas.Width = 0.2 * outputSize.Width;
            PreviewCanvas.Height = 0.2 * outputSize.Height;
            foreach (Renderer r in listRender)
            {
                r.Transform = inputTransform;
                r.Size = outputSize;
            }

        }
        void loadStream(Stream s, Transform t = null)
        {
            Dispatcher.BeginInvoke(() =>
            {

                StopCapture(null, null);
                restartCamera = false;


                BitmapImage bitmap = new BitmapImage();
                bitmap.CreateOptions = BitmapCreateOptions.None;
                s.Seek(0, SeekOrigin.Begin);
                bitmap.SetSource(s);
                var tmp = new WriteableBitmap(bitmap);


                if (tmp.PixelWidth < 800 && tmp.PixelHeight < 800)
                {
                    CameraBitmap = tmp;
                    tmp = null;
                }
                else
                {
                    int fact = Math.Max(tmp.PixelWidth, tmp.PixelHeight) / 800;
                    CameraBitmap = new WriteableBitmap(tmp.PixelWidth / fact, tmp.PixelHeight / fact);
                    for (int i = 0; i < CameraBitmap.PixelHeight; ++i)
                        for (int j = 0; j < CameraBitmap.PixelWidth; ++j)
                            CameraBitmap.Pixels[i * CameraBitmap.PixelWidth + j] = tmp.Pixels[(fact * i) * tmp.PixelWidth + fact * j];


                }
                if (t == null)
                {
                    s.Seek(0, SeekOrigin.Begin);
                    JpegInfo info = ExifLib.ExifReader.ReadJpeg(s, "");
                    s.Seek(0, SeekOrigin.Begin);


                    if (info.Orientation == ExifOrientation.TopRight)
                    {
                        RotateTransform rotate = new RotateTransform();
                        rotate.CenterX = 0.5;
                        rotate.CenterY = 0.5;
                      /*  if (App.IsWP8)
                            rotate.Angle = -90;
                        else*/
                            rotate.Angle = 90;
                        t = rotate;
                    }
                    if (info.Orientation == ExifOrientation.BottomRight)
                    {
                        RotateTransform rotate = new RotateTransform();
                        rotate.CenterX = 0.5;
                        rotate.CenterY = 0.5;
                        rotate.Angle = 180;
                        t = rotate;
                    }
                    if (info.Orientation == ExifOrientation.BottomLeft)
                    {
                        RotateTransform rotate = new RotateTransform();
                        rotate.CenterX = 0.5;
                        rotate.CenterY = 0.5;
                       /* if (App.IsWP8)
                            rotate.Angle = 90;
                        else*/
                            rotate.Angle = -90;
                        t = rotate;
                    }
                }
                setImage(CameraBitmap, t);
                FullCameraBitmap = tmp;
                if (FullCameraBitmap != null)
                {
                    var buff = FullCameraBitmap.Pixels;
                    for (int i = 0; i < buff.Length; ++i)
                    {

                        buff[i] = (int)((uint)buff[i] & 0xFF00FF00)
                                        |
                                        (buff[i] >> 16 & 0xFF)
                                        |
                                        (buff[i] & 0xFF) << 16;
                    }
                    FullCameraBitmap.Invalidate();
                }
                AfterCaptureMenu();
            });


        }
        bool restartCamera = false;
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);
            // Create a new SpriteBatch, which can be used to draw textures.
            if (listRender.Count == 0)
            {
                listRender.Add(new Manual());
                listRender.Add(new Toto());
                listRender.Add(new MirorRenderer());
                listRender.Add(new RandomRenderer2());
                listRender.Add(new Frog());
                listRender.Add(new Lips());

                RootPivot.ItemsSource = listRender;
                var rand = new Random();
                RootPivot.SelectedIndex = rand.Next(4);
            }

            foreach (Renderer r in listRender)
            {
                r.Stop();
            }
            (RootPivot.SelectedItem as Renderer).Start();
            // Previewelements.DataContext = (RootPivot.SelectedItem as Renderer);
            DataContext = RootPivot.SelectedItem;
            displayPreview = true;

            spriteBatch = new SpriteBatch(SharedGraphicsDeviceManager.Current.GraphicsDevice);


            // TODO: use this.content to load your game content here

            // Start the timer
            timer.Start();
            IDictionary<string, string> queryStrings = this.NavigationContext.QueryString;

            //queryStrings["token"] = "{523e20af-829c-e536-20f3-f1a57fea12b3}";
            if (queryStrings.ContainsKey("token"))
            {






                /*if (displayPopup != null)
                {
                    displayPopup.message = queryStrings["token"];
                    displayPopup.Visibility = System.Windows.Visibility.Visible;
                }
                else
                {
                    MessageBox.Show(queryStrings["token"]);
                }*/

                MediaLibrary library = new MediaLibrary();
                Picture picture = library.GetPictureFromToken(queryStrings["token"]);
                loadStream(picture.GetImage());



                queryStrings.Remove("token");






            }
            else if (restartCamera)
            {
                StartCapture(null, null);

            }

            restartCamera = false;
            base.OnNavigatedTo(e);

        }
        protected override void OnNavigatingFrom(System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            try
            {
                timer.Stop();
                if (running)
                {
                    restartCamera = true;
                    StopCapture(null, null);
                }
            }
            catch (Exception exp)
            {
            }
            try
            {
                SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(false);
            }
            catch (Exception exp)
            {
            }

            base.OnNavigatingFrom(e);
        }



        private void StartCapture(object sender, EventArgs e)
        {
            if (!running || Camera == null)
            {


                // PreviewCanvas.Visibility = System.Windows.Visibility.Visible;
                StartCamera();

                CaptureMenu();
            }



        }

        private void ShotImage(object sender, EventArgs e)
        {
            if (running)
            {

                Camera.CaptureImageAvailable += (a, b) =>
                    {
                        Dispatcher.BeginInvoke(() =>
                            {

                                if (Camera != null && b.ImageStream != null)
                                    loadStream(b.ImageStream);

                            });
                    };
                try
                {
                    Camera.CaptureImage();
                }
                catch (Exception exp)
                {
                }
            }
        }

        private void StopCapture(object sender, EventArgs e)
        {
            if (running || Camera != null)
            {


                //PreviewCanvas.Visibility = System.Windows.Visibility.Collapsed;
                StopCamera();
            }


        }


        private void FocusCapture(object sender, EventArgs e)
        {
            if (running)
            {
                FocusCamera();
            }
        }

        private void CancelFocusCapture(object sender, EventArgs ev)
        {
            CancelFocusCamera();

        }

        /* private void mirorCanvas_Tap(object sender, GestureEventArgs e)
         {
             StopCapture(sender, e);
         }*/

        private void RootPivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

            /* SharedGraphicsDeviceManager.Current.GraphicsDevice.SetSharingMode(true);
             timer.Start();
             foreach (Renderer r in listRender)
             {
                 r.Stop();
             }
             (RootPivot.SelectedItem as Renderer).Start();
             // Previewelements.DataContext = (RootPivot.SelectedItem as Renderer);
             DataContext = RootPivot.SelectedItem;
             displayPreview = true;*/

        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
        }
        protected override void OnBackKeyPress(System.ComponentModel.CancelEventArgs e)
        {
            //Check if the PopUp window is open
            if (displayPopup != null && displayPopup.Visibility == System.Windows.Visibility.Visible)
            {
                //Close the PopUp Window
                displayPopup.Visibility = System.Windows.Visibility.Collapsed;

                //Keep the back button from
                //navigating away from the current page
                e.Cancel = true;
            }
            else
            {
                //There is no PopUp open, use the back button normally
                base.OnBackKeyPress(e);
            }
        }
    }
}