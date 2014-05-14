using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System.Runtime.InteropServices.WindowsRuntime;

namespace CustomEffect
{
    public class GrayscaleToAlphaEffect : CustomEffectBase      
    {
        
        
        public IImageProvider Mask {get; set;}

        public GrayscaleToAlphaEffect(IImageProvider source):
            base(source, false)
        {
           
        }
        public GrayscaleToAlphaEffect(IImageProvider source,   IImageProvider mask):
            base(source, false)
        {
            Mask = mask;
        }

  
        

        protected override void OnProcess(PixelRegion sourcePixelRegion, PixelRegion targetPixelRegion)
        {
            if(Mask == null)
            {
                //copy all pixel
                Array.Copy(sourcePixelRegion.ImagePixels,targetPixelRegion.ImagePixels,sourcePixelRegion.ImagePixels.Length);
                return;
            }

            //create grayscale buffer
            var buffer = new Byte[(int)(sourcePixelRegion.Bounds.Width*sourcePixelRegion.Bounds.Height)];

            //interface grayscale buffer
            var bitmapMask = new Bitmap(
                new Windows.Foundation.Size(sourcePixelRegion.Bounds.Width, sourcePixelRegion.Bounds.Height),
                ColorMode.Gray8,
                (uint)sourcePixelRegion.Bounds.Width,
                buffer.AsBuffer());
            //load grayscale buffer
            Mask.GetBitmapAsync(bitmapMask,OutputOption.Stretch).AsTask().Wait();
     


            sourcePixelRegion.ForEachRow((index, width, pos) =>
            {
                for (int x = 0; x < width; ++x, ++index)
                {
                    uint color = sourcePixelRegion.ImagePixels[index];

                    // copy grayscale buffer to alpha channel 
                    var a = buffer[index];
                    uint rgb = (color & 0x00FFFFFF); 
                    targetPixelRegion.ImagePixels[index] = rgb | (uint)(a << 24);
                }
            }); 
        }
    
    }
}
