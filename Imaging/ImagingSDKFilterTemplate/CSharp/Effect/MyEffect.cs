using Nokia.Graphics.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSharp
{
    public class MyEffect : CustomEffectBase
    {

        double factor;
        public MyEffect(IImageProvider source, double f = 2.0):
            base(source, false)
        {
            factor = f;
        }
        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    uint color = sourcePixelRegion.ImagePixels[index];

                    // Extract color channel values 
                    var a = (byte)((color >> 24) & 255);
                    var r = (byte)((color >> 16) & 255);
                    var g = (byte)((color >> 8) & 255);
                    var b = (byte)((color) & 255);

                    r = (byte)Math.Min(255, r * factor);
                    g = (byte)Math.Min(255, g * factor);
                    b = (byte)Math.Min(255, b * factor);

                    // Combine modified color channels 
                    var newColor = (uint)(b | (g << 8) | (r << 16) | (a << 24));

                    targetPixelRegion.ImagePixels[index] = newColor;
                }
            }); 
        }
    }
}
