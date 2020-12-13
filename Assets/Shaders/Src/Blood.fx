struct vInput
{
	float4 position : POSITION;
	float2 uvBlood : TEXCOORD0;
	float2 uvTiles : TEXCOORD1;
	float2 uvLighting : TEXCOORD2;
};

struct vOutput
{
	float4 position : POSITION;
	float2 uvBlood : TEXCOORD0;
	float2 uvTiles : TEXCOORD1;
	float2 uvLighting : TEXCOORD2;
};

sampler textureSampler0 = sampler_state {
	Texture = <texture0>;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	MagFilter = Point;
	MinFilter = Point;
	Mipfilter = Point;
};

sampler textureSampler1 = sampler_state
{
	Texture = <texture1>;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	MagFilter = Point;
	MinFilter = Point;
	Mipfilter = Point;
};

sampler lightingSampler = sampler_state
{
	Texture = <lightingBuffer>;
	AddressU = Clamp;
	AddressV = Clamp;
	AddressW = Clamp;
	MagFilter = Linear;
	MinFilter = Linear;
	Mipfilter = Linear;
};

Texture texture0 : register(s0);
Texture texture1 : register(s1);
Texture lightingBuffer : register(s2);

float4x4 transformMatrix;

vOutput vert(vInput input)
{
	vOutput output;
	
	output.position = mul(input.position, transformMatrix);
	output.uvBlood = input.uvBlood;
	output.uvTiles = input.uvTiles;
	output.uvLighting = input.uvLighting;
	
	return output;
}

float4 frag(vOutput input) : COLOR
{
	float4 blood = tex2D(textureSampler0, input.uvBlood);
	float4 tiles = tex2D(textureSampler1, input.uvTiles);
	
	if (tiles.a < 0.5) {
		blood = float4(0.0);
	} else {
		blood *= tex2D(lightingSampler, input.uvLighting);
	}
	
	return blood;
}

technique
{
	pass P0
	{
		VertexShader = compile vs_2_0 vert();
		PixelShader = compile ps_2_0 frag();
	}
}
