#pragma warning (disable : 4717)

struct vInput {
	float4 position : POSITION;
	float2 uvBlood : TEXCOORD0;
	float2 uvTiles : TEXCOORD1;
	float2 uvLighting : TEXCOORD2;
};

struct vOutput {
	float4 position : POSITION;
	float2 uvBlood : TEXCOORD0;
	float2 uvTiles : TEXCOORD1;
	float2 uvLighting : TEXCOORD2;
};

float4x4 transformMatrix;
Texture2D texture0 : register(s0);
Texture2D maskTexture : register(s1);
Texture2D lightingBuffer : register(s2);

sampler textureSampler0 = sampler_state {
	Texture = texture0;
	AddressU = Clamp; AddressV = Clamp; AddressW = Clamp;
	MagFilter = Point; MinFilter = Point; Mipfilter = Point;
};
sampler maskTextureSampler = sampler_state {
	Texture = maskTexture;
	AddressU = Clamp; AddressV = Clamp; AddressW = Clamp;
	MagFilter = Point; MinFilter = Point; Mipfilter = Point;
};
sampler lightingSampler = sampler_state {
	Texture = lightingBuffer;
	AddressU = Clamp; AddressV = Clamp; AddressW = Clamp;
	MagFilter = Linear; MinFilter = Linear; Mipfilter = Linear;
};

vOutput vert(vInput input) {
	vOutput output;
	
	output.position = mul(input.position, transformMatrix);
	output.uvBlood = input.uvBlood;
	output.uvTiles = input.uvTiles;
	output.uvLighting = input.uvLighting;
	
	return output;
}

float4 frag(vOutput input) : COLOR {
	float4 decals = tex2D(textureSampler0, input.uvBlood);
	float4 mask = tex2D(maskTextureSampler, input.uvTiles);
	
	if (mask.a < 0.05) {
		decals = float4(0.0, 0.0, 0.0, 0.0);
	} else {
		decals *= tex2D(lightingSampler, input.uvLighting);
	}
	
	return decals;
}

technique {
	pass P0 {
		VertexShader = compile vs_2_0 vert();
		PixelShader = compile ps_2_0 frag();
	}
}
