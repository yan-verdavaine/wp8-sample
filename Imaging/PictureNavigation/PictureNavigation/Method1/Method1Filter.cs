/*
Copyright (c) 2013 verdavaine yan

Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY 

*/

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
using Microsoft.Phone.Info;

namespace PictureNavigation.Method1
{
    class Method1Filter : IDisposable
    {
        private  double _ScreenToPixelFactor = 0;
        private  double ScreenToPixelFactor
        {
            get
            {
                if (_ScreenToPixelFactor == 0)
                {
                    try
                    {
                        _ScreenToPixelFactor = ((System.Windows.Size)DeviceExtendedProperties.GetValue("PhysicalScreenResolution")).Width / 480;
                    }
                    catch (Exception)
                    {
                        _ScreenToPixelFactor = System.Windows.Application.Current.Host.Content.ScaleFactor / 100.0;
                    }
                }
                return _ScreenToPixelFactor;
            }
        }


       // StreamImageSource source = null;
        BufferImageSource source = null;
        FilterEffect filter = null;
        ReframingFilter reframing = null;
        WriteableBitmapRenderer rendererLR = null;
        WriteableBitmapRenderer rendererHR = null;

        Size inputSize = new Size();


        WriteableBitmap outputBitmapLRTmp;
        WriteableBitmap outputBitmapLR;

        WriteableBitmap outputBitmapHRTmp;
        WriteableBitmap outputBitmapHR;


        Image output = null;
        Size outputSize = new Size();
        Point originPos = new Point();
        double originScale = 1.0;
        double currentAngle = 0.0;

        Point currentPos = new Point();
        double currentScale = 1.0;
        double originAngle = 0.0;


        TimeSpan lastDuration = new TimeSpan();

        TimeSpan sumLRtime = new TimeSpan();
        TimeSpan minLRtime = new TimeSpan();
        TimeSpan maxLRtime = new TimeSpan();
        int nbLRImage = 0;

        TimeSpan sumHRtime = new TimeSpan();
        TimeSpan minHRtime = new TimeSpan();
        TimeSpan maxHRtime = new TimeSpan();
        int nbHRImage = 0;

