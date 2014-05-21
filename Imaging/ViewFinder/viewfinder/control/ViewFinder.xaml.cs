using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using Microsoft.Xna.Framework.Media; // For the media library
using System.IO;                     // For the memory stream
using Microsoft.Devices;
using System.Windows.Media;

using System.Windows.Media.Imaging;
using Windows.Phone.Media.Capture;
using System.Threading.Tasks;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using Nokia.Graphics.Imaging;

namespace viewfinder.control
{
    public partial class ViewFinder : UserControl
    {
        PhotoCaptureDevice m_captureDevice;
        double m_orientationAngle = 0.0;
        CameraSensorLocation m_sensorLocation;
        object m_parentPage;
        bool m_previewRunning = false;

        bool commandeRunning = false;
        public enum StrechMode
        {
            Uniform,
            UniformFill
        };
        StrechMode m_Mode;
        public StrechMode Mode
        {
            get { return m_Mode; }
            set
            {
                if (!commandeRunning)
                {
                    m_Mode = value;
                    computeVideoBruchTransform();
                }
            }
        }
        public CameraSensorLocation SensorLocation
        {
            get { return m_sensorLocation; }
            set
            {
                if (!commandeRunning)
                {
                    m_sensorLocation = value;
                    if (m_previewRunning)
                        initCamera();
                }

            }
        }

        public void start()
        {
            m_previewRunning = true;
            initCamera();
        }

        public void strop()
        {
            m_previewRunning = false;
            if (m_captureDevice != null)
            {
                m_captureDevice.Dispose();
                m_captureDevice = null;

            }
        }

        public ViewFinder()
        {
            InitializeComponent();
            Mode = StrechMode.UniformFill;
            this.LayoutUpdated += ViewFinder_LayoutUpdated;
            if (App.RootFrame != null)
            {
                App.RootFrame.OrientationChanged += RootFrame_OrientationChanged;
                App.RootFrame.Navigating += RootFrame_Navigating;
                App.RootFrame.Navigated += RootFrame_Navigated;
                Tap += ViewFinder_Tap;
                Loaded += (s, e) =>
                {
                    m_parentPage = App.RootFrame.Content;
                    setPageOrientation(App.RootFrame.Orientation);

                };
            };
        }

        async void ViewFinder_Tap(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (m_previewRunning && !commandeRunning)
            {
                try
                {
                    commandeRunning = true;

                    //compute vector between preview picture center and Inverted transformation center
                    var tmp = viewfinderBrush.Transform.Inverse.TransformBounds(new Rect(new Point(), viewfinderCanvas.RenderSize));
                    var dx = m_captureDevice.PreviewResolution.Width / 2 - (tmp.X + tmp.Width / 2);
                    var dy = m_captureDevice.PreviewResolution.Height / 2 - (tmp.Y + tmp.Height / 2);
        
                    //invert tap position
                    var p =  e.GetPosition(this);
                    var pInPreview = viewfinderBrush.Transform.Inverse.Transform(p);

                    //transform inverted position to picture reference
                    double X = pInPreview.X + dx;
                    double Y = pInPreview.Y + dy;

                    if (X < 0) X = 0;
                    if (X >= m_captureDevice.PreviewResolution.Width) X = m_captureDevice.PreviewResolution.Width - 1;

                    if (Y >= m_captureDevice.PreviewResolution.Height) Y = m_captureDevice.PreviewResolution.Height - 1;
                    if (Y < 0) Y = 0;

                    m_captureDevice.FocusRegion = new Windows.Foundation.Rect(
                        new Windows.Foundation.Point(X, Y),
                        new Windows.Foundation.Size());
                    await m_captureDevice.FocusAsync();

                }
                catch (Exception ee)
                {
                }
                finally
                {
                    commandeRunning = false;
                }



            }
        }

        void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {
            if (m_captureDevice != null)
            {
                m_captureDevice.Dispose();
                m_captureDevice = null;

            }
        }

        void RootFrame_Navigated(object sender, NavigationEventArgs e)
        {
            if (m_parentPage != null && m_parentPage == e.Content)
            {
                if (m_previewRunning)
                    initCamera();
            }
        }

        void setPageOrientation(PageOrientation orientation)
        {
            if ((orientation & PageOrientation.Portrait) == PageOrientation.Portrait)
            {
                m_orientationAngle = 0;
            }
            else if ((orientation & PageOrientation.LandscapeLeft) == PageOrientation.LandscapeLeft)
            {
                m_orientationAngle = -90;
            }
            else
            {
                m_orientationAngle = +90;
            }
            computeVideoBruchTransform();
        }


        void RootFrame_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            setPageOrientation(e.Orientation);
        }



        void ViewFinder_LayoutUpdated(object sender, EventArgs e)
        {
            if (m_previewRunning)
                computeVideoBruchTransform();
        }


