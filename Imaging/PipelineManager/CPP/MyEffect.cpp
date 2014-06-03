#include "pch.h"
#include "MyEffect.h"

//include use to acces IBuffer memory
#include <wrl.h>
#include <robuffer.h>

//concurrency::create_async
#include <ppltasks.h>

using namespace CPP;

MyEffect::MyEffect(double f):factor(f)
{
}


MyEffect::~MyEffect(void)
{
	m_sourceBuffer = nullptr;
	m_targetBuffer = nullptr;
}


Windows::Foundation::IAsyncAction^ MyEffect::LoadAsync()
{
	//if you don't need to load data, return nullptr
	return nullptr;

	//or use concurrency::create_async
	return concurrency::create_async([this]()
	{
		//load you data here
	});
}

Windows::Storage::Streams::IBuffer^ MyEffect::ProvideSourceBuffer(Windows::Foundation::Size imageSize)
{
	m_imageSize = imageSize;
	if(m_sourceBuffer == nullptr || m_sourceBuffer->Capacity != 4*imageSize.Width*imageSize.Height)
	{
		m_sourceBuffer = ref new Windows::Storage::Streams::Buffer (4*imageSize.Width*imageSize.Height);
		m_sourceBuffer->Length = m_sourceBuffer->Capacity;
	}

	return m_sourceBuffer;
}
Windows::Storage::Streams::IBuffer^ MyEffect::ProvideTargetBuffer(Windows::Foundation::Size imageSize)
{
	m_imageSize = imageSize;
	if(m_targetBuffer == nullptr || m_targetBuffer->Capacity != 4*imageSize.Width*imageSize.Height)
	{
		m_targetBuffer = ref new Windows::Storage::Streams::Buffer (4*imageSize.Width*imageSize.Height);
		m_targetBuffer->Length = m_targetBuffer->Capacity;
	}

	return m_targetBuffer;
}

namespace
{
	byte* GetPointerToPixelData(Windows::Storage::Streams::IBuffer^ pixelBuffer, unsigned int *length = nullptr)
	{
		if (length != nullptr)
		{
			*length = pixelBuffer ->Length;
		}
		// Query the IBufferByteAccess interface.
		Microsoft::WRL::ComPtr< Windows::Storage::Streams::IBufferByteAccess> bufferByteAccess;
		reinterpret_cast<IInspectable*>( pixelBuffer)->QueryInterface(IID_PPV_ARGS(&bufferByteAccess));

		// Retrieve the buffer data.
		byte* pixels = nullptr;
		bufferByteAccess->Buffer(&pixels);
		return pixels;
	}

	template<typename T>
	byte range(T v)
	{
		return v<0 ? (byte)0 : v>255 ? (byte)255 : (byte)v+.5;
	}
}




void  MyEffect::Process(Windows::Foundation::Rect rect)
{

	auto sourceBuffer = GetPointerToPixelData(m_sourceBuffer);
	auto targetBuffer = GetPointerToPixelData(m_targetBuffer);

	for(int y =rect.Top;y< rect.Bottom;++y)
	{
		int StartIndex = y* 4*m_imageSize.Width;

		for(int x =rect.Left;x < rect.Right;++x)
		{
			int index = StartIndex + 4*x;

			targetBuffer[index] =   range(factor*sourceBuffer[index]); //B
			targetBuffer[index+1] = range(factor*sourceBuffer[index+1]); //G
			targetBuffer[index+2] = range(factor*sourceBuffer[index+2]); ///R
			targetBuffer[index+3] = sourceBuffer[index+3]; // A

		}
	}



}
