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
