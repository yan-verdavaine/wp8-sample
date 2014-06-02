using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp.Recipe
{
    public class RecipeTemplate : IImageProvider, IImageConsumer, IDisposable
    {


        WeakReference<IImageConsumer> pipelineBegin = null;
        WeakReference<IImageProvider> pipelineEnd = null;
        WeakReference<IImageProvider> pipelineSource = null;



        public RecipeTemplate(IImageProvider source)
        {
            Source = source;
        }


        protected void SetPipelineBeginEnd(IImageConsumer begin, IImageProvider end)
        {

            IImageProvider source;

            if (begin != null && pipelineSource != null && pipelineSource.TryGetTarget(out source))
            {
                begin.Source = source;
            }

            pipelineBegin = new WeakReference<IImageConsumer>(begin);
            pipelineEnd = new WeakReference<IImageProvider>(end);
        }


        #region IImageProvider implementation :
        public Windows.Foundation.IAsyncOperation<Bitmap> GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                return null;
            else
                return provider.GetBitmapAsync(bitmap, outputOption);
        }

        public Windows.Foundation.IAsyncOperation<ImageProviderInfo> GetInfoAsync()
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                return null;
            else
                return provider.GetInfoAsync();
        }

        public bool Lock(RenderRequest renderRequest)
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                return false;
            else
                return provider.Lock(renderRequest);

        }

        public Windows.Foundation.IAsyncAction PreloadAsync()
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                return null;
            else
                return provider.PreloadAsync();
        }
        #endregion



        #region IImageConsumer implementation
        public IImageProvider Source
        {
            get
            {
                IImageProvider source;

                if (pipelineSource != null && pipelineSource.TryGetTarget(out source))
                {
                    return source;
                }

                return null;

            }
            set
            {
                pipelineSource = new WeakReference<IImageProvider>(value);
                IImageConsumer begin;
                if (pipelineBegin != null && pipelineBegin.TryGetTarget(out begin))
                    begin.Source = value;
            }
        }
        #endregion

        #region IDipose implementation
        //http://msdn.microsoft.com/library/system.idisposable.aspx

        // Flag: Has Dispose already been called? 
        bool disposed = false;

        // Public implementation of Dispose pattern callable by consumers. 
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        // Protected implementation of Dispose pattern. 
        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                // Free any other managed objects here. 
                pipelineBegin = null;
                pipelineEnd = null;
                pipelineSource = null;
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
        }
        #endregion
    }
}
