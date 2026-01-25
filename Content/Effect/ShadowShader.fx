sampler2D SdfTexture : register(s0);

float ShadowAmount;       // 阴影扩展量（像素）
float4 ShadowColor;     // 阴影颜色 (RGBA)
float2 TextureSize;     // 纹理尺寸

struct VertexShaderOutput {
    float4 Position : SV_Position;
    float2 TexCoord : TEXCOORD0;
};

float4 PixelShaderPS(VertexShaderOutput input) : SV_Target {

    // 采样 SDF（0.5 = 原始轮廓）
    float sdf = tex2D(SdfTexture, input.TexCoord).r;

    // 转为像素距离（外部为正）
    float dist = (sdf - 0.5) * ( ShadowAmount + 1 ) * 2.0;

    // ★ 关键：硬判断是否在阴影范围内
    if (dist > ShadowAmount) 
        discard; // ← 不在阴影内，什么都别画
       
    // ★ 在阴影范围内：纯色，不衰减
    return ShadowColor;
}

technique Shadow {
    pass Fully {
        PixelShader = compile ps_3_0 PixelShaderPS();
    }
};
