using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmoothBorder.CustomEffect{

    class SmoothBorder : CustomEffectBase
    {



        public uint Size { get; set; }
        public SmoothBorder(IImageProvider source, uint size = 8) :
            base(source, false)
        {

            Size = size;
        }



        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {

            uint maskSize = Size % 2 == 0 ? Size + 1 : Size;

            uint width = (uint)sourcePixelRegion.Bounds.Width;
            uint height = (uint)sourcePixelRegion.Bounds.Height;


            List<Tuple<int, double, byte>> maskIndex = new List<Tuple<int, double, byte>>();

            uint r2 = maskSize * maskSize;

            //compute circle mask
            for (int i = -(int)maskSize; i <= maskSize; ++i)
                for (int j = -(int)maskSize; j <= maskSize; ++j)
                {
                    int d2 = i * i + j * j;
                    if (d2 == 0 || d2 > r2)
                        continue;

                    maskIndex.Add(new Tuple<int, double, byte>(
                        i * (int)width + j,
                        d2 / (double)r2,
                        (byte)(255 * Math.Pow(d2 / (double)r2 / 0.5, 0.5) + 0.5)));
                }



            for (uint i = maskSize; i < height - maskSize; ++i)
                for (uint j = maskSize; j < width - maskSize; ++j)
                {
                    uint index = i * width + j;

                    if (sourcePixelRegion.ImagePixels[index] >> 24 > 128)
                    {

                        Tuple<int, double, byte> p = null;
                        foreach (var id in maskIndex)
                        {

                            if (sourcePixelRegion.ImagePixels[index + id.Item1] >> 24 < 128)
                            {
                                if (p == null || id.Item2 < p.Item2)
                                    p = id;
                            }
                        }
                        if (p != null && p.Item2 <= 0.5)
                        {

                            var alpha = p.Item3;
                            uint color = sourcePixelRegion.ImagePixels[index];
                            uint rgb = (color & 0x00FFFFFF);
                            targetPixelRegion.ImagePixels[index] = rgb | (uint)(alpha << 24);
                            continue;
                        }
                        targetPixelRegion.ImagePixels[index] = sourcePixelRegion.ImagePixels[index];
                    }
                }
        }
    }

}
