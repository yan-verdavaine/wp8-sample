using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HighlightAlpha.Filter
{
    class HighlightAlphaFilter: CustomFilterBase
    {
       
         public Windows.UI.Color Color {get; set;}
         public uint Size {get; private set;}
         public HighlightAlphaFilter( uint size = 8) :
             base(new Margins() { Bottom = size, Left = size, Right = size, Top = size }, false, new ColorMode[] { ColorMode.Bgra8888 })
        {
            Color = Windows.UI.Color.FromArgb(255,255,255,255);
            Size = size;
        }

         public HighlightAlphaFilter( Windows.UI.Color color, uint size = 9) :
             base(new Margins() { Bottom = size / 2 + 1, Left = size / 2 + 1, Right = size / 2 + 1, Top = size / 2 + 1 }, false, new ColorMode[] { ColorMode.Bgra8888 })
         {
             Color = color;
             Size = size;
         }


     

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            uint color = ((uint)Color.A << 24) | ((uint)Color.R << 16) | ((uint)Color.G << 8) | ((uint)Color.B);
            uint maskSize = Size % 2 == 0 ? Size + 1 : Size;

            uint width = (uint)sourcePixelRegion.Bounds.Width;
            uint height = (uint)sourcePixelRegion.Bounds.Height;


            List<int> maskIndex = new List<int>();

            uint maskSize_2 = maskSize / 2;

            uint r2 = maskSize_2 * maskSize_2;

            //compute circle mask
            for (int i = -(int)maskSize_2; i <= maskSize_2; ++i)
                for (int j = -(int)maskSize_2; j <= maskSize_2; ++j)
                {
                    int d2 = i * i + j * j;
                    if (d2 == 0 || d2 > r2)
                        continue;

                    maskIndex.Add(i * (int)sourcePixelRegion.Pitch + j);
                }


            for (uint i = 0; i < height ; ++i)
                for (uint j = 0; j < width ; ++j)
                {
                    uint index_source = (uint) (sourcePixelRegion.StartIndex + i * sourcePixelRegion.Pitch + j);
                    uint index_target = (uint) (targetPixelRegion.StartIndex + i * targetPixelRegion.Pitch + j);

                    if (sourcePixelRegion.ImagePixels[index_source] >> 24 < 128)
                    {

                        bool NoALphaPixelFound = false;
                        foreach (int id in maskIndex)
                        {

                            if (sourcePixelRegion.ImagePixels[index_source + id] >> 24 > 128)
                            {
                                NoALphaPixelFound = true;
                                break;
                            }
                        }
                        if (NoALphaPixelFound)
                        {
                            targetPixelRegion.ImagePixels[index_target] = color;
                            continue;
                        }
                    }
                        targetPixelRegion.ImagePixels[index_target] = sourcePixelRegion.ImagePixels[index_source];

                }




        }
    }
}
