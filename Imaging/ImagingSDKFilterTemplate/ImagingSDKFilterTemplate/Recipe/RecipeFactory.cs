using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImagingSDKFIlterTemplate.Recipe
{
    public class RecipeFactory
    {
        private static Lazy<RecipeFactory> mSingleton =  new Lazy<RecipeFactory>(true);
        static public RecipeFactory Current { get { return mSingleton.Value; } }


        public double Param { get; set; }


        public IImageProvider CreatePipeline(IImageProvider source)
        {

            var value = 0.5 + 3 * Param;
            return new RecipeCSharpEffect(source, value); //Recipe with C# custom effect
          //  return new RecipeCSharpFilter(source, value);//Recipe with C# custom Filter
           // return new RecipeCPPEffect(source, value);   //Recipe with CPP custom effect   
            //return new RecipeCPPFilter(source, value);     //Recipe with CPP custom effect  
            return new RecipeDaisyChain(source, value);    //Recipe Daisy chain 


        }



        public RecipeFactory(){}



    }
}
