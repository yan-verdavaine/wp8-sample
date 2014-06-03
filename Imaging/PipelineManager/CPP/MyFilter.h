#pragma once
namespace CPP
{
	public ref class MyFilter  sealed:  Nokia::Graphics::Imaging::ICustomFilter, Nokia::Graphics::Imaging::ICustomFilterResponse
	{

		 Windows::Storage::Streams::IBuffer^ m_sourceBuffer;
		 Windows::Storage::Streams::IBuffer^ m_targetBuffer;
		 double factor;
	public:
		MyFilter(double f);
		 virtual ~MyFilter(void);

		 //Nokia::Graphics::Imaging::ICustomFilter implementation
		virtual Nokia::Graphics::Imaging::ICustomFilterResponse^ BeginProcessing(Nokia::Graphics::Imaging::ICustomFilterRequest^ request);
		virtual Windows::Foundation::IAsyncAction^ PrepareAsync();
		virtual void ProcessBlock(Nokia::Graphics::Imaging::CustomFilterBlockParameters blockParameters);



		//Nokia::Graphics::Imaging::ICustomFilterResponse implementation
		 virtual property Nokia::Graphics::Imaging::ColorMode ColorMode
		 { 
			 Nokia::Graphics::Imaging::ColorMode get(){return Nokia::Graphics::Imaging::ColorMode::Bgra8888; }
		 }
		 virtual property Windows::Storage::Streams::IBuffer^ SourceBuffer { 
			  Windows::Storage::Streams::IBuffer^  get(){return m_sourceBuffer;}
		 }
		 virtual property Windows::Storage::Streams::IBuffer^ TargetBuffer { 
			  Windows::Storage::Streams::IBuffer^  get(){return m_targetBuffer;}
		 }

	};

}