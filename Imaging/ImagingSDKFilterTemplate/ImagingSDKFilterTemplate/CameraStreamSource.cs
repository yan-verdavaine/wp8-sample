using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Threading;
using Windows.Foundation;
using Windows.Phone.Media.Capture;
using Windows.Storage.Streams;
using System.Runtime.InteropServices.WindowsRuntime;
using ImagingSDKFIlterTemplate.Recipe;
using System.Globalization;
using System.Windows;


namespace ImagingSDKFIlterTemplate
{

    public class CameraStreamSource : MediaStreamSource
    {
        private readonly Dictionary<MediaSampleAttributeKeys, string> _emptyAttributes = new Dictionary<MediaSampleAttributeKeys, string>();
        private MediaStreamDescription _videoStreamDescription = null;
        private DispatcherTimer _frameRateTimer = null;
        private MemoryStream _frameStream = null;
        private PhotoCaptureDevice _camera = null;

        private long _currentTime = 0;
        private int _frameStreamOffset = 0;
        private int _frameTime = 0;
  
        private Windows.Foundation.Size _frameSize = new Windows.Foundation.Size(0, 0);
        private int _frameBufferSize = 0;
        private byte[] _frameBuffer = null;
        private Bitmap _frameBitmap = null;


        private byte[] _cameraFrameBuffer = null;
        private Bitmap _cameraBitmap = null;
        private IImageProvider _effect = null;
        private BitmapImageSource _source = null;
        private BitmapRenderer _renderer = null;
        private bool _updateEffect = true;


        bool m_initFPSComputation = true;
        DateTime m_startTime;
        long m_nbPicture;
        public Double FPS { get; private set; }
        

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="_cameraEffect">Camera effect to use.</param>
        /// <param name="size">Size of the media element where the stream is rendered to.</param>
        public CameraStreamSource(PhotoCaptureDevice camera, Windows.Foundation.Size size)
        {
            _camera = camera;

            _frameSize = size;
        }

        /// <summary>
        /// Initialises the data structures to pass data to the media pipeline via the MediaStreamSource.
        /// </summary>
        protected override void OpenMediaAsync()
        {
            // General properties

            _frameBufferSize = (int)_frameSize.Width * (int)_frameSize.Height * 4; // RGBA
            _frameBuffer = new byte[_frameBufferSize];

            _frameStream = new MemoryStream(_frameBuffer);

            int layersize = (int)(_frameSize.Width * _frameSize.Height);
            int layersizeuv = layersize / 2;
            _cameraFrameBuffer = new byte[layersize + layersizeuv];
            _cameraBitmap = new Bitmap(
                    _frameSize,
                    ColorMode.Yuv420Sp,
                    new uint[] { (uint)_frameSize.Width, (uint)_frameSize.Width },
                    new IBuffer[] { _cameraFrameBuffer.AsBuffer(0, layersize), _cameraFrameBuffer.AsBuffer(layersize, layersizeuv) });
            _source = new BitmapImageSource(_cameraBitmap);
            _frameBitmap = new Bitmap(
                    _frameSize,
                    ColorMode.Bgra8888,
                   4 * (uint)_frameSize.Width,
                     _frameBuffer.AsBuffer());



            _renderer = new BitmapRenderer();
            _renderer.Bitmap = _frameBitmap;


            // Media stream attributes

            var mediaStreamAttributes = new Dictionary<MediaStreamAttributeKeys, string>();

            mediaStreamAttributes[MediaStreamAttributeKeys.VideoFourCC] = "RGBA";
            mediaStreamAttributes[MediaStreamAttributeKeys.Width] = ((int)_frameSize.Width).ToString();
            mediaStreamAttributes[MediaStreamAttributeKeys.Height] = ((int)_frameSize.Height).ToString();

            _videoStreamDescription = new MediaStreamDescription(MediaStreamType.Video, mediaStreamAttributes);

            // Media stream descriptions

            var mediaStreamDescriptions = new List<MediaStreamDescription>();
            mediaStreamDescriptions.Add(_videoStreamDescription);

            // Media source attributes

            var mediaSourceAttributes = new Dictionary<MediaSourceAttributesKeys, string>();
            mediaSourceAttributes[MediaSourceAttributesKeys.Duration] = TimeSpan.FromSeconds(0).Ticks.ToString(CultureInfo.InvariantCulture);
            mediaSourceAttributes[MediaSourceAttributesKeys.CanSeek] = false.ToString();

            _frameTime = (int)TimeSpan.FromSeconds((double)0).Ticks;

            // Start frame rate timer

           

            // Report that we finished initializing its internal state and can now pass in frame samples

            ReportOpenMediaCompleted(mediaSourceAttributes, mediaStreamDescriptions);
        }

