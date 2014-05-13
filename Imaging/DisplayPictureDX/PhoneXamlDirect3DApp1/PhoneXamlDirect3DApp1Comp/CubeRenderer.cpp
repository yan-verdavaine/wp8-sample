#include "pch.h"
#include "CubeRenderer.h"


using namespace DirectX;
using namespace Microsoft::WRL;
using namespace Windows::Foundation;
using namespace Windows::UI::Core;

using namespace concurrency;
using namespace Platform;

CubeRenderer::CubeRenderer() :
	m_loadingComplete(false),
	m_indexCount(0)
{

}


void CubeRenderer::CreateTextureFromByte(byte  *  buffer,int width,int height)
{
    std::lock_guard<std::mutex> lock(m_mutex);
		CD3D11_TEXTURE2D_DESC textureDesc(
			DXGI_FORMAT_B8G8R8A8_UNORM,
			static_cast<UINT>(width),
			static_cast<UINT>(height),
			1,
			1,
			D3D11_BIND_SHADER_RESOURCE
			);
		int pixelSize = sizeof(int); 
		D3D11_SUBRESOURCE_DATA data;
		data.pSysMem = buffer;
		data.SysMemPitch = pixelSize*width;
		data.SysMemSlicePitch =	pixelSize*width*height ;


		DX::ThrowIfFailed(
			m_d3dDevice->CreateTexture2D(
			&textureDesc,
			&data,
			&m_Texture
			)
			);


		m_d3dDevice->CreateShaderResourceView(			m_Texture.Get(),NULL,&SRV); 
		D3D11_SAMPLER_DESC sampDesc;
		ZeroMemory( &sampDesc, sizeof(sampDesc) );
		sampDesc.Filter = D3D11_FILTER_MIN_MAG_MIP_LINEAR;
		sampDesc.AddressU = D3D11_TEXTURE_ADDRESS_WRAP;
		sampDesc.AddressV = D3D11_TEXTURE_ADDRESS_WRAP;
		sampDesc.AddressW = D3D11_TEXTURE_ADDRESS_WRAP;
		sampDesc.ComparisonFunc = D3D11_COMPARISON_NEVER;
		sampDesc.MinLOD = 0;
		sampDesc.MaxLOD = D3D11_FLOAT32_MAX;
		m_d3dDevice->CreateSamplerState( &sampDesc, &CubesTexSamplerState );
}


void CubeRenderer::CreateTexture(int  *  buffer,int width,int height)
{

	if(buffer)
	{
		//use uint32 buffer
		uint32 * uBuffer = (uint32 *)buffer;
		//compenstae alpha 
		std::vector<uint32> ARGBBuffer(width*height);
		//for each pixel
		for (int i =0; i <width*height;++i)
		{
			//extract alpha value
			uint8 a = uBuffer[i] >>24;
			//alpha = 0   => can't compensate RGB value
			//alpha = 255 => premultiplied_ARGB == ARGB
			if(a ==0 || a ==255)
			{

				ARGBBuffer[i] = uBuffer[i];
			}
			else
			{
				//compute alpha cefficient
				double aCoef = (uBuffer[i] >>24)/255.;

				//extract RGB value and compensate alpha coeficient
				uint8 r = (uBuffer[i] >>16 & 0xFF) /aCoef +.5;
				uint8 g = (uBuffer[i] >>8	& 0xFF) /aCoef +.5;
				uint8 b = (uBuffer[i]		& 0xFF) /aCoef +.5;

				//recreate ARGB value to uint32
				ARGBBuffer[i] = (a <<24) + (r <<16) + (g <<8) + b;
			}
        }

        CreateTextureFromByte(  (byte*)(ARGBBuffer.data()), width, height);

	}


}




