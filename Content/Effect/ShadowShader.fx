sampler2D SpriteTextureSampler : register(s0);

float2 TextureSize; 
int ShadowSize;     
float4 ShadowColor; 
float CameraScale;

struct VertexShaderOutput {
    float4 Position : SV_Position;
    float4 Color : COLOR0;
    float2 TextureCoordinates : TEXCOORD0;
};

float4 ShadowPS(VertexShaderOutput input) : COLOR {
    float texelWidth = 1.0 / TextureSize.x;
    float texelHeight = 1.0 / TextureSize.y;
    float fShadowSize = (float)ShadowSize;

    float stepX = texelWidth * CameraScale;
    float stepY = texelHeight * CameraScale;

    [loop]
    for (int i = -16; i <= 16; i++) {
        if (abs((float)i) <= fShadowSize) {
            float2 offset = float2(i * stepX, 0);
            float4 neighbor = tex2Dlod(SpriteTextureSampler, float4(input.TextureCoordinates + offset, 0, 0));
            if (neighbor.a > 0) return ShadowColor;
        }
    }
    
    [loop]
    for (int i = -16; i <= 16; i++) {
        if (abs((float)i) <= fShadowSize) {
            float2 offset = float2(0, i * stepY);
            float4 neighbor = tex2Dlod(SpriteTextureSampler, float4(input.TextureCoordinates + offset, 0, 0));
            if (neighbor.a > 0) return ShadowColor;
        }
    }
   
    discard;
    return float4(0, 0, 0, 0);
}


technique Shadow {
    pass Fully { PixelShader = compile ps_3_0 ShadowPS(); }
};