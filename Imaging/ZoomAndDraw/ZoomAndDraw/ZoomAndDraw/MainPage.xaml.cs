using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using ZoomAndDraw.Resources;
using System.Windows.Media;
using System.Windows.Shapes;
using Microsoft.Phone.Tasks;
using System.Windows.Media.Imaging;

namespace ZoomAndDraw
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PhotoChooserTask _phototask = new PhotoChooserTask();
        private PhotoResult _photoResult;
        private SolidColorBrush _brush;
        private Polyline _polyline;
        private bool _manipulating;
        bool isPinching;
        Point ptPinchPositionStart;

        public MainPage()
        {
            InitializeComponent();
            _phototask.ShowCamera = true;
            _phototask.Completed += PhotoChooserTask_Completed;
        }
        //yve : remove event
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            ManipulationArea.ManipulationStarted -= AnnotationsCanvas_ManipulationStarted;
            ManipulationArea.ManipulationDelta -= AnnotationsCanvas_ManipulationDelta;
            ManipulationArea.ManipulationCompleted -= AnnotationsCanvas_ManipulationCompleted;
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_photoResult != null)
            {
                if (_brush == null)
                {
                    _brush = new SolidColorBrush(Colors.Red);
                }
            }
            else
            {
                _brush = null;
            }

            if (_photoResult != null)
            {
                var originalBitmap = new BitmapImage
                {
                    DecodePixelWidth = (int)(480.0 * Application.Current.Host.Content.ScaleFactor / 100.0)
                };

                originalBitmap.SetSource(_photoResult.ChosenPhoto);

                OriginalImage.Source = originalBitmap;

                originalBitmap = null;
                _photoResult = null;
            }

            ManipulationArea.ManipulationStarted += AnnotationsCanvas_ManipulationStarted;
            ManipulationArea.ManipulationDelta += AnnotationsCanvas_ManipulationDelta;
            ManipulationArea.ManipulationCompleted += AnnotationsCanvas_ManipulationCompleted;
        }

        private Point NearestPointInElement(double x, double y, FrameworkElement element)
        {
            var clampedX = Math.Min(Math.Max(0, x), element.ActualWidth);
            var clampedY = Math.Min(Math.Max(0, y), element.ActualHeight);

            return new Point(clampedX, clampedY);
        }

        private void AnnotationsCanvas_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            if (!isPinching)
            {
                _manipulating = true;

                _polyline = new Polyline
                {
                    Stroke = _brush,
                    StrokeThickness = 6
                };

                var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
                var manipulationAreaDeltaY = ManipulationArea.Margin.Top;

                var point = NearestPointInElement(e.ManipulationOrigin.X + manipulationAreaDeltaX, e.ManipulationOrigin.Y + manipulationAreaDeltaY, AnnotationsCanvas);

                _polyline.Points.Add(point);

                CurrentAnnotationCanvas.Children.Add(_polyline);
            }
        }

        private void AnnotationsCanvas_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            if (!isPinching)
            {
                var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
                var manipulationAreaDeltaY = ManipulationArea.Margin.Top;
                //yve : if you remove translation you use old position
                var x = e.ManipulationOrigin.X + manipulationAreaDeltaX;
                var y = e.ManipulationOrigin.Y  + manipulationAreaDeltaY;

                var point = NearestPointInElement(x, y, AnnotationsCanvas);

                _polyline.Points.Add(point);
            }
        }

        private void AnnotationsCanvas_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (!isPinching)
            {
                if (_polyline == null || _polyline.Points.Count < 2)
                {
                    CurrentAnnotationCanvas.Children.Clear();

                    _manipulating = false;
                }
                else
                {
                    CurrentAnnotationCanvas.Children.RemoveAt(CurrentAnnotationCanvas.Children.Count - 1);

                    AnnotationsCanvas.Children.Add(_polyline);

                    _manipulating = false;
                }

                _polyline = null;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            _phototask.Show();
        }

        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _photoResult = e;
            }
        }

        void OnGestureListenerPinchStarted(object sender, PinchStartedGestureEventArgs args)
        {
            isPinching = true;


            //yve : remove current annotation
            CurrentAnnotationCanvas.Children.Clear();
            _manipulating = false;
            _polyline = null;




            if (isPinching)
            {
                // Set transform centers
                Point ptPinchCenter = args.GetPosition(OriginalImage);
                ptPinchCenter = previousTransform.Transform(ptPinchCenter);

                scaleTransform.CenterX = ptPinchCenter.X;
                scaleTransform.CenterY = ptPinchCenter.Y;

                ptPinchPositionStart = args.GetPosition(this);
            }
        }

        void OnGestureListenerPinchDelta(object sender, PinchGestureEventArgs args)
        {
            if (isPinching)
            {
                // Set scaling
                scaleTransform.ScaleX = args.DistanceRatio;
                scaleTransform.ScaleY = args.DistanceRatio;

                // Set translation
                Point ptPinchPosition = args.GetPosition(this);
                translateTransform.X = ptPinchPosition.X - ptPinchPositionStart.X;
                translateTransform.Y = ptPinchPosition.Y - ptPinchPositionStart.Y;
            }
        }

        void OnGestureListenerPinchCompleted(object sender, PinchGestureEventArgs args)
        {
            if (isPinching)
            {
                TransferTransforms();
                isPinching = false;
            }
        }

        void TransferTransforms()
        {
            previousTransform.Matrix = Multiply(previousTransform.Matrix, currentTransform.Value);

            // Set current transforms to default values
            scaleTransform.ScaleX = scaleTransform.ScaleY = 1;
            scaleTransform.CenterX = scaleTransform.CenterY = 0;

            translateTransform.X = translateTransform.Y = 0;
        }

        Matrix Multiply(Matrix A, Matrix B)
        {
            return new Matrix(A.M11 * B.M11 + A.M12 * B.M21,
                              A.M11 * B.M12 + A.M12 * B.M22,
                              A.M21 * B.M11 + A.M22 * B.M21,
                              A.M21 * B.M12 + A.M22 * B.M22,
                              A.OffsetX * B.M11 + A.OffsetY * B.M21 + B.OffsetX,
                              A.OffsetX * B.M12 + A.OffsetY * B.M22 + B.OffsetY);
        }
    }
}