        protected override void CloseMedia()
        {
         
            {
                _camera = null;
                if (_frameStream != null)
                {
                    _frameStream.Close();
                    _frameStream = null;
                }

               

               
                if (_renderer != null)
                {
                    _renderer.Bitmap = null; // bug  : crash on bitmap dispose
  
                    _renderer.Dispose();
                    _renderer = null;
                  
                }

                if (_effect != null && _effect is IDisposable)
                {
                  // (_effect as IDisposable).Dispose(); // bug : crash on CustomEffectBase dispose
                    _effect = null;
                }
              
                if (_source != null)
                {
                    _source.Dispose();
                    _source = null;
                }
                if (_frameBitmap != null)
                {
                   _frameBitmap.Dispose();
                    _frameBitmap = null;
                }
                if (_cameraBitmap != null)
                {
                   _cameraBitmap.Dispose();
                    _cameraBitmap = null;
                }


                _frameStreamOffset = 0;
                _frameTime = 0;
           
                _frameBufferSize = 0;
                _frameBuffer = null;
                _cameraFrameBuffer = null;
                _videoStreamDescription = null;
                _currentTime = 0;
            }
        }

        /// <summary>
        /// Processes camera frameBuffer using the set effect and provides media element with a filtered frameBuffer.
        /// </summary>
        protected async override void GetSampleAsync(MediaStreamType mediaStreamType)
        {
            if (m_initFPSComputation)
            {
                FPS = -1;
                m_startTime = DateTime.Now;
                m_nbPicture = 0;
                m_initFPSComputation = false;
            }
            
            {

                if (_camera == null)
                {
                    return;
                    _frameStream.Position = 0;
                    _currentTime += _frameTime;
               

                    var sample = new MediaStreamSample(_videoStreamDescription, _frameStream, _frameStreamOffset, _frameBufferSize, _currentTime, _emptyAttributes);

                    ReportGetSampleCompleted(sample);
                }
                _camera.GetPreviewBufferYCbCr(_cameraFrameBuffer);
            }

            if (_updateEffect)
            {
                if (_effect != null && _effect is IDisposable)
                {
                    (_effect as IDisposable).Dispose();
                    _effect = null;
                }
                _effect = RecipeFactory.Current.CreatePipeline(_source);
                _renderer.Source = _effect;
            }
            _updateEffect = false;




           await _renderer.RenderAsync();

         //   task.ContinueWith((action) =>
          //  {
                if (_frameStream != null)
                {
                    _frameStream.Position = 0;
                    _currentTime += _frameTime;

                    var sample = new MediaStreamSample(_videoStreamDescription, _frameStream, _frameStreamOffset, _frameBufferSize, _currentTime, _emptyAttributes);

                    ReportGetSampleCompleted(sample);
                }

          //  });
                ++m_nbPicture;
                var t = DateTime.Now;
                if (m_nbPicture > 10 && t.Subtract(m_startTime).TotalMilliseconds > 1000)
                {
                    FPS = m_nbPicture / t.Subtract(m_startTime).TotalSeconds;
                    m_startTime = t;
                    m_nbPicture = 0;

                }

        }

        protected override void GetDiagnosticAsync(MediaStreamSourceDiagnosticKind diagnosticKind)
        {
            throw new NotImplementedException();
        }

        protected override void SeekAsync(long seekToTime)
        {
            _currentTime = seekToTime;

            ReportSeekCompleted(_currentTime);
        }

        protected override void SwitchMediaStreamAsync(MediaStreamDescription mediaStreamDescription)
        {
            throw new NotImplementedException();
        }

       

        internal void UpdateEffect()
        {
            _updateEffect = true;
        }
        
    }

}
