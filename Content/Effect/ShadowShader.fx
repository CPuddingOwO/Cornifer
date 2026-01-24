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

// --- 水平扫描 ---
float4 HorizontalPS(VertexShaderOutput input) : COLOR {
    float texelWidth = 1.0 / TextureSize.x;
    float fShadowSize = (float)ShadowSize;
    float step = texelWidth * CameraScale;

    [loop]
    for (int i = -16; i <= 16; i++) {
        if (abs((float)i) <= fShadowSize) {
            float2 offset = float2(i * step, 0);
            float4 neighbor = tex2Dlod(SpriteTextureSampler, float4(input.TextureCoordinates + offset, 0, 0));
            if (neighbor.a > 0.1) return ShadowColor;
        }
    }
    return float4(0, 0, 0, 0);
}

// --- 垂直扫描 ---
float4 VerticalPS(VertexShaderOutput input) : COLOR {
    float texelHeight = 1.0 / TextureSize.y;
    float fShadowSize = (float)ShadowSize;
    float step = texelHeight * CameraScale;

    [loop]
    for (int i = -16; i <= 16; i++) {
        if (abs((float)i) <= fShadowSize) {
            float2 offset = float2(0, i * step);
            float4 neighbor = tex2Dlod(SpriteTextureSampler, float4(input.TextureCoordinates + offset, 0, 0));
            if (neighbor.a > 0.1) return ShadowColor;
        }
    }
    return float4(0, 0, 0, 0);
}

technique Shadow {
    pass Horizontal { PixelShader = compile ps_3_0 HorizontalPS(); }
    pass Vertical   { PixelShader = compile ps_3_0 VerticalPS(); }
};