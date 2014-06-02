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


        protected IImageConsumer pipelineBegin;
        protected IImageProvider pipelineEnd;
        protected IImageProvider pipelineSource;



        public RecipeTemplate(IImageProvider source)
        {
            Source = source;
        }


        protected void SetPipelineBeginEnd(IImageConsumer begin, IImageProvider end)
        {
            pipelineBegin = begin;
            if (pipelineSource != null && begin != null)
                pipelineBegin.Source = pipelineSource;


            pipelineEnd = end;
        }


        #region IImageProvider implementation :
        public Windows.Foundation.IAsyncOperation<Bitmap> GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
        {
            return pipelineEnd.GetBitmapAsync(bitmap, outputOption);
        }

        public Windows.Foundation.IAsyncOperation<ImageProviderInfo> GetInfoAsync()
        {
            return pipelineEnd.GetInfoAsync();
        }

        public bool Lock(RenderRequest renderRequest)
        {
            return pipelineEnd.Lock(renderRequest);

        }

        public Windows.Foundation.IAsyncAction PreloadAsync()
        {
            return pipelineEnd.PreloadAsync();
        }
        #endregion



        #region IImageConsumer implementation
        public IImageProvider Source
        {
            get
            {

                return pipelineSource;

            }
            set
            {
                pipelineSource = value;
                if (pipelineBegin != null)
                    pipelineBegin.Source = value;
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
                //
            }
            pipelineBegin = null;
            pipelineEnd = null;
            pipelineSource = null;



            // Free any unmanaged objects here. 
            //
            disposed = true;
        }
        #endregion
    }
}
