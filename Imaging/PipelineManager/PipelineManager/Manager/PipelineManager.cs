using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PipelineManager.Manager
{
    class PipelineManager
    {
        List<Object>             lElement           = new  List<Object>();
        List<Object>             lPipeline          = new  List<Object>();

        WeakReference<IImageConsumer> pipelineBegin = null;
        WeakReference<IImageProvider> pipelineEnd = null;
        WeakReference<IImageProvider> pipelineSource = null;

       

        public PipelineManager(IImageProvider source)
        {
            Source = source;
        }



        protected void SetPipelineBeginEnd()
        {
             if(lPipeline.Count == 0)
             {
                 pipelineBegin = null;
                 pipelineEnd = null;
             }

            IImageProvider source;

            if (pipelineSource != null && pipelineSource.TryGetTarget(out source))
            {
                (lPipeline[0] as IImageConsumer).Source = source;
            }

            pipelineBegin = new WeakReference<IImageConsumer>(lPipeline.First() as IImageConsumer);
            pipelineEnd = new WeakReference<IImageProvider>(lPipeline.Last() as IImageProvider);
        }
      

        void Add(IFilter filter) 
        {
           if(lPipeline.Count >0 && lPipeline.Last() is FilterEffect)
           {
               var effect = (FilterEffect)lPipeline.Last();
               var l = effect.Filters.ToList();
               l.Add((IFilter)filter);
               effect.Filters = l;

           }
           else
           {

               var effect = new FilterEffect(){Filters = new IFilter[]{filter}};
               lElement.Add(effect);


               if(lPipeline.Count >0)
                   effect.Source = lPipeline.Last() as IImageProvider;
               lPipeline.Add(effect);
               SetPipelineBeginEnd();
           }

        }

        void Add(ICustomFilter filter)
        {
           lElement.Add(filter);
           Add((IFilter)new DelegatingFilter(filter));

        }

         void Add(ICustomEffect effect)
        {

            lElement.Add(effect);
            Add(new DelegatingEffect(effect));

        }

         void Add<T>(T effect) where T : IImageConsumer, IImageProvider
        {
            lElement.Add(effect);
            if (lPipeline.Count > 0)
                effect.Source = lPipeline.Last() as IImageProvider;
            lPipeline.Add(effect);
            SetPipelineBeginEnd();

        }

        void Undo()
        {

            if (lPipeline.Count == 0)
                return;

            if(lElement.Last() is DelegatingEffect)
            {

                if(lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                lPipeline.Remove(lPipeline.Last());

            }
            else if(lElement.Last() is DelegatingFilter )
            {
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                var effect = lPipeline.Last() as FilterEffect;
                if(effect.Filters.Count() >1)
                {
                    var l = effect.Filters.ToList();
                    l.Remove(l.Last());
                    effect.Filters = l;
                }
                else
                {
                    if (lElement.Last() is IDisposable)
                    {
                        (lElement.Last() as IDisposable).Dispose();
                    }
                    lElement.Remove(lElement.Last());
                    lPipeline.Remove(lPipeline.Last());
                }


            }
            else if (lElement.Last() is IFilter)
            {
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                var effect = lPipeline.Last() as FilterEffect;
                if (effect.Filters.Count() > 1)
                {
                    var l = effect.Filters.ToList();
                    l.Remove(l.Last());
                    effect.Filters = l;
                }
                else
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
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                lPipeline.Remove(lPipeline.Last());

            }



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
            
            foreach(var l in lElement)
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
