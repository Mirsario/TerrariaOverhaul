// Config icon shader

texture2D Texture;
sampler TextureSampler : register(ps, s0) = sampler_state {
	Texture = (Texture);
	AddressU = Clamp;
	AddressV = Clamp;
	MagFilter = Point;
	MinFilter = Point;
	Mipfilter = Point;
};
texture2D Background;
sampler BackgroundSampler : register(ps, s1) = sampler_state {
	Texture = (Background);
	AddressU = Clamp;
	AddressV = Clamp;
	MagFilter = Point;
	MinFilter = Point;
	Mipfilter = Point;
};

float Time;
float2 Resolution;

float Sin01(float i)
{
    return sin(i) * 0.5 + 0.5;
}

float4 SpritePixelShader(float4 color : COLOR0, float2 texCoord : TEXCOORD0) : SV_Target0
{
	float4 result = float4(0.0, 0.0, 0.0, 0.0);
	float2 p = 1.0 / Resolution;
	float4 background;
	
	{
		float2 backgroundUV = lerp(float2(0.5, 0.5), texCoord, lerp(0.75, 1.0, Sin01(Time * 2.0)));
		backgroundUV = floor(backgroundUV * Resolution) / Resolution;
		
		background = tex2D(BackgroundSampler, backgroundUV);
		result = lerp(result, background, background.a);
	}

	{
		//float intensity = 0.0;
		//float4 outlineColor = float4(1.0 - background.rgb, 1.0);
		//intensity = min(intensity, 1.0);
		
		float4 outlineColor = (
			+ tex2D(TextureSampler, texCoord + float2(-p.x,  0.0))
			+ tex2D(TextureSampler, texCoord + float2( 0.0, -p.y))
			+ tex2D(TextureSampler, texCoord + float2( p.x,  0.0))
			+ tex2D(TextureSampler, texCoord + float2( 0.0,  p.y))
		);
		
		outlineColor /= outlineColor.a > 0.0 ? outlineColor.a : 1.0;
		outlineColor.rgb = 1.0 - outlineColor.rgb;
		outlineColor.rgb = lerp(outlineColor.rgb, 1.0 - background.rgb, 0.65); //((outlineColor.rgb) + (1.0 - background.rgb) + float3(1.0, 1.0, 1.0)) / 3;
		
		result = lerp(result, outlineColor, outlineColor.a);
	}

	{
		float4 foreground = tex2D(TextureSampler, texCoord);
		
		result = lerp(result, foreground, foreground.a);
	}
	
	return result;
}

technique {
	pass P0 {
		PixelShader = compile ps_2_0 SpritePixelShader();
	}
}
