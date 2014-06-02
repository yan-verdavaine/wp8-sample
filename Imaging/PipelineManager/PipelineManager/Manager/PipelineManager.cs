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
            SetPipelineBeginEnd();
        }



        protected void SetPipelineBeginEnd()
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

        public void Add(IImageConsumer effect)
        {
            if (effect is IImageProvider == false)
                throw new Exception("Add(IImageConsumer) : element should implement IImageProvider interface");


            if (lPipeline.Count > 0)
                effect.Source = lPipeline.Last() as IImageProvider;

            lElement.Add(effect);
            lPipeline.Add(effect);

            SetPipelineBeginEnd();

        }

        public void Add(IFilter filter)
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
                SetPipelineBeginEnd();
            }


        }

        public void Add(CustomFilterBase filter)
        {

            Add((IFilter)filter);

        }

        public void Add(CustomEffectBase filter)
        {

            Add((IImageConsumer)filter);

        }

        public void Add(ICustomFilter filter)
        {


            Add(new DelegatingFilter(filter));
            lElement.Add(filter);


        }

        public void Add(ICustomEffect effect)
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


            if (lElement.Last() is ICustomEffect && !(lElement.Last() is CustomEffectBase))
            {
                //remove ICustomEffect
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                //remove DelegatingEffect
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                lPipeline.Remove(lPipeline.Last());

            }
            else if (lElement.Last() is ICustomFilter && !(lElement.Last() is CustomFilterBase))
            {
                //remove ICustomFilter
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                //remove DelegatingFilter
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                //remove filter or FilterEffect
                var effect = lPipeline.Last() as FilterEffect;
                if (effect.Filters.Count() > 1)
                {
                    var l = effect.Filters.ToList();
                    l.Remove(l.Last());
                    effect.Filters = l;
                }
            }
            else if (lElement.Last() is IFilter)
            {
                //Remove filter
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());

                //remove filter or FilterEffect
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
                //remove effect
                if (lElement.Last() is IDisposable)
                {
                    (lElement.Last() as IDisposable).Dispose();
                }
                lElement.Remove(lElement.Last());
                lPipeline.Remove(lPipeline.Last());

            }
            SetPipelineBeginEnd();


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
                SetPipelineBeginEnd();

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
