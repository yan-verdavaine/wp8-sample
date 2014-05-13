using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneAppTest.Resources;
using Microsoft.Phone.Tasks;
using System.IO;
using Nokia.Graphics.Imaging;
using Windows.Storage.Streams;
using Microsoft.Xna.Framework.Media;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Windows.Media;
namespace PhoneAppTest
{
    public partial class MainPage : PhoneApplicationPage
    {
        PictureGesture gestureBackground = new PictureGesture();
        PictureGesture gestureFace = new PictureGesture();
        Stream sBackground;
        Stream sFace;
        public MainPage()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                gestureBackground.Output = displayImage;
                gestureFace.Output = displayImageBlend;
            };
        }

        private void Button_background(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        sBackground = res.ChosenPhoto;
                        gestureBackground.Input = sBackground;
                    }

                };
                task.Show();
            }
            catch (Exception)
            {

                throw;
            }

        }
        private void Button_foreground(object sender, RoutedEventArgs e)
        {
            try
            {
                PhotoChooserTask task = new PhotoChooserTask();
                task.Completed += (s, res) =>
                {
                    if (res.TaskResult == TaskResult.OK)
                    {
                        sFace = res.ChosenPhoto;
                        gestureFace.Input = sFace;
                    }

                };
                task.Show();
            }
            catch (Exception)
            {

                throw;
            }

        }

        private async void ApplicationBarIconButton_reframing(object sender, EventArgs e)
        {
            sFace.Position = 0;
            sBackground.Position = 0;

            IBuffer result;
            using (var faceSource = new StreamImageSource(sFace))
            using (var faceReframing = new FilterEffect(faceSource))
            using (var source = new StreamImageSource(sBackground))
            using (var effect = new FilterEffect(source) )
            using (var renderer = new JpegRenderer(effect))
            {

                //face reframing => blend input
                faceReframing.Filters = new IFilter[] { gestureFace.CreateReframingFilter() } ;

                //face
                effect.Filters = new IFilter[] { 
                    gestureBackground.CreateReframingFilter() ,//background reframing
                    new BlendFilter(faceReframing) //blending


                };


                result = await renderer.RenderAsync();
            }

            using (var media = new MediaLibrary())
                media.SavePictureToCameraRoll("test", result.ToArray());

        }

        private async void ApplicationBarIconButton_All(object sender, EventArgs e)
        {
            sFace.Position = 0;
            sBackground.Position = 0;

            IBuffer result;
            using (var faceSource = new StreamImageSource(sFace))
            using (var faceReframing = new FilterEffect(faceSource))
            using (var source = new StreamImageSource(sBackground))
            using (var effect = new FilterEffect(source))
            using (var renderer = new JpegRenderer(effect))
            {

                

                var size = gestureBackground.ImageSize;
                var Facesize = gestureFace.ImageSize;

                //target scale
                var scale = gestureFace.Scale / gestureBackground.Scale;
                //target angle
                var angle = gestureFace.Angle - gestureBackground.Angle;


                //translation between image center and background position
                var backgroundTranslation = new Point(size.Width / 2 - gestureBackground.Pos.X, size.Height / 2 - gestureBackground.Pos.Y);

                //convert translation to Face referential translation
                CompositeTransform gestureTransform = new CompositeTransform();
                gestureTransform.ScaleX = gestureTransform.ScaleY = scale;
                gestureTransform.Rotation = angle;
                var translation = gestureTransform.Inverse.Transform(backgroundTranslation);

                //target position
                var posX = gestureFace.Pos.X + translation.X;
                var posY = gestureFace.Pos.Y + translation.Y;



                var currentSize = new Windows.Foundation.Size(size.Width / scale, size.Height / scale);
                var corner = new Windows.Foundation.Point(posX - currentSize.Width / 2, posY - currentSize.Height / 2);
                var reframing = new ReframingFilter(new Windows.Foundation.Rect(corner, currentSize), -angle);

                //face reframing => blend input
                faceReframing.Filters = new IFilter[] { reframing };
                effect.Filters = new IFilter[] { new BlendFilter(faceReframing) };//


                result = await renderer.RenderAsync();
            }

            using (var media = new MediaLibrary())
                media.SavePictureToCameraRoll("test", result.ToArray());
        }



    }
}