void CubeRenderer::TakeSnapShot(int  *  buffer,int width,int height)
{
	std::lock_guard<std::mutex> lock(m_mutex);
	
	try
	{
		Microsoft::WRL::ComPtr<ID3D11Texture2D>			renderTarget;
		Microsoft::WRL::ComPtr<ID3D11RenderTargetView>  renderTargetView; 
		
		CD3D11_TEXTURE2D_DESC renderTargetDesc(
		DXGI_FORMAT_B8G8R8A8_UNORM,
		static_cast<UINT>(width),
		static_cast<UINT>(height),
		1,
		1,
		D3D11_BIND_RENDER_TARGET | D3D11_BIND_SHADER_RESOURCE
		);
		renderTargetDesc.MiscFlags = 0;


		DX::ThrowIfFailed(
		m_d3dDevice->CreateTexture2D(
			&renderTargetDesc,
			nullptr,
			&renderTarget
			)
		);
		D3D11_RENDER_TARGET_VIEW_DESC renderTargetViewDesc;
		renderTargetViewDesc.Format = renderTargetDesc.Format;
		renderTargetViewDesc.ViewDimension = D3D11_RTV_DIMENSION_TEXTURE2D;
		renderTargetViewDesc.Texture2D.MipSlice = 0;
		DX::ThrowIfFailed(
		m_d3dDevice->CreateRenderTargetView(
			renderTarget.Get(),
			&renderTargetViewDesc,
			&renderTargetView
			)
		);



		Microsoft::WRL::ComPtr<ID3D11DepthStencilView>  depthStencilView;
		// Create a depth stencil view.
	CD3D11_TEXTURE2D_DESC depthStencilDesc(
		DXGI_FORMAT_D24_UNORM_S8_UINT,
		static_cast<UINT>(width),
		static_cast<UINT>(height),
		1,
		1,
		D3D11_BIND_DEPTH_STENCIL
		);

	ComPtr<ID3D11Texture2D> depthStencil;
	DX::ThrowIfFailed(
		m_d3dDevice->CreateTexture2D(
			&depthStencilDesc,
			nullptr,
			&depthStencil
			)
		);

	CD3D11_DEPTH_STENCIL_VIEW_DESC depthStencilViewDesc(D3D11_DSV_DIMENSION_TEXTURE2D);
	DX::ThrowIfFailed(
		m_d3dDevice->CreateDepthStencilView(
			depthStencil.Get(),
			&depthStencilViewDesc,
			&depthStencilView
			)
		);

	{
		// Set the rendering viewport to target the entire window.
	CD3D11_VIEWPORT viewport(
		0.0f,
		0.0f,
		width,
		height
		);

	m_d3dContext->RSSetViewports(1, &viewport);
	}

		Render(renderTargetView, depthStencilView);

		{
		// Set the rendering viewport to target the entire window.
	CD3D11_VIEWPORT viewport(
		0.0f,
		0.0f,
		m_renderTargetSize.Width,
		m_renderTargetSize.Height
		);

	m_d3dContext->RSSetViewports(1, &viewport);
	}

		CD3D11_TEXTURE2D_DESC renderTargetDesc2(
			DXGI_FORMAT_B8G8R8A8_UNORM,
			static_cast<UINT>(width),
			static_cast<UINT>(height),
			1,
			1,
			0,
			D3D11_USAGE_STAGING,
			D3D11_CPU_ACCESS_READ
			);
		Microsoft::WRL::ComPtr<ID3D11Texture2D>			renderTarget2;
		
		DX::ThrowIfFailed(
		m_d3dDevice->CreateTexture2D(
			&renderTargetDesc2,
			nullptr,
			&renderTarget2
			)
		);



		m_d3dContext->CopyResource(renderTarget2.Get(),renderTarget.Get());
		D3D11_MAPPED_SUBRESOURCE mapped;
	DX::ThrowIfFailed(m_d3dContext->Map( renderTarget2.Get(), 0, D3D11_MAP_READ, 0, &mapped ));

	for(int i = 0;i <width*height; ++i)
	{
		buffer[i]  = ((int*)	mapped.pData)[i];
	}

	m_d3dContext->Unmap(renderTarget2.Get(), 0);


	}
	catch(...)
	{
	}
	
}

