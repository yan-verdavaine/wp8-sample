#pragma once
namespace CPP
{
public ref class MyEffect sealed:  Nokia::Graphics::Imaging::ICustomEffect
{
		Windows::Foundation::Size m_imageSize;
		 Windows::Storage::Streams::IBuffer^ m_sourceBuffer;
		 Windows::Storage::Streams::IBuffer^ m_targetBuffer;
		 double factor;
public:
	MyEffect(double f);
	virtual ~MyEffect(void);
	virtual Windows::Foundation::IAsyncAction^ LoadAsync();
	virtual void  Process(Windows::Foundation::Rect rect);
	virtual Windows::Storage::Streams::IBuffer^ ProvideSourceBuffer(Windows::Foundation::Size imageSize);
	virtual Windows::Storage::Streams::IBuffer^ ProvideTargetBuffer(Windows::Foundation::Size imageSize);
};

}