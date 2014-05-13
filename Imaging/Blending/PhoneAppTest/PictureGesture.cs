using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using Nokia.Graphics.Imaging;
using System.Windows.Controls;
using System.IO;
using System.Windows.Input;
using Windows.Foundation;
using System.Windows.Media;
using Microsoft.Xna.Framework.Media;

using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Storage.Streams;
using System.Windows.Shapes;

namespace PhoneAppTest
{
    class PictureGesture : IDisposable
    {
        Size inputSize = new Size();

        WriteableBitmap InputLR;
        Rectangle  output = null;
        ImageBrush brush = new ImageBrush();

        Size outputSize = new Size();
        Point originPos = new Point();
        double originScale = 1.0;
        double currentAngle = 0.0;

        public Size ImageSize   {get{return  inputSize;}}
        public Size AreaSize    {get{return  outputSize;}}
        public Point Pos        {get{return  originPos;}}
        public double Scale     {get{return  originScale;}}
        public double Angle     {get{return  originAngle;}}


        Point currentPos = new Point();
        double currentScale = 1.0;
        double originAngle = 0.0;



        async void setInput(Stream s)
        {
           
            //reset stream position
            s.Seek(0, SeekOrigin.Begin);
            //create a session
            using (var source = new StreamImageSource(s))
            using (var renderer = new WriteableBitmapRenderer(source))
            {
                var info = await source.GetInfoAsync();
                InputLR = new WriteableBitmap(
                                (int)( info.ImageSize.Width),
                                (int)(info.ImageSize.Height)
                                );

                renderer.WriteableBitmap = InputLR;
                await renderer.RenderAsync();
                inputSize = new Size() { Width = info.ImageSize.Width, Height = info.ImageSize.Height };
                currentPos = new Point(info.ImageSize.Width / 2, info.ImageSize.Height / 2);
                if (info.ImageSize.Width > info.ImageSize.Height)
                {
                    currentScale = outputSize.Height / info.ImageSize.Height;
                }
                else
                {
                    currentScale = outputSize.Width / info.ImageSize.Width;
                }
                currentAngle = 0.0;
            }
            saveLastPossaveLastPositionData();
            brush.ImageSource = InputLR;
            brush.Stretch = Stretch.None;
            brush.AlignmentX = AlignmentX.Left;
            brush.AlignmentY = AlignmentY.Top;

           processRenderingLR();
        }



        //Picture Stream.
        public Stream Input
        {
            set
            {
                setInput(value);
            }
        }

        //UI control output
        public Rectangle Output
        {
            set
            {
                if (output != null)
                {
                    output.ManipulationStarted -= ManipulationStarted;
                    output.ManipulationDelta -= ManipulationDelta;
                    output.ManipulationCompleted -= ManipulationCompleted;
                }
                output = value;

                if (output != null)
                {
                    outputSize = new Size(output.ActualWidth, output.ActualHeight);
                    originPos = new Point();

                    originScale = 1.0;
                    originAngle = 0.0;


                    output.ManipulationStarted += ManipulationStarted;
                    output.ManipulationDelta += ManipulationDelta;
                    output.ManipulationCompleted += ManipulationCompleted;

                    output.Fill = brush;

                   
                }
                processRenderingLR();
            }
        }


       

     
        public void Dispose()
        {
            
        

            if (output != null)
            {
                output.ManipulationStarted -= ManipulationStarted;
                output.ManipulationDelta -= ManipulationDelta;
                output.ManipulationCompleted -= ManipulationCompleted;
            }
            output = null;
        }

    
        #region Gesture

        enum GESTURE_TYPE
        {
            NONE,
            TRANSLATION,
            PINCH

        };
        GESTURE_TYPE oldGestureType = GESTURE_TYPE.NONE;



        // copied from http://www.developer.nokia.com/Community/Wiki/Real-time_rotation_of_the_Windows_Phone_8_Map_Control
        public static double angleBetween2Lines(PinchContactPoints line1, PinchContactPoints line2)
        {
            if (line1 != null && line2 != null)
            {

                double angle1 = Math.Atan2(line1.PrimaryContact.Y - line1.SecondaryContact.Y,
                                           line1.PrimaryContact.X - line1.SecondaryContact.X);
                double angle2 = Math.Atan2(line2.PrimaryContact.Y - line2.SecondaryContact.Y,
                                           line2.PrimaryContact.X - line2.SecondaryContact.X);
                double angle = (angle1 - angle2) * 180 / Math.PI;

                return angle;
            }
            else { return 0.0; }
        }

