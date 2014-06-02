using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeCPPEffect : CSharp.Recipe.RecipeTemplate
    {
        CPP.MyEffect effect;
        DelegatingEffect delegatingEffect;

        public RecipeCPPEffect(IImageProvider source, double factor)
            : base(source)
        {
             effect = new CPP.MyEffect( factor);

             delegatingEffect = new DelegatingEffect(source, effect);
             SetPipelineBeginEnd(delegatingEffect, delegatingEffect);
        }

        #region IDispose
        // Flag: Has Dispose already been called? 
        bool disposed = false;

        // Protected implementation of Dispose pattern. 
        protected override void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (delegatingEffect != null)
                {
                    delegatingEffect.Dispose();
                    delegatingEffect = null;
                }
                if (effect != null)
                {
                    effect.Dispose();
                    effect = null;
                }
            }

            // Free any unmanaged objects here. 
            //
            disposed = true;
            // Call base class implementation. 
            base.Dispose(disposing);
        }
        #endregion




    }
}
