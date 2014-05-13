using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeCPPFilter : CSharp.Recipe.RecipeTemplate
    {
        CPP.MyFilter filter;
        DelegatingFilter delegatingFilter;
        FilterEffect   effect;


        public RecipeCPPFilter(IImageProvider source, double factor)
            : base(source)
        {

            filter = new CPP.MyFilter(factor);
            delegatingFilter = new DelegatingFilter(filter);

            effect = new FilterEffect(source);
            effect.Filters = new IFilter[] { delegatingFilter };

             SetPipelineBeginEnd(effect, effect);
        }

        override public void Dispose()
        {
            if (filter != null)
            {
                filter.Dispose();
                filter = null;
            }
            if (delegatingFilter != null)
            {
                delegatingFilter.Dispose();
                delegatingFilter = null;
            }
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }



    }
}
