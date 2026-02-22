float2 ViewportSize;

float2 CameraWorldPos;
float GridUnit;
float4x4 MatrixTransform;


float AxisThickness;
float MajorThickness;
float MinorDotSize;

float4 AxisColor;
float4 MajorColor;
float4 MinorColor;

struct VSInput {
    float4 Position : POSITION0;
    float2 TexCoord : TEXCOORD0; // SpriteBatch 自动传出的纹理坐标 (0~1)
};

struct VSOutput {
    float4 Position : SV_Position;
    float2 ScreenPos : TEXCOORD0; 
};

VSOutput VS(VSInput input) {
    VSOutput o;
    // 强制把矩形铺满 NDC 空间 (-1 to 1)
    o.Position = mul(input.Position, MatrixTransform);
    
    // 使用 ViewportSize 将 0~1 的纹理坐标转回稳定的像素坐标
    o.ScreenPos = input.TexCoord * ViewportSize;
    return o;
}

float drawLine(float dist, float thickness) {
    return step(dist, thickness * 0.5);
}

float4 PS(VSOutput input) : SV_Target {

    float2 p = input.ScreenPos; // 稳定的屏幕像素坐标 (0,0) 是左上
    
        // 如果相机 Position 就是左上角：
        // 世界坐标 = (当前像素 / 缩放) + 相机左上角世界坐标
        float2 w = (p / GridUnit) + CameraWorldPos;
    
        // ===== 坐标轴绘制逻辑 =====
        // 由于坐标轴是无限细的线，我们需要根据 GridUnit 换算粗细
        float2 thickness = (AxisThickness / GridUnit);
        
        // 检查是否靠近世界坐标 0
        float axisX = step(abs(w.y), thickness.y * 0.5);
        float axisY = step(abs(w.x), thickness.x * 0.5);
    
        if (axisX > 0 || axisY > 0)
            return AxisColor;

    // ===== 5 单位主网格 =====
//    float2 major = abs(frac(w / 5.0) - 0.5);
//    float majorLine =
//         min(
//            step(major.x, MajorThickness / GridUnit * 0.5),
//            step(major.y, MajorThickness / GridUnit * 0.5)
//        );
//
//    if (majorLine > 0)
//        return MajorColor;

    // ===== 单位点 =====
//    float2 unit = abs(frac(w) - 0.5);
//    float dot =
//        step(max(unit.x, unit.y), MinorDotSize / GridUnit);

//    if (dot > 0)
//        return MinorColor;

    discard;
    return float4(0,0,0,0);
}


technique Grid {
    pass P0 {
        VertexShader = compile vs_3_0 VS();
        PixelShader  = compile ps_3_0 PS();
    }
}
