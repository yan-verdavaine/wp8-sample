using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineManager.Manager
{
    class PipelineManager : IImageConsumer, IImageProvider, IDisposable
    {
        List<Object> lElement = new List<Object>();
        List<Object> lPipeline = new List<Object>();

        WeakReference<IImageConsumer> pipelineBegin = null;
        WeakReference<IImageProvider> pipelineEnd = null;
        WeakReference<IImageProvider> pipelineSource = null;



        public PipelineManager(IImageProvider source)
        {
            Source = source;
            updatePipelineBeginEnd();
        }



        protected void updatePipelineBeginEnd()
        {
            if (lPipeline.Count == 0)
            {
                pipelineBegin = null;
                pipelineEnd = pipelineSource;
                return;
            }

            IImageProvider source;

            if (pipelineSource != null && pipelineSource.TryGetTarget(out source))
            {
                (lPipeline[0] as IImageConsumer).Source = source;
            }

            pipelineBegin = new WeakReference<IImageConsumer>(lPipeline.First() as IImageConsumer);
            pipelineEnd = new WeakReference<IImageProvider>(lPipeline.Last() as IImageProvider);
        }



        public void Add(Object obj)
        {
            if (obj is IImageConsumer)
            {
                addImageConsumer(obj as IImageConsumer);
            }
            else if (obj is IFilter)
            {
                addIFilter(obj as IFilter);
            }
            else if (obj is ICustomFilter)
            {
                addICustomFilter(obj as ICustomFilter);
            }
            else if (obj is ICustomEffect)
            {
                addICustomEffect(obj as ICustomEffect);
            }
            else
            {
                throw new ArgumentException("Element not supported");
            }
        }
        private void addImageConsumer(IImageConsumer effect)
        {
            if (effect is IImageProvider == false)
                throw new Exception("Add(IImageConsumer) : element should implement IImageProvider interface");


            if (lPipeline.Count > 0)
                effect.Source = lPipeline.Last() as IImageProvider;

            lElement.Add(effect);
            lPipeline.Add(effect);

            updatePipelineBeginEnd();

        }

        private void addIFilter(IFilter filter)
        {

            if (lPipeline.Count > 0 && lPipeline.Last() is FilterEffect)
            {
                var effect = (FilterEffect)lPipeline.Last();
                var l = effect.Filters.ToList();
                l.Add(filter);
                effect.Filters = l;
                lElement.Add(filter);

            }
            else
            {

                var effect = new FilterEffect() { Filters = new IFilter[] { filter } };


                if (lPipeline.Count > 0)
                    effect.Source = lPipeline.Last() as IImageProvider;
                lPipeline.Add(effect);
                lElement.Add(effect);
                lElement.Add(filter);
                updatePipelineBeginEnd();
            }


        }

   
        private void addICustomFilter(ICustomFilter filter)
        {

            Add(new DelegatingFilter(filter));
            lElement.Add(filter);


        }

        public void addICustomEffect(ICustomEffect effect)
        {
            Add(new DelegatingEffect(effect));
            lElement.Add(effect);

        }

        public IImageProvider End()
        {
            if (lPipeline.Count > 0)
                return lPipeline.Last() as IImageProvider;



            return Source;
        }

        public void Undo()
        {

            if (lPipeline.Count == 0)
                return;

            //IFilter and ICustomFilter
            if (lElement.Last() is IFilter || lElement.Last() is ICustomFilter)
            {

                //remove Filter
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                if(lElement.Last() is DelegatingFilter)
                {

                    //remove DelegatingFilter
                    if (lElement.Last() is IDisposable)
                    {
                        (lElement.Last() as IDisposable).Dispose();
                    }
                    lElement.Remove(lElement.Last());
                }
               

                //remove filter from FilterEffect
                var effect = lPipeline.Last() as FilterEffect;
                if (effect.Filters.Count() > 1)
                {
                    var l = effect.Filters.ToList();
                    l.Remove(l.Last());
                    effect.Filters = l;
                }
                else//remove FilterEffect
                {
                    if (lElement.Last() is IDisposable)
                    {
                        (lElement.Last() as IDisposable).Dispose();
                    }
                    lElement.Remove(lElement.Last());
                    lPipeline.Remove(lPipeline.Last());
                }

            }
            else
            {
                //remove Effect
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                if (lElement.Count>0 && lElement.Last() is DelegatingEffect)
                {
                    //remove DelegatingEffect
                    if (lElement.Last() is IDisposable)
                    {
                        (lElement.Last() as IDisposable).Dispose();
                    }
                    lElement.Remove(lElement.Last());
                }
                lPipeline.Remove(lPipeline.Last());

            }
        
            updatePipelineBeginEnd();


        }



        #region IImageProvider implementation :
        public Windows.Foundation.IAsyncOperation<Bitmap> GetBitmapAsync(Bitmap bitmap, OutputOption outputOption)
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                throw new InvalidOperationException("No image provider in the pipeline.");
            else
                return provider.GetBitmapAsync(bitmap, outputOption);
        }

        public Windows.Foundation.IAsyncOperation<ImageProviderInfo> GetInfoAsync()
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                throw new InvalidOperationException("No image provider in the pipeline.");
            else
                return provider.GetInfoAsync();
        }

        public bool Lock(RenderRequest renderRequest)
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                throw new InvalidOperationException("No image provider in the pipeline.");
            else
                return provider.Lock(renderRequest);

        }

        public Windows.Foundation.IAsyncAction PreloadAsync()
        {
            IImageProvider provider = null;
            if (pipelineEnd == null || !pipelineEnd.TryGetTarget(out provider))
                throw new InvalidOperationException("No image provider in the pipeline.");
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
                updatePipelineBeginEnd();

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

            foreach (var l in lElement)
            {
                if (l is IDisposable)
                {
                    (l as IDisposable).Dispose();
                }
            }
            lElement.Clear();
            lPipeline.Clear();

            disposed = true;
        }
        #endregion
    }

}