        async void setInput(Stream s)
        {

            //Dispose old session ressources.
            if (source != null)
            {
                source.Dispose(); source = null;
                filter.Dispose(); filter = null;
                reframing = null;
                rendererLR.Dispose(); rendererLR = null;
                rendererHR.Dispose(); rendererHR = null;
            }
            //reset stream position
            s.Seek(0, SeekOrigin.Begin);

            MemoryStream mstream = new MemoryStream();
            s.CopyTo(mstream);
            var tmpsource = new BufferImageSource(mstream.GetBuffer().AsBuffer());
           // var tmpsource = new StreamImageSource(s);
            {

                var info = await tmpsource.GetInfoAsync();

                filter = new FilterEffect(tmpsource);

                reframing = new ReframingFilter();
                filter.Filters = new IFilter[] { reframing };
                rendererLR = new WriteableBitmapRenderer(filter, outputBitmapLRTmp);
                rendererHR = new WriteableBitmapRenderer(filter, outputBitmapHRTmp);

                inputSize = new Size() { Width = info.ImageSize.Width, Height = info.ImageSize.Height };
                currentPos = new Point(info.ImageSize.Width / 2, info.ImageSize.Height / 2);
                if (info.ImageSize.Width > info.ImageSize.Height)
                {
                    currentScale = output.Height / info.ImageSize.Height;
                }
                else
                {
                    currentScale = output.Width / info.ImageSize.Width;
                }
                currentAngle = 0.0;
                saveLastPossaveLastPositionData();
                GC.Collect();
            }
            source = tmpsource;
            requestProcessing();
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
        public Image Output
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
                    outputSize = new Size(output.Width, output.Height);



                    outputBitmapHR = new WriteableBitmap(
                        (int)(output.Width * ScreenToPixelFactor),
                        (int)(output.Height * ScreenToPixelFactor)
                        );
                    outputBitmapHRTmp = new WriteableBitmap(outputBitmapHR);

                    outputBitmapLR = new WriteableBitmap(
                        (int)(outputBitmapHR.PixelWidth / 2.0 + 0.5),
                        (int)(outputBitmapHR.PixelHeight / 2.0 + 0.5)
                        );
                    outputBitmapLRTmp = new WriteableBitmap(outputBitmapLR);

                    outputSize = new Size(output.Width, output.Height);
                    originPos = new Point();

                    originScale = 1.0;
                    originAngle = 0.0;


                    output.ManipulationStarted += ManipulationStarted;
                    output.ManipulationDelta += ManipulationDelta;
                    output.ManipulationCompleted += ManipulationCompleted;
                    output.Source = outputBitmapLR;
                }
                requestProcessing();
            }
        }

        //extract Oriented ROI
        async public Task<IBuffer> GenerateReframingPicture()
        {

            var currentSize = new Size(
                  outputSize.Width / currentScale,
                  outputSize.Height / currentScale);
            var corner = new Point(currentPos.X - currentSize.Width / 2, currentPos.Y - currentSize.Height / 2);


            reframing.ReframingArea = new Rect(corner, currentSize);
            reframing.Angle = -currentAngle;

            using (var render = new JpegRenderer(filter))
            {
              return await render.RenderAsync();
            }

        }

        public string Info()
        {
            try
            {


                return string.Format("Input size = {0}x{1}\nLR : {2,5:F} ms [{3,5:F} {4,5:F}]\nHR : {5,5:F} ms [{6,5:F} {7,5:F}]\n",
                    inputSize.Width, inputSize.Height,
                    nbLRImage > 0 ? sumLRtime.TotalMilliseconds / nbLRImage : 0,
                    minLRtime.TotalMilliseconds,
                    maxLRtime.TotalMilliseconds,
                    nbHRImage > 0 ? sumHRtime.TotalMilliseconds / nbHRImage : 0,
                    minHRtime.TotalMilliseconds,
                    maxHRtime.TotalMilliseconds,
                    inputSize.Width, inputSize.Height)
                    +
                    string.Format("\nType = {0}\nDuration : {1,5:F} \nScale : {2,5:F}\nAngle : {3,5:F}\nPos :  [{4,5:F} , {5,5:F}]",
                    outputResolution == RESOLUTION.LOW ? "LR" : "HR",
                    lastDuration.TotalMilliseconds,
                    currentScale,
                    currentAngle,
                    currentPos.X, currentPos.Y);


            }
            catch (Exception e)
            {
                return "";
            }


        }

        public void Dispose()
        {
            if (source != null)
            {
                source.Dispose(); source = null;
                filter.Dispose(); filter = null;
                reframing = null;
                rendererLR.Dispose(); rendererLR = null;
                rendererHR.Dispose(); rendererHR = null;
            }
            outputBitmapLRTmp = null;
            outputBitmapLR = null;
            outputBitmapHRTmp = null;
            outputBitmapHR = null;

            if (output != null)
            {
                output.ManipulationStarted -= ManipulationStarted;
                output.ManipulationDelta -= ManipulationDelta;
                output.ManipulationCompleted -= ManipulationCompleted;
            }
            output = null;
        }
       
        #region Interactive State Machine
        enum RESOLUTION
        {
            LOW,
            HIGH
        };

        RESOLUTION outputResolution = RESOLUTION.LOW;

        //State of the Interactive State Machine
        enum STATE
        {
            WAIT,
            APPLY,
            SCHEDULE
        };
        //Current State
        STATE currentState = STATE.WAIT;


        void requestProcessing()
        {
            switch (currentState)
            {
                //State machine transition : WAIT -> APPLY 
                case STATE.WAIT:
                    currentState = STATE.APPLY;
                    //enter in APPLY STATE => apply the filter
                    processRendering();
                    break;

                //State machine transition : APPLY -> SCHEDULE
                case STATE.APPLY:
                    currentState = STATE.SCHEDULE;
                    break;

                //State machine transition : SCHEDULE -> SCHEDULE
                case STATE.SCHEDULE:
                    currentState = STATE.SCHEDULE;
                    break;
            }
        }
        void processFinished()
        {
            switch (currentState)
            {
                //State machine transition : APPLY -> WAIT.  
                case STATE.APPLY:
                    currentState = STATE.WAIT;
                    break;
                //State machine transition : SCHEDULE -> APPLY. 
                case STATE.SCHEDULE:
                    currentState = STATE.APPLY;
                    //enter in APPLY STATE => apply the filter
                    processRendering();
                    break;
            }
        }

        async void processRendering()
        {

            try
            {
                if (output != null && source != null)
                {

                    var currentoutputResolution = outputResolution;
                    await processRenderingLR();

                    if (currentoutputResolution == RESOLUTION.HIGH)
                    {
                        await processRenderingHR();
                    }

                }
            }
            catch (Exception e)
            {
                GC.Collect();
                originScale *= 1.1;
                currentScale *= 1.1;
                requestProcessing();
            }
            finally
            {
                processFinished();
            }


        }


        
        #endregion
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
            outputResolution = RESOLUTION.LOW;
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
                    originContactPos.X -= outputSize.Width / 2.0;
                    originContactPos.Y -= outputSize.Height / 2.0;

                    CompositeTransform gestureTransform = new CompositeTransform();
                    gestureTransform.Rotation = originAngle;
                    gestureTransform.ScaleX = gestureTransform.ScaleY = originScale;
                    originContactPos = gestureTransform.Inverse.Transform(originContactPos);
                }

                var currentContactPos = arg.PinchManipulation.Current.PrimaryContact;
                {
                    currentContactPos.X -= outputSize.Width / 2.0;
                    currentContactPos.Y -= outputSize.Height / 2.0;
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


            requestProcessing();
        }
        public virtual void ManipulationCompleted(object sender, ManipulationCompletedEventArgs arg)
        {
            saveLastPossaveLastPositionData();
            outputResolution = RESOLUTION.HIGH;
            requestProcessing();
        }

        #endregion


        
        
  

        async Task processRenderingHR()
        {
            var time = DateTime.Now;


            var currentSize = new Size(
                   outputSize.Width /  currentScale,
                   outputSize.Height /  currentScale);
            var corner = new Point(currentPos.X - currentSize.Width / 2, currentPos.Y - currentSize.Height / 2);

         
            reframing.ReframingArea = new Rect(corner, currentSize);
            reframing.Angle = -currentAngle;

            await rendererHR.RenderAsync();
            outputBitmapHRTmp.Pixels.CopyTo(outputBitmapHR.Pixels, 0);
            outputBitmapHR.Invalidate();
            if (output.Source != outputBitmapHR)
                output.Source = outputBitmapHR;


            lastDuration = DateTime.Now - time;
            if (nbHRImage == 0)
            {
                minHRtime = lastDuration;
                maxHRtime = lastDuration;
            }
            if (lastDuration < minHRtime) minHRtime = lastDuration;
            if (lastDuration > maxHRtime) maxHRtime = lastDuration;

            sumHRtime += lastDuration;
            nbHRImage++;
        }

        async Task processRenderingLR()
        {
            var time = DateTime.Now;
            


            var currentSize = new Size(
                   outputSize.Width/  currentScale,
                   outputSize.Height/ currentScale);
            var corner = new Point(currentPos.X - currentSize.Width / 2, currentPos.Y - currentSize.Height / 2);

            reframing.ReframingArea = new Rect(corner, currentSize);
            reframing.Angle = -currentAngle;
            await rendererLR.RenderAsync();
            outputBitmapLRTmp.Pixels.CopyTo(outputBitmapLR.Pixels, 0);
            outputBitmapLR.Invalidate();
            if (output.Source != outputBitmapLR)
                output.Source = outputBitmapLR;

            lastDuration = DateTime.Now - time;
            if (nbLRImage == 0)
            {
                minLRtime = lastDuration;
                maxLRtime = lastDuration;
            }
            if (lastDuration < minLRtime) minLRtime = lastDuration;
            if (lastDuration > maxLRtime) maxLRtime = lastDuration;

            sumLRtime += lastDuration;
            nbLRImage++;
        }


       
    }
}
