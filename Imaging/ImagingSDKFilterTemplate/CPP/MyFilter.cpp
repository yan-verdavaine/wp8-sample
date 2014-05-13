#include "pch.h"
#include "MyFilter.h"
#include <wrl.h>
#include <robuffer.h>

using namespace CPP;

MyFilter::MyFilter(double f)
	:factor(f)
{
}


MyFilter::~MyFilter(void)
{
	m_sourceBuffer = nullptr;
	m_targetBuffer = nullptr;
}

Nokia::Graphics::Imaging::ICustomFilterResponse^ MyFilter::BeginProcessing(Nokia::Graphics::Imaging::ICustomFilterRequest^ request)
{
	if(m_sourceBuffer == nullptr || m_sourceBuffer->Capacity !=4*request->SourceBufferLength)
		m_sourceBuffer = ref new Windows::Storage::Streams::Buffer (4*request->SourceBufferLength);
	m_sourceBuffer->Length = m_sourceBuffer->Capacity;

	if(m_targetBuffer == nullptr || m_targetBuffer->Capacity !=4*request->TargetBufferLength)
		m_targetBuffer = ref new Windows::Storage::Streams::Buffer (4*request->TargetBufferLength);
	m_targetBuffer->Length = m_targetBuffer->Capacity;
	return this;

}
		
Windows::Foundation::IAsyncAction^ MyFilter::PrepareAsync()
{
	return nullptr;
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


		
void MyFilter::ProcessBlock(Nokia::Graphics::Imaging::CustomFilterBlockParameters blockParameters)
{

	auto sourceBuffer = GetPointerToPixelData(m_sourceBuffer);
	auto targetBuffer = GetPointerToPixelData(m_targetBuffer);

	for(int y =0;y< blockParameters.Height;++y)
	{
		int index = 4*blockParameters.SourceStartIndex + y* 4*blockParameters.SourcePitch;
		for(int x =0;x< blockParameters.Width;++x, index += 4)
		{

			targetBuffer[index] =   range(factor*sourceBuffer[index]); //B
			targetBuffer[index+1] = range(factor*sourceBuffer[index+1]); //G
			targetBuffer[index+2] = range(factor*sourceBuffer[index+2]); ///R
			targetBuffer[index+3] = sourceBuffer[index+3]; // A

		}
	}



}