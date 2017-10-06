#if OPENGL
	#define SV_POSITION POSITION
	#define VS_SHADERMODEL vs_3_0
	#define PS_SHADERMODEL ps_3_0
#else
	#define VS_SHADERMODEL vs_4_0_level_9_1
	#define PS_SHADERMODEL ps_4_0_level_9_1
#endif

Texture2D SpriteTexture;
sampler2D SpriteTextureSampler = sampler_state
{
	Texture = <SpriteTexture>;
};

Texture2D MaskTexture;
sampler2D MaskTextureSampler = sampler_state
{
	Texture = <MaskTexture>;
	MinFilter = Point;
	MagFilter = Point;
	MipFilter = Point;
	AddressU = Clamp;
	AddressV = Clamp;
};

float2 MaskBasePosition;
float2 TileBasePosition;

float BlockSize = 8 / 128.0;

struct VertexShaderOutput
{
	float4 Position : SV_POSITION;
	float4 Color : COLOR0;
	float2 TextureCoordinates : TEXCOORD0;
};

float4 MainPS(VertexShaderOutput input) : COLOR
{
	float4 TileColor = tex2D(SpriteTextureSampler, input.TextureCoordinates);
	float2 PositionInTile = float2((input.TextureCoordinates.x - TileBasePosition.x), (input.TextureCoordinates.y - TileBasePosition.y));
	float4 MaskColor = tex2D(MaskTextureSampler, float2(MaskBasePosition.x + PositionInTile.x, MaskBasePosition.y + PositionInTile.y));
	return TileColor * (1 - MaskColor.r);
}

technique SpriteDrawing
{
	pass P0
	{
		PixelShader = compile PS_SHADERMODEL MainPS();
	}
};