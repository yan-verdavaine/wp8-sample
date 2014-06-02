using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeCSharpFilter : CSharp.Recipe.RecipeTemplate
    {
        CSharp.MyFilter filter;
        FilterEffect   effect;
     

        public RecipeCSharpFilter(IImageProvider source, double factor)
            : base(source)
        {

            effect = new FilterEffect(source);
            effect.Filters = new IFilter[]{new CSharp.MyFilter (factor)};

             SetPipelineBeginEnd(effect, effect);
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
            
            }

            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }

            disposed = true;
            // Call base class implementation. 
            base.Dispose(disposing);
        }
        #endregion

      



    }
}