void CubeRenderer::CreateDeviceResources()
{
	Direct3DBase::CreateDeviceResources();
	D3D11_BLEND_DESC blendDesc;
	ZeroMemory( &blendDesc, sizeof(blendDesc) );

	D3D11_RENDER_TARGET_BLEND_DESC rtbd;
	ZeroMemory( &rtbd, sizeof(rtbd) );

	
	rtbd.BlendEnable = TRUE;
	rtbd.SrcBlend = D3D11_BLEND_SRC_ALPHA;
	rtbd.DestBlend = D3D11_BLEND_INV_SRC_ALPHA;
	rtbd.BlendOp = D3D11_BLEND_OP_ADD;
	rtbd.SrcBlendAlpha = D3D11_BLEND_ONE;
	rtbd.DestBlendAlpha = D3D11_BLEND_ZERO;
	rtbd.BlendOpAlpha = D3D11_BLEND_OP_ADD;
	rtbd.RenderTargetWriteMask = 0x0f;



	blendDesc.AlphaToCoverageEnable = false;
	blendDesc.RenderTarget[0] = rtbd;

	m_d3dDevice->CreateBlendState(&blendDesc, &Transparency);


	D3D11_RASTERIZER_DESC cmdesc;
	ZeroMemory(&cmdesc, sizeof(D3D11_RASTERIZER_DESC));
    
	cmdesc.FillMode = D3D11_FILL_SOLID;
	cmdesc.CullMode = D3D11_CULL_BACK;
	cmdesc.DepthClipEnable = TRUE;

    
	cmdesc.FrontCounterClockwise = true;
	m_d3dDevice->CreateRasterizerState(&cmdesc, &CCWcullMode);

	cmdesc.FrontCounterClockwise = false;
	m_d3dDevice->CreateRasterizerState(&cmdesc, &CWcullMode);





	auto loadVSTask = DX::ReadDataAsync("SimpleVertexShader.cso");
	auto loadPSTask = DX::ReadDataAsync("SimplePixelShader.cso");

	auto createVSTask = loadVSTask.then([this](Platform::Array<byte>^ fileData) {
		DX::ThrowIfFailed(
			m_d3dDevice->CreateVertexShader(
			fileData->Data,
			fileData->Length,
			nullptr,
			&m_vertexShader
			)
			);

		const D3D11_INPUT_ELEMENT_DESC vertexDesc[] = 
		{
			{ "POSITION",   0, DXGI_FORMAT_R32G32B32_FLOAT, 0, 0,  D3D11_INPUT_PER_VERTEX_DATA, 0 },
			{ "TEXCOORD",    0, DXGI_FORMAT_R32G32_FLOAT, 0, 12, D3D11_INPUT_PER_VERTEX_DATA, 0 },
		};




		DX::ThrowIfFailed(
			m_d3dDevice->CreateInputLayout(
			vertexDesc,
			ARRAYSIZE(vertexDesc),
			fileData->Data,
			fileData->Length,
			&m_inputLayout
			)
			);
	});

	auto createPSTask = loadPSTask.then([this](Platform::Array<byte>^ fileData) {
		DX::ThrowIfFailed(
			m_d3dDevice->CreatePixelShader(
			fileData->Data,
			fileData->Length,
			nullptr,
			&m_pixelShader
			)
			);

		CD3D11_BUFFER_DESC constantBufferDesc(sizeof(ModelViewProjectionConstantBuffer), D3D11_BIND_CONSTANT_BUFFER);
		DX::ThrowIfFailed(
			m_d3dDevice->CreateBuffer(
			&constantBufferDesc,
			nullptr,
			&m_constantBuffer
			)
			);
	});

	auto createCubeTask = (createPSTask && createVSTask).then([this] () {
		Vertex v[] =
		{
			// Front Face
			Vertex(-1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
			Vertex(-1.0f,  1.0f, -1.0f, 0.0f, 0.0f),
			Vertex( 1.0f,  1.0f, -1.0f, 1.0f, 0.0f),
			Vertex( 1.0f, -1.0f, -1.0f, 1.0f, 1.0f),

			// Back Face
			Vertex(-1.0f, -1.0f, 1.0f, 1.0f, 1.0f),
			Vertex( 1.0f, -1.0f, 1.0f, 0.0f, 1.0f),
			Vertex( 1.0f,  1.0f, 1.0f, 0.0f, 0.0f),
			Vertex(-1.0f,  1.0f, 1.0f, 1.0f, 0.0f),

			// Top Face
			Vertex(-1.0f, 1.0f, -1.0f, 0.0f, 1.0f),
			Vertex(-1.0f, 1.0f,  1.0f, 0.0f, 0.0f),
			Vertex( 1.0f, 1.0f,  1.0f, 1.0f, 0.0f),
			Vertex( 1.0f, 1.0f, -1.0f, 1.0f, 1.0f),

			// Bottom Face
			Vertex(-1.0f, -1.0f, -1.0f, 1.0f, 1.0f),
			Vertex( 1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
			Vertex( 1.0f, -1.0f,  1.0f, 0.0f, 0.0f),
			Vertex(-1.0f, -1.0f,  1.0f, 1.0f, 0.0f),

			// Left Face
			Vertex(-1.0f, -1.0f,  1.0f, 0.0f, 1.0f),
			Vertex(-1.0f,  1.0f,  1.0f, 0.0f, 0.0f),
			Vertex(-1.0f,  1.0f, -1.0f, 1.0f, 0.0f),
			Vertex(-1.0f, -1.0f, -1.0f, 1.0f, 1.0f),

			// Right Face
			Vertex( 1.0f, -1.0f, -1.0f, 0.0f, 1.0f),
			Vertex( 1.0f,  1.0f, -1.0f, 0.0f, 0.0f),
			Vertex( 1.0f,  1.0f,  1.0f, 1.0f, 0.0f),
			Vertex( 1.0f, -1.0f,  1.0f, 1.0f, 1.0f),
		};



		D3D11_SUBRESOURCE_DATA vertexBufferData = {0};
		vertexBufferData.pSysMem = v;
		vertexBufferData.SysMemPitch = 0;
		vertexBufferData.SysMemSlicePitch = 0;
		CD3D11_BUFFER_DESC vertexBufferDesc(sizeof(v), D3D11_BIND_VERTEX_BUFFER);
		DX::ThrowIfFailed(
			m_d3dDevice->CreateBuffer(
			&vertexBufferDesc,
			&vertexBufferData,
			&m_vertexBuffer
			)
			);

		DWORD indices[] = {
			// Front Face
			0,  2,  1,
			0,  3,  2,

			// Back Face
			4,  6,  5,
			4,  7,  6,

			// Top Face
			8,  10, 9,
			8, 11, 10,

			// Bottom Face
			12, 14, 13,
			12, 15, 14,

			// Left Face
			16, 18, 17,
			16, 19, 18,

			// Right Face
			20, 22, 21,
			20, 23, 22
		};

		m_indexCount = ARRAYSIZE(indices);

		D3D11_SUBRESOURCE_DATA indexBufferData = {0};
		indexBufferData.pSysMem = indices;
		indexBufferData.SysMemPitch = 0;
		indexBufferData.SysMemSlicePitch = 0;
		CD3D11_BUFFER_DESC indexBufferDesc(sizeof(indices), D3D11_BIND_INDEX_BUFFER);
		DX::ThrowIfFailed(
			m_d3dDevice->CreateBuffer(
			&indexBufferDesc,
			&indexBufferData,
			&m_indexBuffer
			)
			);
	});

	createCubeTask.then([this] () {
		m_loadingComplete = true;
	});
}

void CubeRenderer::CreateWindowSizeDependentResources()
{
	Direct3DBase::CreateWindowSizeDependentResources();

	float aspectRatio = m_windowBounds.Width / m_windowBounds.Height;
	float fovAngleY = 70.0f * XM_PI / 180.0f;
	if (aspectRatio < 1.0f)
	{
		fovAngleY /= aspectRatio;
	}

	XMStoreFloat4x4(
		&m_constantBufferData.projection,
		XMMatrixTranspose(
		XMMatrixPerspectiveFovRH(
		fovAngleY,
		aspectRatio,
		0.01f,
		100.0f
		)
		)
		);
}

void CubeRenderer::Update(float timeTotal, float timeDelta)
{
	(void) timeDelta; // Unused parameter.

	XMVECTOR eye = XMVectorSet(0.0f, 0.0f, 3.f, 0.0f);
	XMVECTOR at = XMVectorSet(0.0f, 0.0f, 0.0f, 0.0f);
	XMVECTOR up = XMVectorSet(0.0f, 1.0f, 0.0f, 0.0f);

	XMStoreFloat4x4(&m_constantBufferData.view, XMMatrixTranspose(XMMatrixLookAtRH(eye, at, up)));
	XMStoreFloat4x4(&m_constantBufferData.model, XMMatrixTranspose(XMMatrixRotationY(timeTotal * XM_PIDIV4)));

    

}

void CubeRenderer::Render()
{

    std::lock_guard<std::mutex> lock(m_mutex);
    Render(m_renderTargetView, m_depthStencilView);
}

void CubeRenderer::Render(Microsoft::WRL::ComPtr<ID3D11RenderTargetView> renderTargetView, Microsoft::WRL::ComPtr<ID3D11DepthStencilView> depthStencilView)
{

	const float black[] = {0, 0, 0, 1.0 };
	m_d3dContext->ClearRenderTargetView(
		renderTargetView.Get(),
		black
		);

	m_d3dContext->ClearDepthStencilView(
		depthStencilView.Get(),
		D3D11_CLEAR_DEPTH,
		1.0f,
		0
		);



	// Only draw the cube once it is loaded (loading is asynchronous).
	if (!SRV || !m_loadingComplete)
	{
		return;
	}

    if(m_camera != nullptr)
    {
           if(m_pNative == nullptr)
           {
               HRESULT hr = reinterpret_cast<IUnknown*>(m_camera)->QueryInterface(__uuidof(ICameraCaptureDeviceNative ),  (void**)m_pNative.GetAddressOf());
               hr = m_pNative->SetPreviewFormat(DXGI_FORMAT_B8G8R8A8_UNORM);
               hr = m_pNative->SetDevice(m_d3dDevice.Get(), m_d3dContext.Get());
           }
       
       auto res = m_pNative->GetPreviewBufferTexture(m_Texture.Get());
    }
    

	m_d3dContext->OMSetRenderTargets(
		1,
		renderTargetView.GetAddressOf(),
		depthStencilView.Get()
		);

	m_d3dContext->UpdateSubresource(
		m_constantBuffer.Get(),
		0,
		NULL,
		&m_constantBufferData,
		0,
		0
		);

	UINT stride = sizeof(Vertex);
	UINT offset = 0;
	m_d3dContext->IASetVertexBuffers(
		0,
		1,
		m_vertexBuffer.GetAddressOf(),
		&stride,
		&offset
		);

	m_d3dContext->IASetIndexBuffer(
		m_indexBuffer.Get(),
		DXGI_FORMAT_R32_UINT,
		0
		);


	m_d3dContext->IASetPrimitiveTopology(D3D11_PRIMITIVE_TOPOLOGY_TRIANGLELIST);

	m_d3dContext->IASetInputLayout(m_inputLayout.Get());

	m_d3dContext->VSSetShader(
		m_vertexShader.Get(),
		nullptr,
		0
		);

	m_d3dContext->VSSetConstantBuffers(
		0,
		1,
		m_constantBuffer.GetAddressOf()
		);

	m_d3dContext->PSSetShader(
		m_pixelShader.Get(),
		nullptr,
		0
		);

	m_d3dContext->PSSetShaderResources( 0, 1, SRV.GetAddressOf());
	m_d3dContext->PSSetSamplers( 0, 1, &CubesTexSamplerState );
	
	//float blendFactor[] = {0.75f, 0.75f, 0.75f, 1.0f};
	m_d3dContext->OMSetBlendState(Transparency.Get(), nullptr, 0xffffffff);

	m_d3dContext->RSSetState(CCWcullMode.Get());
	m_d3dContext->DrawIndexed(
		m_indexCount,
		0,
		0
		);

	m_d3dContext->RSSetState(CWcullMode.Get());
	m_d3dContext->DrawIndexed(
		m_indexCount,
		0,
		0
		);
}



 void CubeRenderer::startPreviewCamera()
 {
    Collections::IVectorView<Size> ^availableSizes = PhotoCaptureDevice::GetAvailableCaptureResolutions(CameraSensorLocation::Back);
	Collections::IIterator<Windows::Foundation::Size> ^availableSizesIterator = availableSizes->First();
   delete m_camera;
    m_camera = nullptr;
    Concurrency::create_task([this,availableSizesIterator](){ return  PhotoCaptureDevice::OpenAsync(CameraSensorLocation::Back, availableSizesIterator->Current);})
    .then([this](PhotoCaptureDevice^ photoCaptureDevice)
    {
         std::lock_guard<std::mutex> lock(m_mutex);
      
         m_pNative.Reset();
      
        
		CD3D11_TEXTURE2D_DESC textureDesc(
			DXGI_FORMAT_B8G8R8A8_UNORM,
			static_cast<UINT>(photoCaptureDevice->PreviewResolution.Width),
			static_cast<UINT>(photoCaptureDevice->PreviewResolution.Height),
			1,
			1,
			D3D11_BIND_SHADER_RESOURCE | D3D11_BIND_RENDER_TARGET
			);
        DX::ThrowIfFailed(
			m_d3dDevice->CreateTexture2D(
			&textureDesc,
			nullptr,
			&m_Texture
			)
			);
        m_d3dDevice->CreateShaderResourceView(			m_Texture.Get(),NULL,&SRV); 
       m_camera = photoCaptureDevice;


    });
           
		

		
 }