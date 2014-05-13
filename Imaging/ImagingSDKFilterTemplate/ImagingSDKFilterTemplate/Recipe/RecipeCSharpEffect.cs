using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeCSharpEffect : CSharp.Recipe.RecipeTemplate
    {
        CSharp.MyEffect effect;

        public RecipeCSharpEffect(IImageProvider source, double factor)
            : base(source)
        {
             effect = new CSharp.MyEffect(source, factor);

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
