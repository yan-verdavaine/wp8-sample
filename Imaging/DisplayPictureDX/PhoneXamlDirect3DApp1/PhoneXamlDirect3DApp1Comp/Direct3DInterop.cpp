#include "pch.h"
#include "Direct3DInterop.h"
#include "Direct3DContentProvider.h"
#include <windows.storage.streams.h>
#include <wrl.h>
#include <robuffer.h>
using namespace Windows::Storage::Streams;
using namespace Microsoft::WRL;



using namespace Windows::Foundation;
using namespace Windows::UI::Core;
using namespace Microsoft::WRL;
using namespace Windows::Phone::Graphics::Interop;
using namespace Windows::Phone::Input::Interop;
using namespace Nokia::Graphics::Imaging;
namespace PhoneXamlDirect3DApp1Comp
{

    Direct3DInterop::Direct3DInterop() :
        m_timer(ref new BasicTimer())
    {
    }

    IDrawingSurfaceContentProvider^ Direct3DInterop::CreateContentProvider()
    {
        ComPtr<Direct3DContentProvider> provider = Make<Direct3DContentProvider>(this);
        return reinterpret_cast<IDrawingSurfaceContentProvider^>(provider.Detach());
    }

    // IDrawingSurfaceManipulationHandler
    void Direct3DInterop::SetManipulationHost(DrawingSurfaceManipulationHost^ manipulationHost)
    {
        manipulationHost->PointerPressed +=
            ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerPressed);

