using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeDaisyChain : CSharp.Recipe.RecipeTemplate
    {
        HdrEffect effect_1;
        FilterEffect    effect_2;
        public RecipeDaisyChain(IImageProvider source, double factor)
            :base(source)
        {
            effect_1 = new HdrEffect(source);
            effect_2 = new FilterEffect(effect_1);

            
            if(factor>2) factor = 2;
            effect_2.Filters = new IFilter[] { new HueSaturationFilter(-1 + factor, 0), new LomoFilter() };

            SetPipelineBeginEnd(effect_1, effect_2);


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

            if (effect_1 != null)
            {
                effect_1.Dispose();
                effect_1 = null;
            }
            if (effect_2 != null)
            {
                effect_2.Dispose();
                effect_2 = null;
            }
            disposed = true;
            // Call base class implementation. 
            base.Dispose(disposing);
        }
        #endregion


       
    }
}
