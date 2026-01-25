// using System;
// using System.Buffers;
// using Microsoft.Xna.Framework;
// using Microsoft.Xna.Framework.Graphics;
// using Cornifer.Renderers;
//
// namespace Cornifer.Helpers;
//
// /// <summary>
// /// 使用CPU烘焙阴影贴图的工具类
// /// Using CPU to bake shadow textures
// /// DEPRECATED: 该类已被弃用 正在ShadowShader取代 但是ShadowShader不完善 效果也不如这个
// /// </summary>
// public static class ShadowBaker {
//     private static RenderTarget2D? _shadeRenderTarget;
//
//     public static Texture2D GenerateShadow(GraphicsDevice device, Texture2D originalTex, int shadeAmount, int cornerRadius) {
//         // 1. 计算阴影贴图的大小
//         var shadeWidth = originalTex.Width + (shadeAmount * 2);
//         var shadeHeight = originalTex.Height + (shadeAmount * 2);
//
//         // 2. 准备/复用渲染目标
//         if (_shadeRenderTarget is null || _shadeRenderTarget.Width < shadeWidth ||
//             _shadeRenderTarget.Height < shadeHeight) {
//             _shadeRenderTarget?.Dispose();
//             _shadeRenderTarget = new RenderTarget2D(device, shadeWidth, shadeHeight, false, SurfaceFormat.Color,
//                 DepthFormat.None);
//         }
//
//         // 3. 将原图画入中心
//         device.SetRenderTarget(_shadeRenderTarget);
//         device.Clear(Color.Transparent);
//
//
//         using (var sb = new SpriteBatch(device)) {
//             sb.Begin();
//             // 将原图偏移 shadeAmount 绘制，这样四周才有空间生成阴影
//             sb.Draw(originalTex, new Vector2(shadeAmount, shadeAmount), Color.White);
//             sb.End();
//         }
//
//         device.SetRenderTarget(null);
//
//         // 4. 读取像素并处理
//         var totalPixels = shadeWidth * shadeHeight;
//         var pixels = ArrayPool<Color>.Shared.Rent(totalPixels);
//         _shadeRenderTarget.GetData(0, new Rectangle(0, 0, shadeWidth, shadeHeight), pixels, 0, totalPixels);
//
//         // 调用你原有的算法逻辑
//         ProcessShadow(pixels, shadeWidth, shadeHeight, shadeAmount, cornerRadius);
//
//         // 5. 生成最终阴影贴图
//         var resultTex = new Texture2D(device, shadeWidth, shadeHeight);
//         resultTex.SetData(pixels, 0, totalPixels);
//
//         ArrayPool<Color>.Shared.Return(pixels);
//         return resultTex;
//     }
//
//     public static Shadow Bake(GraphicsDevice device, Texture2D originalTex, int shadeAmount, int cornerRadius) {
//         var shadeTex = GenerateShadow(device, originalTex, shadeAmount, cornerRadius);
//         return new Shadow {
//             Texture = shadeTex,
//             Offset = new Vector2(shadeAmount, shadeAmount),
//             Amount = shadeAmount,
//             CornerRadius = cornerRadius
//         };
//     }
//
//     /// <summary>
//     /// 生成阴影贴图的核心算法
//     /// </summary>
//     /// <param name="colors"></param>
//     /// <param name="width"></param>
//     /// <param name="height"></param>
//     /// <param name="size"></param>
//     /// <param name="cornerRadius"></param>
//     private static void ProcessShadow(Color[] colors, int width, int height, int size, int? cornerRadius) {
//         var length = width * height;
//         var shadow = ArrayPool<bool>.Shared.Rent(length);
//
//         var patternSide = size * 2 + 1;
//
//         bool[] shadowPattern = null!;
//
//         if (cornerRadius.HasValue) {
//             shadowPattern = ArrayPool<bool>.Shared.Rent(patternSide * patternSide);
//
//             var patternRadSq = cornerRadius.Value * cornerRadius.Value;
//
//             for (var j = 0; j < patternSide; j++)
//             for (var i = 0; i < patternSide; i++) {
//                 float lengthsq = (size - i) * (size - i) + (size - j) * (size - j);
//                 shadowPattern[i + patternSide * j] = lengthsq <= patternRadSq;
//             }
//         }
//
//         for (var j = 0; j < height; j++)
//         for (var i = 0; i < width; i++) {
//             var index = width * j + i;
//
//             shadow[index] = false;
//
//             if (colors[index].A > 0) {
//                 shadow[index] = true;
//                 continue;
//             }
//
//             if (size <= 0)
//                 continue;
//
//             var probing = true;
//             for (var l = -size; l <= size && probing; l++)
//             for (var k = -size; k <= size && probing; k++) {
//                 if (cornerRadius.HasValue) {
//                     var patternIndex = (l + size) * patternSide + k + size;
//                     if (!shadowPattern[patternIndex])
//                         continue;
//                 }
//
//                 var x = i + k;
//                 var y = j + l;
//
//                 if (x < 0 || y < 0 || x >= width || y >= height || k == 0 && l == 0)
//                     continue;
//
//                 var testIndex = width * y + x;
//
//                 if (colors[testIndex].A <= 0) continue;
//                 shadow[index] = true;
//                 probing = false;
//             }
//         }
//
//         for (var i = 0; i < length; i++)
//             colors[i] = shadow[i] ? Color.Black : Color.Transparent;
//
//         ArrayPool<bool>.Shared.Return(shadow);
//         if (cornerRadius.HasValue)
//             ArrayPool<bool>.Shared.Return(shadowPattern);
//     }
// }