        manipulationHost->PointerMoved +=
            ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerMoved);

        manipulationHost->PointerReleased +=
            ref new TypedEventHandler<DrawingSurfaceManipulationHost^, PointerEventArgs^>(this, &Direct3DInterop::OnPointerReleased);
    }

    void Direct3DInterop::RenderResolution::set(Windows::Foundation::Size renderResolution)
    {
        if (renderResolution.Width  != m_renderResolution.Width ||
            renderResolution.Height != m_renderResolution.Height)
        {
            m_renderResolution = renderResolution;

            if (m_renderer)
            {
                m_renderer->UpdateForRenderResolutionChange(m_renderResolution.Width, m_renderResolution.Height);
                RecreateSynchronizedTexture();
            }
        }
    }

    // Event Handlers
    void Direct3DInterop::OnPointerPressed(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
    {
        // Insert your code here.
    }

    void Direct3DInterop::OnPointerMoved(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
    {
        // Insert your code here.
    }

    void Direct3DInterop::OnPointerReleased(DrawingSurfaceManipulationHost^ sender, PointerEventArgs^ args)
    {
        // Insert your code here.
    }

    // Interface With Direct3DContentProvider
    HRESULT Direct3DInterop::Connect(_In_ IDrawingSurfaceRuntimeHostNative* host)
    {
        m_renderer = ref new CubeRenderer();
        m_renderer->Initialize();
        m_renderer->UpdateForWindowSizeChange(WindowBounds.Width, WindowBounds.Height);
        m_renderer->UpdateForRenderResolutionChange(m_renderResolution.Width, m_renderResolution.Height);

        // Restart timer after renderer has finished initializing.
        m_timer->Reset();

        return S_OK;
    }

    void Direct3DInterop::Disconnect()
    {
        m_renderer = nullptr;
    }

    HRESULT Direct3DInterop::PrepareResources(_In_ const LARGE_INTEGER* presentTargetTime, _Out_ BOOL* contentDirty)
    {
        *contentDirty = true;

        return S_OK;
    }

    HRESULT Direct3DInterop::GetTexture(_In_ const DrawingSurfaceSizeF* size, _Out_ IDrawingSurfaceSynchronizedTextureNative** synchronizedTexture, _Out_ DrawingSurfaceRectF* textureSubRectangle)
    {
        m_timer->Update();
        m_renderer->Update(m_timer->Total, m_timer->Delta);
        m_renderer->Render();

        RequestAdditionalFrame();

        return S_OK;
    }

    ID3D11Texture2D* Direct3DInterop::GetTexture()
    {
        return m_renderer->GetTexture();
    }


    void Direct3DInterop::CreateTexture(const Platform::Array<int>^  buffer,int with,int height)
    {
       if(m_renderer)
            m_renderer->CreateTexture(buffer->Data,with,height);
    }


    byte* GetPointerToPixelData( Windows::Storage::Streams::IBuffer ^ pixelBuffer)
    {

        // Query the IBufferByteAccess interface.
        ComPtr<IBufferByteAccess> bufferByteAccess;
        reinterpret_cast<IInspectable*>( pixelBuffer)->QueryInterface(IID_PPV_ARGS(&bufferByteAccess));

        // Retrieve the buffer data.
        byte* pixels = nullptr;
        bufferByteAccess->Buffer(&pixels);
        return pixels;
    }


    Windows::Foundation::IAsyncAction^ Direct3DInterop::CreateTextureFromFileAsync(Windows::Storage::Streams::IBuffer^ file,int width,int height)
    {
        //Create an ImageSource which decode the picture file
        auto source =  ref new BufferImageSource(file);
        return Concurrency::create_async([this,source,width, height]()
        {
                //create a buffer to decode the picture.
                auto outBuffer = ref new Windows::Storage::Streams::Buffer(4*width*height);
                outBuffer->Length = outBuffer->Capacity;

                //interface the buffer in a Bitmap
                auto bitmap = ref new Bitmap( Windows::Foundation::Size(width, height),
                    ColorMode::Bgra8888,
                    4*width,
                    outBuffer);
                //crete the renderer to decode the ImageSOurce to the buffer
                auto render = ref new BitmapRenderer(source,bitmap);

                return Concurrency::create_task(
                    //launch rendering
                    [this,render](){return render->RenderAsync();}
                )
                    .then(
                    [this,outBuffer,width, height](Bitmap ^bmp)
                {

                    //image is decoded => create the texture
                    if(m_renderer)
                        m_renderer->CreateTextureFromByte(GetPointerToPixelData(outBuffer), width, height);
                }

                );
            }
            );



    }

    Windows::Foundation::IAsyncAction^ Direct3DInterop::CreateTextureFromFileAsync(Windows::Storage::Streams::IBuffer^ file)
    {
        //Create an ImageSource which decode the picture file
        auto source =  ref new BufferImageSource(file);

        return Concurrency::create_async([this,source]()
        {
            return Concurrency::create_task(
                //get Picture size
                [this,source](){return source->GetInfoAsync();}
            )
                .then(
                [this,source](ImageProviderInfo ^info)
            {
                //create a buffer to decode the picture.
                auto outBuffer = ref new Windows::Storage::Streams::Buffer(4*info->ImageSize.Width*info->ImageSize.Height);
                outBuffer->Length = outBuffer->Capacity;

                //interface the buffer in a Bitmap
                auto bitmap = ref new Bitmap(info->ImageSize,
                    ColorMode::Bgra8888,
                    4*info->ImageSize.Width,
                    outBuffer);
                //crete the renderer to decode the ImageSOurce to the buffer
                auto render = ref new BitmapRenderer(source,bitmap);

                return Concurrency::create_task(
                    //launch rendering
                    [this,render](){return render->RenderAsync();}
                )
                    .then(
                    [this,outBuffer,info](Bitmap ^bmp)
                {

                    //image is decoded => create the texture
                    if(m_renderer)
                        m_renderer->CreateTextureFromByte(GetPointerToPixelData(outBuffer), info->ImageSize.Width, info->ImageSize.Height);
                }

                );
            }
            );
        });
    }

    void Direct3DInterop::TakeSnapShot(const Platform::Array<int>^ buffer,int width,int height)
    {
        if(m_renderer)
            m_renderer->TakeSnapShot(buffer->Data, width, height);
    }
    Windows::Foundation::IAsyncOperation<Windows::Storage::Streams::IBuffer^>^ Direct3DInterop::TakeSnapShotAsync(int width,int height)
    {
        return Concurrency::create_async([this,width,height](){

         auto pixelsBuffer = ref new Windows::Storage::Streams::Buffer(4*width*height);
         pixelsBuffer->Length = pixelsBuffer->Capacity;


             if(m_renderer)
                m_renderer->TakeSnapShot((int*)GetPointerToPixelData(pixelsBuffer), width, height);

              //interface the buffer in a Bitmap
             auto bitmap = ref new Bitmap( Windows::Foundation::Size(width, height),
                    ColorMode::Bgra8888,
                    4*width,
                    pixelsBuffer);
             auto source = ref new BitmapImageSource(bitmap);
             auto renderer = ref new JpegRenderer(source);

             return    Concurrency::create_task([renderer](){return renderer->RenderAsync();});
        });
          
 


    }
    void Direct3DInterop::StartPreviewCamera()
    {
        if(m_renderer)
            m_renderer->startPreviewCamera();
    }
}
