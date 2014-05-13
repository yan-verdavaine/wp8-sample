#pragma once

#include "Direct3DBase.h"
#include <d3d11.h>
#include <mutex>
#include <Windows.Phone.Media.Capture.h>
#include <Windows.Phone.Media.Capture.Native.h>


using namespace Windows::Phone::Media::Capture;

struct ModelViewProjectionConstantBuffer
{
	DirectX::XMFLOAT4X4 model;
	DirectX::XMFLOAT4X4 view;
	DirectX::XMFLOAT4X4 projection;
};

struct Vertex	//Overloaded Vertex Structure
{
	Vertex(){}
	Vertex(float x, float y, float z,
		float u, float v)
		: pos(x,y,z), texCoord(u, v){}

	DirectX::XMFLOAT3 pos;
	DirectX::XMFLOAT2 texCoord;
};

// This class renders a simple spinning cube.
ref class CubeRenderer sealed : public Direct3DBase
{
public:
	CubeRenderer();

	// Direct3DBase methods.
	virtual void CreateDeviceResources() override;
	virtual void CreateWindowSizeDependentResources() override;
   virtual void Render() override;
	
	// Method for updating time-dependent objects.
	void Update(float timeTotal, float timeDelta);

	void CreateTexture(int  *  buffer,int with,int height);
    void CreateTextureFromByte(byte  *  buffer,int width,int height);
    void TakeSnapShot(int  *  buffer,int width,int height);

    void startPreviewCamera();
private:
    void Render(Microsoft::WRL::ComPtr<ID3D11RenderTargetView> renderTargetView, Microsoft::WRL::ComPtr<ID3D11DepthStencilView> depthStencilView);
	bool m_loadingComplete;

	Microsoft::WRL::ComPtr<ID3D11InputLayout>	m_inputLayout;
	Microsoft::WRL::ComPtr<ID3D11Buffer>		m_vertexBuffer;
	Microsoft::WRL::ComPtr<ID3D11Buffer>		m_indexBuffer;
	Microsoft::WRL::ComPtr<ID3D11VertexShader>	m_vertexShader;
	Microsoft::WRL::ComPtr<ID3D11PixelShader>	m_pixelShader;
	Microsoft::WRL::ComPtr<ID3D11Buffer>		m_constantBuffer;
	Microsoft::WRL::ComPtr<ID3D11Texture2D>		 m_Texture;
	Microsoft::WRL::ComPtr<ID3D11ShaderResourceView> SRV;
	Microsoft::WRL::ComPtr<ID3D11SamplerState> CubesTexSamplerState;
	uint32 m_indexCount;
	ModelViewProjectionConstantBuffer m_constantBufferData;
    std::mutex   m_mutex;
	Microsoft::WRL::ComPtr<ID3D11BlendState> Transparency;
	Microsoft::WRL::ComPtr<ID3D11RasterizerState> CCWcullMode;
	Microsoft::WRL::ComPtr<ID3D11RasterizerState> CWcullMode;

    Microsoft::WRL::ComPtr<ICameraCaptureDeviceNative> m_pNative;
    PhotoCaptureDevice^ m_camera;
	
};
