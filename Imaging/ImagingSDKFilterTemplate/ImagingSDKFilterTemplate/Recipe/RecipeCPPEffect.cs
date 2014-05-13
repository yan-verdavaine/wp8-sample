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

        override public void Dispose()
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



    }
}