        public virtual void ManipulationStarted(object sender, ManipulationStartedEventArgs arg)
        {
            oldGestureType = GESTURE_TYPE.NONE;
        }

        void saveLastPossaveLastPositionData()
        {
            originPos = currentPos;
            originScale = currentScale;
            originAngle = currentAngle;
        }


        public virtual void ManipulationDelta(object sender, ManipulationDeltaEventArgs arg)
        {
            if (arg.PinchManipulation != null)
            {
                oldGestureType = GESTURE_TYPE.PINCH;


                var p1 = arg.PinchManipulation.Original.PrimaryContact;
                var p2 = arg.PinchManipulation.Original.SecondaryContact;
                var p3 = arg.PinchManipulation.Current.PrimaryContact;
                var p4 = arg.PinchManipulation.Current.SecondaryContact;


                currentScale = originScale
                    *
                    Math.Sqrt((p4.X - p3.X) * (p4.X - p3.X) + (p4.Y - p3.Y) * (p4.Y - p3.Y))
                    /
                    Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));



                currentAngle = originAngle + angleBetween2Lines(arg.PinchManipulation.Current, arg.PinchManipulation.Original);

                while (currentAngle < 0) currentAngle += 360;
                while (currentAngle > 360) currentAngle -= 360;

                var translation = new System.Windows.Point(
                    arg.PinchManipulation.Current.PrimaryContact.X - arg.PinchManipulation.Original.PrimaryContact.X,
                    arg.PinchManipulation.Current.PrimaryContact.Y - arg.PinchManipulation.Original.PrimaryContact.Y);




                // Translate manipulation
                var originContactPos = arg.PinchManipulation.Original.PrimaryContact;
                {
                    originContactPos.X -= outputSize.Width / 2;
                    originContactPos.Y -= outputSize.Height / 2;

                    CompositeTransform gestureTransform = new CompositeTransform();
                    gestureTransform.Rotation = originAngle;
                    gestureTransform.ScaleX = gestureTransform.ScaleY = originScale;
                    originContactPos = gestureTransform.Inverse.Transform(originContactPos);
                }

                var currentContactPos = arg.PinchManipulation.Current.PrimaryContact;
                {
                    currentContactPos.X -= outputSize.Width / 2;
                    currentContactPos.Y -= outputSize.Height / 2;
                    CompositeTransform gestureTransform = new CompositeTransform();
                    gestureTransform.Rotation = currentAngle;
                    gestureTransform.ScaleX = gestureTransform.ScaleY = currentScale;
                    currentContactPos = gestureTransform.Inverse.Transform(currentContactPos);
                }




                currentPos.X = originPos.X - (currentContactPos.X - originContactPos.X);
                currentPos.Y = originPos.Y - (currentContactPos.Y - originContactPos.Y);

            }
            else
            {
                if (oldGestureType == GESTURE_TYPE.PINCH)
                {
                    saveLastPossaveLastPositionData();

                }

                oldGestureType = GESTURE_TYPE.TRANSLATION;

                var translation = arg.CumulativeManipulation.Translation;
                CompositeTransform gestureTransform = new CompositeTransform();

                gestureTransform.ScaleX = gestureTransform.ScaleY = currentScale;
                gestureTransform.Rotation = currentAngle;
                translation = gestureTransform.Inverse.Transform(translation);

                currentPos.X = originPos.X - translation.X;
                currentPos.Y = originPos.Y - translation.Y;
            }


            processRenderingLR();
        }
        public virtual void ManipulationCompleted(object sender, ManipulationCompletedEventArgs arg)
        {
            saveLastPossaveLastPositionData();
            processRenderingLR();
        }
        #endregion

     
     
        void processRenderingLR()
        {
           
            CompositeTransform gestureTransform = new CompositeTransform();
            gestureTransform.CenterX = currentPos.X;
            gestureTransform.CenterY = currentPos.Y;
            gestureTransform.Rotation = currentAngle;
            gestureTransform.ScaleX = gestureTransform.ScaleY = currentScale;


            gestureTransform.TranslateX = -currentPos.X + outputSize.Width / 2.0;
            gestureTransform.TranslateY = -currentPos.Y + outputSize.Height / 2.0;

            brush.Transform = gestureTransform;

          
        }

        public ReframingFilter CreateReframingFilter()
        {
            var currentSize = new Size(outputSize.Width / originScale, outputSize.Height / originScale);
            var corner = new Point(originPos.X - currentSize.Width / 2, originPos.Y - currentSize.Height / 2);

            return new ReframingFilter(new Windows.Foundation.Rect(corner, currentSize), -originAngle);
        }

    }
}
