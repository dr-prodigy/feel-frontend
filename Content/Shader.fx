float4x4 World;
float4x4 View;
float4x4 Projection;

// TODO: add effect parameters here.
sampler ColorMapSampler : register(s0);
texture ColorMap2;
sampler ColorMapSampler2 = sampler_state
{
   Texture = <ColorMap2>;
   MinFilter = Linear;
   MagFilter = Linear;
   MipFilter = Linear;  
   AddressU  = Clamp;
   AddressV  = Clamp;
};
 
float fFadeAmount;
float fSmoothSize;

struct VertexShaderInput
{
    float4 Position : POSITION0;

    // TODO: add input channels such as texture
    // coordinates and vertex colors here.
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;

    // TODO: add vertex shader outputs such as colors and texture
    // coordinates here. These values will automatically be interpolated
    // over the triangle, and provided as input to your pixel shader.
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    // TODO: add your vertex shader code here.

    return output;
}

// Fade Transition
float4 PixelShaderFade(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, Tex); 
	float4 Color2 = tex2D(ColorMapSampler2, Tex); 

	float4 finalColor = lerp(Color2,Color,fFadeAmount);

	// Set our alphachannel to fAlphaAmount.
	finalColor.a = 1;

	return finalColor;
}

// Slide Transition
float4 PixelShaderSlide(float2 Tex: TEXCOORD0) : COLOR
{
	float4 Color = tex2D(ColorMapSampler, Tex); 
	float4 Color2 = tex2D(ColorMapSampler2, Tex); 

	float4 finalColor = lerp(Color,Color2,smoothstep(fFadeAmount,fFadeAmount+fSmoothSize,Tex.y));

	// Set our alphachannel to fAlphaAmount.
	finalColor.a = 1;

	return finalColor;
}

technique PostProcessSlide
{
	pass P0
	{
		// A post process shader only needs a pixel shader.
		PixelShader = compile ps_2_0 PixelShaderSlide();
	}
}

technique PostProcessFade
{
	pass P0
	{
		// A post process shader only needs a pixel shader.
		PixelShader = compile ps_2_0 PixelShaderFade();
	}
}
