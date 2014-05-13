// WindowsPhoneRuntimeComponent1.cpp
#include "pch.h"
#include "WindowsPhoneRuntimeComponent1.h"
#include "implements.h"
#include <ppltasks.h>
#include <wrl/implements.h>
#include <windows.storage.streams.h>
#include <robuffer.h>


using namespace concurrency;
using namespace WindowsPhoneRuntimeComponent1;
using namespace Platform;
using namespace Microsoft::WRL;
using namespace Nokia::Graphics::Imaging;








WindowsPhoneRuntimeComponent::WindowsPhoneRuntimeComponent():
	pNativeFrame(NULL),
    pBuffer(NULL)
{
}

IAsyncAction^ WindowsPhoneRuntimeComponent::InitCapture()
{
	return create_async([this] {

		Collections::IVectorView<Size> ^availableSizes = PhotoCaptureDevice::GetAvailableCaptureResolutions(CameraSensorLocation::Back);
		Collections::IIterator<Windows::Foundation::Size> ^availableSizesIterator = availableSizes->First();

		IAsyncOperation<PhotoCaptureDevice^> ^openOperation = nullptr;
		if (availableSizesIterator->HasCurrent)
		{
			ImageSize = availableSizesIterator->Current;
			openOperation = PhotoCaptureDevice::OpenAsync(CameraSensorLocation::Back, availableSizesIterator->Current);
		} else
		{
			throw ref new FailureException("Can't open the camera");
		}

		return create_task(openOperation).then([this](PhotoCaptureDevice^ photoCaptureDevice)
		{
			::OutputDebugString(L"+[WindowsPhoneRuntimeComponent::InitCapture] => OpenAsync Completed\n");
			this->m_camera = photoCaptureDevice;

			m_cameraCaptureSequence = m_camera->CreateCaptureSequence(1);

			return m_camera->PrepareCaptureSequenceAsync(m_cameraCaptureSequence);

		}).then([](){
			::OutputDebugString(L"+[WindowsPhoneRuntimeComponent::InitCapture] => PrepareAsync Completed\n");	
		});

	});
}

class NativeBuffer : 
    public Microsoft::WRL::RuntimeClass<Microsoft::WRL::RuntimeClassFlags<Microsoft::WRL::RuntimeClassType::WinRtClassicComMix>,
    ABI::Windows::Storage::Streams::IBuffer,
    Windows::Storage::Streams::IBufferByteAccess>
{
public:
    virtual ~NativeBuffer()
    {
    }

    STDMETHODIMP RuntimeClassInitialize(byte *buffer, UINT totalSize)
    {
        m_length = totalSize;
        m_buffer = buffer;
        return S_OK;
    }

    STDMETHODIMP Buffer(byte **value)
    {
        *value = m_buffer;

        return S_OK;
    }

    STDMETHODIMP get_Capacity(UINT32 *value)
    {
        *value = m_length;

        return S_OK;
    }

    STDMETHODIMP get_Length(UINT32 *value)
    {
        *value = m_length;

        return S_OK;
    }

    STDMETHODIMP put_Length(UINT32 value)
    {
        m_length = value;

        return S_OK;
    }

private:
    UINT32 m_length;
    byte *m_buffer;
};
IBuffer^ CreateBuffer(byte * buffer, UINT32 cbBytes)
	{
		ComPtr<NativeBuffer> nativeBuffer;
		MakeAndInitialize<NativeBuffer>(&nativeBuffer, buffer,cbBytes);
		auto iinspectable = (IInspectable*)reinterpret_cast<IInspectable*>(nativeBuffer.Get());
		return reinterpret_cast<IBuffer^>(iinspectable);

	}
IAsyncAction^ WindowsPhoneRuntimeComponent::CaptureImage()
{
    if (pNativeFrame != NULL)
    {
        pNativeFrame->UnmapBuffer();
    }

    return create_async([this]
    {

        return   create_task(m_camera->FocusAsync())
            .then([this](CameraFocusStatus status){return m_cameraCaptureSequence->StartCaptureAsync();} )
            .then([this]()
            {
                ::OutputDebugString(L"+[WindowsPhoneRuntimeComponent::CaptureImage] => Capture Completed \n");

                CameraCaptureFrame^ frame = m_cameraCaptureSequence->Frames->GetAt(0);


                HRESULT hr = reinterpret_cast<IUnknown*>(frame)->QueryInterface(__uuidof(ICameraCaptureFrameNative ), (void**) &pNativeFrame);

                if (NULL == pNativeFrame || FAILED(hr))
                {
                    throw ref new FailureException("Unable to QI ICameraCaptureFrameNative");
                }

                m_bufferSize=0;
                pBuffer = NULL;
                pNativeFrame->MapBuffer(&m_bufferSize, &pBuffer); // Pixels are in pBuffer. 
                auto inputScanlines = ref new Array<uint32>(2);
                inputScanlines->set(0,ImageSize.Width);
                inputScanlines->set(1,ImageSize.Width);
                int size =ImageSize.Width*ImageSize.Height;
                  auto inputBuffers = ref new Array<IBuffer^>(2);
                inputBuffers->set(0,  CreateBuffer(pBuffer,size));
                inputBuffers->set(1,  CreateBuffer(pBuffer + size,size));
                


              m_bitmap = ref new Bitmap(Size(ImageSize.Width,ImageSize.Height),
                    ColorMode::Yuv420Sp,
                    inputScanlines,
                    inputBuffers);
    

            });
        });
}


void WindowsPhoneRuntimeComponent::GetImageArray(Platform::WriteOnlyArray<byte>^ arr)
{
	for(unsigned int i = 0; i < m_bufferSize; i++)
	{
		arr[i] = pBuffer[i];
	}   
}

IAsyncAction^ WindowsPhoneRuntimeComponent::render(Bitmap^ bout)
{
    auto source =ref new BitmapImageSource(m_bitmap);
    auto renderer = ref new BitmapRenderer(source,bout);

    return create_async([source,renderer]
    {
        return create_task( renderer->RenderAsync()).then([](Bitmap^ tmp){});
    });
}