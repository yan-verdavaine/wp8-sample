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

        override public void Dispose()
        {
            if (effect != null)
            {
                effect.Dispose();
                effect = null;
            }
        }



    }
}
