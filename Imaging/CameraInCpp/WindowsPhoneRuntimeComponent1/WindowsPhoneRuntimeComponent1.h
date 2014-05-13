#pragma once

#include <Windows.Phone.Media.Capture.h>
#include <Windows.Phone.Media.Capture.Native.h>
#include <windows.h>
#include <ppltasks.h>

using namespace Windows::Storage::Streams;
using namespace Windows::Foundation;
using namespace Windows::Phone::Media::Capture;

namespace WindowsPhoneRuntimeComponent1
{
	public ref class WindowsPhoneRuntimeComponent sealed
	{
	public:
		WindowsPhoneRuntimeComponent();
		IAsyncAction^ CaptureImage();
		IAsyncAction^ InitCapture();
		void GetImageArray(Platform::WriteOnlyArray<byte>^ arr);
        IAsyncAction^ render(Nokia::Graphics::Imaging::Bitmap^ bout);
		property Windows::Foundation::Size ImageSize;
        property PhotoCaptureDevice^ captureDevice 
            {
               PhotoCaptureDevice^  get(){return m_camera;}
        }

	private:
		PhotoCaptureDevice^ m_camera;
		CameraCaptureSequence^ m_cameraCaptureSequence;
        Nokia::Graphics::Imaging::Bitmap ^m_bitmap;
		ICameraCaptureFrameNative *pNativeFrame;

		DWORD m_bufferSize;
		BYTE * pBuffer;
	};
}