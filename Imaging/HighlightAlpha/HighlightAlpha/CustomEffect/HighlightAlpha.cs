using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;



namespace HighlightAlpha.CustomEffect
{
    class HighlightAlpha : CustomEffectBase      
    {
        
        
        public Windows.UI.Color Color {get; set;}
         public uint Size {get; set;}
         public HighlightAlpha(IImageProvider source,  uint size = 8) :
            base(source, false)
        {
            Color = Windows.UI.Color.FromArgb(255,255,255,255);
            Size = size;
        }

         public HighlightAlpha(IImageProvider source, Windows.UI.Color color, uint size = 9) :
             base(source, false)
         {
             Color = color;
             Size = size;
         }
  
        

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {

            uint color = FromColor(Color);
            uint  maskSize = Size%2 == 0 ? Size + 1 : Size;

            uint width = (uint)sourcePixelRegion.Bounds.Width;
            uint height = (uint)sourcePixelRegion.Bounds.Height;


            List<int> maskIndex = new List<int>();
            
            uint maskSize_2 = maskSize/2;

            uint r2 = maskSize_2 * maskSize_2;

            for(int i = -(int)maskSize_2; i <= maskSize_2;++i)
                for (int j = -(int)maskSize_2; j <= maskSize_2; ++j)
                {

                    int d2 = i*i +j*j;
                    if(d2 == 0 || d2>r2)
                        continue;

                    maskIndex.Add(i * (int)width + j);

                }



           for(uint i = maskSize_2; i <height - maskSize_2;++i)
                for(uint j = maskSize_2; j <width - maskSize_2;++j)
                {
                    uint index = i*width+j;

                    if(sourcePixelRegion.ImagePixels[index] >>24 < 128)
                    {

                        bool NoALphaPixelFound = false;
                        foreach(int id in maskIndex)
                        {

                           if(sourcePixelRegion.ImagePixels[index + id] >>24 >128)
                           {
                               NoALphaPixelFound = true;
                               break;
                           }
                        }
                        if (NoALphaPixelFound)
                            targetPixelRegion.ImagePixels[index] = color;


                    }
                    else
                    {
                        targetPixelRegion.ImagePixels[index] = sourcePixelRegion.ImagePixels[index];
                    }







                }
           /* sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    uint color = sourcePixelRegion.ImagePixels[index];

                    // copy grayscale buffer to alpha channel 
                    var a = buffer[index];
                    uint rgb = (color & 0x00FFFFFF); 
                    targetPixelRegion.ImagePixels[index] = rgb | (uint)(a << 24);
                }
            }); */
        }
    
    }
}