        async void initCamera()
        {
            if (commandeRunning)
                return;
            try
            {
                commandeRunning = true;
                if (m_captureDevice != null)
                {
                    m_captureDevice.Dispose();
                    m_captureDevice = null;

                }

                // Use the back camera.
                /*  var deviceName = Microsoft.Phone.Info.DeviceStatus.DeviceName;
                  if (m_sensorLocation == CameraSensorLocation.Back && (deviceName.Contains("RM-875") || deviceName.Contains("RM-876") || deviceName.Contains("RM-877")))
                  {
                      m_captureDevice = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, new Windows.Foundation.Size(7712, 4352));
                      //m_captureDevice = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back,new Windows.Foundation.Size(7136,5360));

                  }
                  else if (m_sensorLocation == CameraSensorLocation.Back && (deviceName.Contains("RM-937") || deviceName.Contains("RM-938") || deviceName.Contains("RM-939")))
                  {
                      m_captureDevice = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back, new Windows.Foundation.Size(5376, 3024)); // 16:9 ratio
                      //m_captureDevice = await PhotoCaptureDevice.OpenAsync(CameraSensorLocation.Back,new Windows.Foundation.Size(4992, 3744)); // 4:3 ratio
                  }
                  else*/
                {
                    var SupportedResolutions = PhotoCaptureDevice.GetAvailableCaptureResolutions(m_sensorLocation).ToArray();
                    m_captureDevice = await PhotoCaptureDevice.OpenAsync(m_sensorLocation, SupportedResolutions[0]);
                }

                viewfinderBrush.SetSource(m_captureDevice);

                computeVideoBruchTransform();
            }

            finally
            {
                commandeRunning = false;

            }

        }

        void computeVideoBruchTransform()
        {

            if (m_captureDevice == null)
                return;

            var tmptransform = new RotateTransform() { Angle = m_orientationAngle + m_captureDevice.SensorRotationInDegrees };
            var previewSize = tmptransform.TransformBounds(new Rect(new Point(), new Size(m_captureDevice.PreviewResolution.Width, m_captureDevice.PreviewResolution.Height)));



            double s1 = viewfinderCanvas.ActualWidth / previewSize.Width;
            double s2 = viewfinderCanvas.ActualHeight / previewSize.Height;

            //fit out
            double scale = m_Mode == StrechMode.UniformFill ? Math.Max(s1, s2) : Math.Min(s1, s2);

            var t = new TransformGroup();

            if (m_sensorLocation == CameraSensorLocation.Front)
            {
                t.Children.Add(new ScaleTransform() { ScaleX = -1, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2 });
            }
            t.Children.Add(new CompositeTransform() { Rotation = m_orientationAngle + m_captureDevice.SensorRotationInDegrees, CenterX = viewfinderCanvas.ActualWidth / 2, CenterY = viewfinderCanvas.ActualHeight / 2, ScaleX = scale, ScaleY = scale });
                
            viewfinderBrush.Transform = t;
            


        }
        public async Task<CameraFocusStatus> FocusAsync()
        {
            if (commandeRunning)
                return CameraFocusStatus.NotLocked;
            try
            {
                commandeRunning = true;
                if (m_captureDevice != null && m_sensorLocation == CameraSensorLocation.Back)
                    return await m_captureDevice.FocusAsync();
            }
            finally
            {
                commandeRunning = false;
            }
            return CameraFocusStatus.NotLocked;
        }
        public async Task<IBuffer> TakePicture()
        {
            if (m_captureDevice == null || commandeRunning)
                return null;
            try
            {
                commandeRunning = true;
                int angle = (int)(m_orientationAngle + m_captureDevice.SensorRotationInDegrees);
                if (angle < 0) angle += 360;
                if (m_sensorLocation == CameraSensorLocation.Back)
                {
                    m_captureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, angle);
                }
                else
                {
                    m_captureDevice.SetProperty(KnownCameraGeneralProperties.EncodeWithOrientation, -angle);
                }
                m_captureDevice.SetProperty(KnownCameraGeneralProperties.SpecifiedCaptureOrientation, 0);


                var cameraCaptureSequence = m_captureDevice.CreateCaptureSequence(1);
                var stream = new MemoryStream();
                cameraCaptureSequence.Frames[0].CaptureStream = stream.AsOutputStream();
                await m_captureDevice.PrepareCaptureSequenceAsync(cameraCaptureSequence);
                await cameraCaptureSequence.StartCaptureAsync();
                if (m_sensorLocation == CameraSensorLocation.Back)
                {
                    return stream.GetWindowsRuntimeBuffer();
                }
                else
                {
                    return await JpegTools.FlipAndRotateAsync(stream.GetWindowsRuntimeBuffer(), FlipMode.Horizontal, Rotation.Rotate0, JpegOperation.AllowLossy);
                }

            }
            finally
            {
                commandeRunning = false;
            }
            return null;
        }
    }
}
