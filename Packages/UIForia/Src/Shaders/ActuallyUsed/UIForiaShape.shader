Shader "UIForia/UIForiaShape"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { 
           
        }
        
        LOD 100

        Pass {
        
            Cull Off // configurable is probably the best approach for this
            ColorMask [_UIForiaColorMask]
            Stencil {
                Ref [_UIForiaStencilRef]
                Comp [_UIForiaStencilComp]
                Pass [_UIForiaStencilOp] 
            }

            Blend SrcAlpha OneMinusSrcAlpha // todo -- consider unifying the blend mode with text so we have less state change on shader switch
            //Blend One OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            #pragma enable_d3d11_debug_symbols
            #include "UnityCG.cginc"
            #include "UIForia.cginc"
           
            struct appdata {
                uint vid : SV_VertexID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texCoord0 : TEXCOORD0;
                float2 texCoord1 : TEXCOORD1;
                nointerpolation uint4 indices : TEXCOORD2;
                nointerpolation float2 size : TEXCOORD3;
                nointerpolation half4 uvTransform : TEXCOORD4;
                nointerpolation half4 border : TEXCOORD5;
                nointerpolation fixed4 borderColorH : COLOR0;
                nointerpolation fixed4 borderColorV : COLOR1;
            };

            // layout must EXACTLY match ElementMaterialInfo in C#
            struct ElementMaterialInfo {
                
                uint backgroundColor;
                uint backgroundTint;
                uint outlineColor;
                uint outlineTint; // not used atm 

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
                
                uint unused2;
            };
        
            struct UIForiaVertex {
                float2 position;
                float2 size;     
                int4 indices;    
            };
            
            sampler2D _MainTex;
            sampler2D _OutlineTex;
            
            float4 _MainTex_TexelSize; 
            
            float _SpriteAtlasPadding;
            float _UIForiaDPIScale;
            float4x4 _UIForiaOriginMatrix;
            
            struct GradientInfo {
                uint flags;
                uint colorCount;
                uint alphaCount;
                uint unused_padding;
                float4 colors[8];
                float2 alphas[8];
            };
                        
            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;            
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;            
            StructuredBuffer<ElementMaterialInfo> _UIForiaMaterialBuffer;            
            StructuredBuffer<float4> _UIForiaFloat4Buffer;
            StructuredBuffer<GradientInfo> _UIForiaGradientBuffer;
            
            // todo -- byte isnt quite enough precision for this, convert to ushort if possible
            inline float UnpackCornerBevel(uint bevelTop, uint bevelBottom, float2 texCoord) {

                float left = step(texCoord.x, 0.5); // 1 if left
                float bottom = step(1 - texCoord.y, 0.5); // 1 if bottom

                #define top (1 - bottom)
                #define right (1 - left) 

                uint bevelAmount = 0;
                bevelAmount += (top * left) * UnpackHighBytes(bevelTop);
                bevelAmount += (top * right) * UnpackLowBytes(bevelTop);
                bevelAmount += (bottom * right) * UnpackLowBytes(bevelBottom);
                bevelAmount += (bottom * left) * UnpackHighBytes(bevelBottom);

                return  (float)bevelAmount;
                
                #undef top
                #undef right
            }
            
            v2f vert (appdata v) {
               v2f o;
               int vertexId = v.vid & 0xffffff; // 3 bytes
               int cornerIdx = (v.vid >> 24) & (1 << 24) - 1; // 1 byte, 2 bits for corner idx, 6 bits free
                
               UIForiaVertex vertex = _UIForiaVertexBuffer[vertexId];
                
               float3 vpos = float3(vertex.position.xy, 0); // positioned at center
               float2 halfSize = vertex.size * 0.5;

               if(cornerIdx == TOP_LEFT) {
                   vpos.x -= halfSize.x;
                   vpos.y -= halfSize.y;
                   o.texCoord0 = float2(0, 1); 
                   o.texCoord1 = float2(0, 0);
               }
               else if(cornerIdx == TOP_RIGHT) {
                   vpos.x += halfSize.x;
                   vpos.y -= halfSize.y;
                   o.texCoord0 = float2(1, 1);
                   o.texCoord1 = float2(1, 0);
               }
               else if(cornerIdx == BOTTOM_RIGHT) {
                   vpos.x += halfSize.x;
                   vpos.y += halfSize.y;
                   o.texCoord0 = float2(1, 0);
                   o.texCoord1 = float2(1, 1);
               }
               else { // BOTTOM_LEFT
                   vpos.x -= halfSize.x;
                   vpos.y += halfSize.y;
                   o.texCoord0 = float2(0, 0);
                   o.texCoord1 = float2(0, 1);
               }
               
                  // sdf uv offsets for non quad meshes, which we'll probably want eventually to reduce overdraw
                float2 halfUV = float2(0.5, 0.5) / vertex.size; // todo -- this might need to be dpi scaled
                o.texCoord0.x += o.texCoord0.x > 0.5 ? halfUV.x : -halfUV.x;
                o.texCoord0.y += o.texCoord0.y > 0.5 ? halfUV.y : -halfUV.y;
               
               int matrixIndex = UnpackMatrixId(vertex.indices);
               int materialIndex = UnpackMaterialId(vertex.indices);
               ElementMaterialInfo material = _UIForiaMaterialBuffer[materialIndex];

               o.uvTransform = material.uvTransformIdx != 0 ? _UIForiaFloat4Buffer[material.uvTransformIdx] : float4(1, 1, 0, 0);
                
               o.size = vertex.size;
               
               float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
               o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos.xyz, 1.0)));

               // todo -- snapping is terrible when moving/ rotating 
               // o.vertex = UIForiaPixelSnap(o.vertex);
                
               o.indices = uint4(UnpackClipRectId(vertex.indices.x), vertex.indices.y, vertex.indices.z, vertex.indices.w);
               o.borderColorH = lerp(RED, BLUE, o.texCoord1.x < 0.5);
               o.borderColorV = lerp(GREEN, BLACK, o.texCoord1.y > 0.5);
               o.border = half4(20, 20, 20, 0);
               
             //  if(o.border.w == 0 && o.texCoord0.x > 0.5) o.borderColorH = o.borderColorV; //lerp(RED, BLUE, o.texCoord0.x > 0.5);
               
               return o;
            }
            
            inline half DistToLine(half2 pt1, half2 pt2, half2 testPt) {
              half2 lineDir = pt2 - pt1;
              half2 perpDir = half2(lineDir.y, -lineDir.x);
              half2 dirToPt1 = pt1 - testPt;
              return abs(dot(normalize(perpDir), dirToPt1));
            }
            
            fixed4 frag (v2f i) : SV_Target {
                // this could be done in the vertex shader too but this way we can support correct
                
                ElementMaterialInfo material = _UIForiaMaterialBuffer[i.indices.y];
                float2 size = i.size;
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
                float fillRotation = UnpackHighUShortPercentageToFloat(material.fillOpenAndRotation); //frac(_Time.y) * PI * 2;
                float fillRadius = material.fillRadius;
                float2 fillOffset = i.size * float2(0.5, 0.5); //float2(material.fillOffsetX, material.fillOffsetY); // - halfUV;
                 
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
                
                //float4 border = float4(10, 20, 30, 40); // t,r,b,l
                // fixed4 borderColor = ComputeBorderColor(border, originalUV);
                
                // todo -- figure out 9 slicing, i guess it must be different quads or a fully different vertex shader with uv set somehow
                // similar to how we read effect data for text
                
                #define borderTop i.border.x
                #define borderLeft i.border.w
                #define borderBottom i.border.z
                #define borderRight i.border.y
                
                #define borderColorRight BLACK
                #define borderColorLeft GREEN
                #define borderColorTop BLUE
                #define borderColorBottom fixed4(1, 1, 0, 1)
                
                float left = step(originalUV.x, 0.5); // 1 if left
                float bottom = step(1 - originalUV.y, 0.5); // 1 if bottom
                
                #define top (1 - bottom)
                #define right (1 - left)  
                
                half borderH = lerp(borderLeft, borderRight, left);
                half borderV = lerp(borderTop, borderBottom, top);
                
                half2 corner = half2(lerp(0, size.x, right), lerp(0, size.y, bottom));
                half2 inset = half2(lerp(borderH, size.x - borderH, right), lerp(borderV, size.y - borderV, bottom));
                return i.borderColorV;
                half2 p = originalUV * size;
                
                // equasion of a line, take the sign to determine if a point is above or below the line
                half lineEq = (inset.x - corner.x) * (p.y - corner.y) - (inset.y - corner.y) * (p.x - corner.x);
                half distToLine = DistToLine(corner, inset, p);
                fixed sideOfLine = ((left * top || right * bottom) ? -1 : 1) * sign(lineEq);
            
                distToLine = Remap(saturate(distToLine), -1, 1, 0, 1);
                
                if(sideOfLine < 0) {
                    distToLine = 1 - distToLine;
                }
                
                fixed4 borderColor = lerp(i.borderColorH, i.borderColorV, distToLine);
                //color = lerp(borderColorH, borderColorV, distToLine);
                
                float2 borderStep = step(float2(borderLeft, size.y - borderTop), p) - step(float2(size.x - borderRight, borderBottom), p);
                
                color = borderColor;
                
                if(borderStep.x * borderStep.y != 0) {
                 //   color = fixed4(0, 0, 0, 0); //RED;
                    color = fixed4(1, 1, 0, 1);
                }
               // color = fixed4(1, 1, 0, 1);
               // color = fixed4(0, 0, 0, 0); //RED;
                outlineColor = borderColor;
                
                half outlineWidth = material.outlineWidth * 0.5;
                half4 radius = UnpackRadius(packedRadii, minSize);
                
                float t = (fillAmount * 180) * Deg2Rad;
                float v = t + (fillRotation * (2 * PI)) * fillDirection;
                
                float2 angleSinCos = float2(sin(t), cos(t));
                float2 samplePoint = (i.texCoord0.xy - 0.5) * size;
                float2 radialSamplePoint = RotateUV((i.texCoord0.xy * size) - fillOffset, -v * fillDirection);

                fixed4 colors[8] = {
                     fixed4(0.529, 0.227, 0.706, 0),
                     fixed4(GREEN.rgb, 0.25),
                     fixed4(0.992, 0.114, 0.114, 0.5),
                     fixed4(0.988, 0.690, 0.271, 0.75),
                     fixed4(PINK.rgb, 1),
                     
                     fixed4(1, 0, 0, 0.6),
                     fixed4(1, 1, 0, 0.7),
                     fixed4(1, 0, 0, 0.8),
                 };
                 
                fixed2 alphas[8] = {
                    fixed2(1, 0),
                    fixed2(1, 0.5),
                    fixed2(1, 1),
                    
                    fixed2(1, 1),
                    fixed2(1, 1),
                    fixed2(1, 1),
                    fixed2(1, 1),
                    fixed2(1, 1),
                    
                 };
 
                fixed4 grad = WHITE;
                
                float gradientScale = 1;
                float gradRotation = 0 * Deg2Rad; // * Deg2Rad;
                float2 gradientOffset = float2(0, 0);
                int wrapGradient = 1;
                int hardBlend = 0;
                int gradientType = GradientType_Linear; //Conical;
                
                float2 gradientTexCoord = gradientOffset + i.texCoord0.xy;
                half gradientTime = LinearGradient(gradientTexCoord, gradRotation);
                gradientTime = lerp(gradientTime, RadialGradient(gradientTexCoord), gradientType == GradientType_Radial);
                gradientTime = lerp(gradientTime, ConicalGradient(RotateUV(gradientTexCoord, gradRotation, float2(0.5, 0.5))), gradientType == GradientType_Conical);
                
                gradientTime *= gradientScale;
                
               // return (tex2Dlod(_MainTex, float4(frac(gradientTime), 0, 0, 0)));
                grad = SampleGradient(lerp(gradientTime, frac(gradientTime), wrapGradient), colors, alphas, 5, 3, hardBlend);
                grad = lerp(grad, SampleCornerGradient(gradientTexCoord, colors, alphas), 0);
                
                float sdf = sdRoundBox(samplePoint, size * 0.5, radius);

                float radialSDF = sdPie(radialSamplePoint, angleSinCos, fillRadius);

                float bevelAmount = UnpackCornerBevel(material.bevelTop, material.bevelBottom, i.texCoord0);
                
                float2 bevelOffset = float2(size.x * 0.5 * (i.texCoord0.x > 0.5 ? 1 : -1), size.y * 0.5 * (i.texCoord0. y > 0.5 ? 1 : -1));
                float sdfBevel = sdRect(RotateUV(samplePoint - bevelOffset, 45 * Deg2Rad), float2(bevelAmount, bevelAmount));
                
                sdf = max(-sdfBevel, sdf);
                // todo -- move max up here to draw pie instead of using as a clipping bounds
                // sdf = max(radialSDF * invertFill, sdf);
                
                float sdfOutline = outlineWidth > 0 ? abs(sdf) - outlineWidth : 0;
                //sdf = max(radialSDF * invertFill, sdf);
               // sdfOutline = (fillFlag & FillOutline) != 0 ? max(radialSDF * invertFill, sdfOutline) : sdfOutline;
                
                color = ComputeColor(color, grad, tintColor, bodyColorMode, i.texCoord1, _MainTex, uvBounds, originalUV);
                color = lerp(color, outlineColor, outlineWidth == 0 ? 0 : 1 - saturate(sdfOutline));
                color.a *= 1 - smoothstep(0, fwidth(sdf), sdf);
                
                float2 clipPos = float2(i.vertex.x, _ProjectionParams.x > 0 ? i.vertex.y : _ScreenParams.y - i.vertex.y); //* _UIForiaDPIScale;
                float4 clipRect = _UIForiaFloat4Buffer[i.indices.x]; // x = xMin, y = yMin, z = xMax, w = yMax
                float2 s = step(clipRect.xw, clipPos) - step(clipRect.zy, clipPos);
                
               // float grayscale = dot(color.rgb, float3(0.3, 0.59, 0.11));
               // fixed3 gradientCol = fixed3(grayscale * grad.rgb);  //tex2D(_GradientMap, float2(grayscale, 0));
               // return UIForiaColorSpace(fixed4(grad.rgb + (0.25 * color.rgb), color.a)); //gradientCol * c.a * IN.color;
                color.a *= opacity;
                color.a *= (s.x * s.y) != 0;
                color = UIForiaColorSpace(color); // todo -- video texture wont want to adjust color space 
                clip(color.a - 0.01);
                return color;
                
            }
            
           ENDCG
        }
    }
}
