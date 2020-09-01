Shader "UIForia/UIForiaShape" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        // _Sigma ("Sigma", Range(0, 1)) = 0
    }

    SubShader {

        Tags {
        }

        LOD 100

        Pass {

            Cull Off
            ColorMask [_UIForiaColorMask]
            Stencil {
                Ref [_UIForiaStencilRef]
                Comp [_UIForiaStencilComp]
                Pass [_UIForiaStencilOp]
            }

            Blend [_UIForiaSrcMode] [_UIForiaDstBlendMode] // 
            Blend SrcAlpha OneMinusSrcAlpha // todo -- consider unifying the blend mode with text so we have less state change on shader switch
            CGPROGRAM
#pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #include "UnityCG.cginc"
            #include "UIForia.cginc"
            #pragma require 2darray

            struct appdata
            {
                uint vid : SV_VertexID;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 texCoord0 : TEXCOORD0;
                float2 texCoord1 : TEXCOORD1;
                nointerpolation uint4 indices : TEXCOORD2;
                nointerpolation float2 size : TEXCOORD3;
                nointerpolation half4 uvTransform : TEXCOORD4;
                nointerpolation half4 border : TEXCOORD5;
            };

            // layout must EXACTLY match ElementMaterialInfo in C#
            struct ElementMaterialInfo
            {
                uint backgroundColor;
                uint backgroundTint;
                uint outlineColor;
                uint outlineTint; // not used atm

                uint borderColorTop;
                uint borderColorRight;
                uint borderColorBottom;
                uint borderColorLeft;

                uint radius;
                uint bevelTop;
                uint bevelBottom;
                uint fillOpenAndRotation;
                float fillRadius;

                float fillOffsetX;
                float fillOffsetY;

                uint bMode_oMode_meshFillDirection_meshFillInvert;

                float outlineWidth;
                uint uvTransformIdx;
                uint uvRotation_Opacity;

                uint borderIndex;

                // todo -- try to get rid of some of this data 
                uint maskFlags;
                float maskSoftness;
                uint maskTopLeftUV;
                uint maskBottomRightUV;
            };

            struct UIForiaVertex
            {
                float2 position;
                float2 size;
                int4 indices;
            };

            sampler2D _MainTex;
            sampler2D _OutlineTex;
            sampler2D _MaskTexture;

            float4 _MainTex_TexelSize;

            float _UIForiaDPIScale;
            float4x4 _UIForiaOriginMatrix;
            float _MaskSoftness;

            // float _Sigma;
            
            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;
            StructuredBuffer<ElementMaterialInfo> _UIForiaMaterialBuffer;
            StructuredBuffer<float4> _UIForiaFloat4Buffer;

            float sdSegment(in float2 p, in float2 a, in float2 b)
            {
                float2 pa = p - a, ba = b - a;
                float h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
                return length(pa - ba * h);
            }

            v2f vert(appdata v)
            {
                v2f o;
                int vertexId = v.vid & 0xffffff; // 3 bytes
                int cornerIdx = (v.vid >> 24) & (1 << 24) - 1; // 1 byte, 2 bits for corner idx, 6 bits free

                UIForiaVertex vertex = _UIForiaVertexBuffer[vertexId];
                int matrixIndex = UnpackMatrixId(vertex.indices);
                int materialIndex = UnpackMaterialId(vertex.indices);
                ElementMaterialInfo material = _UIForiaMaterialBuffer[materialIndex];

                float3 vpos = float3(vertex.position.xy, 0); // positioned at center

                // vertex.size.x += _WidthAdd;
                // vertex.size.y += _HeightAdd;
                // size *= 1/(ratio * 0.5);
                // float ratio = _Outer / 150.0;
              // vertex.size.x += (vertex.size.x * (_Sigma));
              // vertex.size.y += (vertex.size.y * (_Sigma));

                float2 halfSize = vertex.size * 0.5;
                bool is9SliceBorder = (GetByteN(vertex.indices.y, 3) & ElementShaderFlags_IsNineSliceBorder) != 0;
                bool is9SliceContent = (GetByteN(vertex.indices.y, 3) & ElementShaderFlags_IsNineSliceContent) != 0;

                float4 borderInfo = _UIForiaFloat4Buffer[material.borderIndex];
                o.border = material.borderIndex != 0 ? _UIForiaFloat4Buffer[material.borderIndex] : float4(0, 0, 0, 0);

                float4 mainSize = float4(0, 0, vertex.size.x, vertex.size.y);
                if (is9SliceBorder)
                {
                    material.uvTransformIdx = 0;
                    o.border = float4(0, 0, 0, 0);
                    mainSize = borderInfo; // overloaded border index here
                }

                if (is9SliceContent)
                {
                    o.border = float4(0, 0, 0, 0);
                    mainSize = borderInfo;
                }

                if (cornerIdx == TOP_LEFT)
                {
                    vpos.x -= halfSize.x;
                    vpos.y -= halfSize.y;
                    o.texCoord0 = float2(0, 1);
                    o.texCoord1 = float2(0, 0);
                }
                else if (cornerIdx == TOP_RIGHT)
                {
                    vpos.x += halfSize.x;
                    vpos.y -= halfSize.y;
                    o.texCoord0 = float2(1, 1);
                    o.texCoord1 = float2(1, 0);
                }
                else if (cornerIdx == BOTTOM_RIGHT)
                {
                    vpos.x += halfSize.x;
                    vpos.y += halfSize.y;
                    o.texCoord0 = float2(1, 0);
                    o.texCoord1 = float2(1, 1);
                }
                else
                {
                    // BOTTOM_LEFT
                    vpos.x -= halfSize.x;
                    vpos.y += halfSize.y;
                    o.texCoord0 = float2(0, 0);
                    o.texCoord1 = float2(0, 1);
                }

                o.size = vertex.size;

                if (is9SliceBorder || is9SliceContent)
                {
                    o.texCoord0.x = vpos.x / mainSize.z;
                    o.texCoord0.y = -vpos.y / mainSize.w;
                    o.size = mainSize.zw;
                }

                // sdf uv offsets for non quad meshes, which we'll probably want eventually to reduce overdraw
                float2 halfUV = float2(0.5, 0.5) / mainSize.zw; // todo -- this might need to be dpi scaled
                o.texCoord0.x += o.texCoord0.x > 0.5 ? halfUV.x : -halfUV.x;
                o.texCoord0.y += o.texCoord0.y > 0.5 ? halfUV.y : -halfUV.y;

                o.uvTransform = material.uvTransformIdx != 0
                                    ? _UIForiaFloat4Buffer[material.uvTransformIdx]
                                    : float4(1, 1, 0, 0);

                float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
                o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos.xyz, 1.0)));

                // todo -- snapping is terrible when moving/ rotating 
                o.vertex = UIForiaPixelSnap(o.vertex);

                o.indices = uint4(UnpackClipRectId(vertex.indices.x), vertex.indices.y, vertex.indices.z, vertex.indices.w);

                return o;
            }

           
            float4 ApplyUIForiaMask(uint maskFlags, float4 color, float2 uv, sampler2D maskTexture, float maskSoftness)
            {
                if ((maskFlags & MaskFlags_UseTextureMask) != 0)
                {
                    // todo -- use maskUVs
                    float maskAlpha = (saturate(tex2Dlod(maskTexture, float4(uv, 0, 0)) / maskSoftness));
                    // todo -- mask softness & mask uvs
                    if ((maskFlags & MaskFlags_UseTextureMaskInverted) != 0)
                    {
                        maskAlpha = 1 - maskAlpha;
                    }
                    color.a = saturate(color.a * maskAlpha);
                }
                return color;
            }

            float4 frag(v2f i) : SV_Target
            {
                const float shadowScale = 1;
                // this could be done in the vertex shader too but this way we can support correct
                ElementMaterialInfo material = _UIForiaMaterialBuffer[i.indices.y & 0xffffff];
                float2 size = i.size;

                // float sigma = _Sigma/2; 
                // float2 p0Rect = float2(sigma, sigma);
                // float2 p1Rect = float2(1-sigma, 1-sigma);
                // float2 radii = float2(0, 0);
                // float value = color(i.texCoord0, p0Rect, p1Rect, radii, sigma);
                // return float4(BLACK.rgb, max(value, 0.0));

                float minSize = min(size.x, size.y);
                fixed4 color = UnpackColor(material.backgroundColor);
                fixed4 outlineColor = UnpackColor(material.outlineColor);
                fixed4 tintColor = UnpackColor(material.backgroundTint);
                float opacity = UnpackHighUShortPercentageToFloat(material.uvRotation_Opacity);
                uint packedRadii = material.radius;
                uint bodyColorMode = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 0);

                // todo -- put these in a constant buffer or add to material

                // can be a sign bit or flag elsewhere
                float fillDirection = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 2) == 0 ? 1 : -1;
                float invertFill = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 3) == 0 ? 1 : -1;
                float fillAmount = UnpackLowUShortPercentageToFloat(material.fillOpenAndRotation);
                float fillRotation = UnpackHighUShortPercentageToFloat(material.fillOpenAndRotation);
                //frac(_Time.y) * PI * 2;
                float fillRadius = material.fillRadius;
                float2 fillOffset = float2(material.fillOffsetX, material.fillOffsetY); // - halfUV;
                float2 uvScale = i.uvTransform.xy;
                float2 uvOffset = i.uvTransform.zw;
                float uvRotation = UnpackLowUShortPercentageToFloat(material.uvRotation_Opacity) * 2 * PI;

                float4 uvBounds = float4(
                    UnpackLowBytes(i.indices.z) * _MainTex_TexelSize.x,
                    UnpackHighBytes(i.indices.z) * _MainTex_TexelSize.y,
                    UnpackLowBytes(i.indices.w) * _MainTex_TexelSize.x,
                    UnpackHighBytes(i.indices.w) * _MainTex_TexelSize.y
                );

                float2 originalUV = i.texCoord1;
                i.texCoord1 = TransformUV(i.texCoord1, uvOffset * _MainTex_TexelSize.xy, uvScale, uvRotation, uvBounds);

                #define borderTop i.border.x
                #define borderLeft i.border.w
                #define borderBottom i.border.z
                #define borderRight i.border.y

                float left = step(originalUV.x, 0.5); // 1 if left
                float top = step(1 - originalUV.y, 0.5); // 1 if bottom

                #define bottom (1 - top)
                #define right (1 - left)

                half borderH = lerp(borderLeft, borderRight, right);
                half borderV = lerp(borderTop, borderBottom, bottom);

                half4 borderColorV = UnpackColor(top == 1 ? material.borderColorTop : material.borderColorBottom);
                half4 borderColorH = UnpackColor(left == 1 ? material.borderColorLeft : material.borderColorRight);

                half2 corner = half2(lerp(0, size.x, right), lerp(0, size.y, top));
                half2 inset = half2(lerp(borderH, size.x - borderH, right), lerp(borderV, size.y - borderV, top));
                half2 p = originalUV * size;

                // equation of a line, take the sign to determine if a point is above or below the line
                half lineEq = (inset.x - corner.x) * (p.y - corner.y) - (inset.y - corner.y) * (p.x - corner.x);
                half distToLine = DistToLine(corner, inset, p);
                fixed sideOfLine = ((left * bottom || right * top) ? -1 : 1) * sign(lineEq);

                distToLine = Remap(saturate(distToLine), -1, 1, 0, 1);

                if (sideOfLine < 0)
                {
                    distToLine = 1 - distToLine;
                }

                fixed4 borderColor = lerp(borderColorH, borderColorV, distToLine);
                float2 borderStep = step(float2(borderLeft, size.y - borderTop), p) - step(float2(size.x - borderRight, borderBottom), p);
                fixed4 contentColor = color;
                color = borderColor;
                outlineColor = borderColor.a > 0 ? borderColor : outlineColor;
                if (borderStep.x * borderStep.y != 0)
                {
                    color = contentColor;
                }

                half outlineWidth = material.outlineWidth * 0.5;
                float2 samplePoint = (i.texCoord0.xy - 0.5) * size;

                half radius = UnpackRadius(samplePoint, packedRadii, minSize);

                float t = (fillAmount * 180) * Deg2Rad;
                float v = t + (fillRotation * (2 * PI)) * fillDirection;

                float2 angleSinCos = float2(sin(t), cos(t));
                float2 radialSamplePoint = RotateUV((i.texCoord0.xy * size) - fillOffset, -v * fillDirection);

                float bevelAmount = UnpackCornerBevel(material.bevelTop, material.bevelBottom, i.texCoord0);
                float radialSDF = sdPie(radialSamplePoint, angleSinCos, fillRadius);

                float2 bevelOffset = float2(size.x * 0.5 * (i.texCoord0.x > 0.5 ? 1 : -1),
                                            size.y * 0.5 * (i.texCoord0.y > 0.5 ? 1 : -1));
                float2 bevelPoint = ((i.texCoord0 - 0.5) * size) - bevelOffset;
                float sdfBevel = sdRect(RotateUV(bevelPoint, 45 * Deg2Rad), float2(bevelAmount, bevelAmount));
                float sdf = sdRoundBox(samplePoint, size * 0.5 * shadowScale, radius * shadowScale);
                sdf = max(-sdfBevel, sdf);

                // todo -- move max up here to draw pie instead of using as a clipping bounds
                //sdf = max(radialSDF * invertFill, sdf);
                float sdfOutline = outlineWidth > 0 ? abs(sdf) - outlineWidth : 0;

                // sdf = max(radialSDF * invertFill, sdf);
                 
                // sdfOutline = (fillFlag & FillOutline) != 0 ? max(radialSDF * invertFill, sdfOutline) : sdfOutline;
                fixed4 grad = WHITE;
                color = ComputeColor(color, grad, tintColor, bodyColorMode, i.texCoord1, _MainTex, uvBounds, originalUV);
                color = lerp(color, outlineColor, outlineWidth == 0 ? 0 : 1 - saturate(sdfOutline));
                color.a *= 1.0 - smoothstep(0, fwidth(sdf), sdf);

                //float shadow = minSize * 0.5;
                //color.a *= 1.0 - smoothstep(0, lerp(_Inner, fwidth(sdf), 0), sdf);

                float2 clipPos = float2(i.vertex.x, _ProjectionParams.x > 0 ? i.vertex.y : _ScreenParams.y - i.vertex.y); //* _UIForiaDPIScale;
                float4 clipRect = _UIForiaFloat4Buffer[i.indices.x]; // x = xMin, y = yMin, z = xMax, w = yMax
                float2 s = step(clipRect.xw, clipPos) - step(clipRect.zy, clipPos);

                // todo -- support color toning filters on images, or do it in a pre pass maybe
                // float grayscale = dot(color.rgb, float3(0.3, 0.59, 0.11));
                // fixed3 gradientCol = fixed3(grayscale * grad.rgb);  //tex2D(_GradientMap, float2(grayscale, 0));
                // return UIForiaColorSpace(fixed4(grad.rgb + (0.25 * color.rgb), color.a)); //gradientCol * c.a * IN.color;

                color = ApplyUIForiaMask(material.maskFlags, color, originalUV, _MaskTexture, material.maskSoftness);

                color.a *= opacity;
                color.a *= (s.x * s.y) != 0;
                color = lerp(color, UIForiaColorSpace(color), (bodyColorMode & ColorMode_ApplyColorSpaceCorrection) != 0);
                clip(color.a - 0.01);

                // #ifndef UNITY_COLORSPACE_GAMMA
                // color.rgb = LinearToGammaSpace(color.rgb);
                // #endif
                // // apply intensity exposure
                // color.rgb *= pow(2.0, 0.5);
                // // if not using gamma color space, convert back to linear
                // #ifndef UNITY_COLORSPACE_GAMMA
                // color.rgb = GammaToLinearSpace(color.rgb);
                // #endif
                return color;
            }
            ENDCG
        }
    }
}
//
//  float ratio = _Outer / 100;
//                float halfRatio = ratio * 0.125; //(1/(1 +ratio));//0.4;
//                float start = float2(halfRatio, halfRatio);
//                float end = float2(1 - halfRatio, 1 - halfRatio);
//                size *= 1 / (ratio * 0.125);
//                // float alpha = roundedBoxShadow(float2(ratio * 0.5, ratio * 0.5) , float2(1-ratio* 0.5, 1- ratio * 0.5) , originalUV , ratio, 0);
//                float alpha = roundedBoxShadow(start * size, end * size, i.texCoord0.xy * size, _Outer, _Inner);
//                // alpha = lerp(alpha, 1.0 - smoothstep(0, fwidth(sdf), sdf), _Outer <= 0.25);
//                return UIForiaColorSpace(lerp(fixed4(color.rgb, alpha), RED,0));
//fixed4 colors[8] = {
//     fixed4(0.529, 0.227, 0.706, 0),
//     fixed4(GREEN.rgb, 0.25),
//     fixed4(0.992, 0.114, 0.114, 0.5),
//     fixed4(0.988, 0.690, 0.271, 0.75),
//     fixed4(PINK.rgb, 1),
//     
//     fixed4(1, 0, 0, 0.6),
//     fixed4(1, 1, 0, 0.7),
//     fixed4(1, 0, 0, 0.8),
// };
// 
//fixed2 alphas[8] = {
//    fixed2(1, 0),
//    fixed2(1, 0.5),
//    fixed2(1, 1),
//    
//    fixed2(1, 1),
//    fixed2(1, 1),
//    fixed2(1, 1),
//    fixed2(1, 1),
//    fixed2(1, 1),
//    
// };
//
//fixed4 grad = WHITE;
//
//float gradientScale = 1;
//float gradRotation = 0 * Deg2Rad; // * Deg2Rad;
//float2 gradientOffset = float2(0, 0);
//int wrapGradient = 1;
//int hardBlend = 0;
//int gradientType = GradientType_Linear; //Conical;
//
//float2 gradientTexCoord = gradientOffset + i.texCoord0.xy;
//half gradientTime = LinearGradient(gradientTexCoord, gradRotation);
//gradientTime = lerp(gradientTime, RadialGradient(gradientTexCoord), gradientType == GradientType_Radial);
//gradientTime = lerp(gradientTime, ConicalGradient(RotateUV(gradientTexCoord, gradRotation, float2(0.5, 0.5))), gradientType == GradientType_Conical);
//
//gradientTime *= gradientScale;
//
//// return (tex2Dlod(_MainTex, float4(frac(gradientTime), 0, 0, 0)));
//grad = SampleGradient(lerp(gradientTime, frac(gradientTime), wrapGradient), colors, alphas, 5, 3, hardBlend);
//grad = lerp(grad, SampleCornerGradient(gradientTexCoord, colors, alphas), 0);