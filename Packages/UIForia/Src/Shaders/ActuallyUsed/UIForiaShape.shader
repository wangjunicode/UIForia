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

            Blend SrcAlpha OneMinusSrcAlpha
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
                uint4 indices : TEXCOORD2;
                float2 size : TEXCOORD3;
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
                uint meshPie;
                uint meshPieOffset; // cannot use half2 because its not 2 bytes, its 4 on most platforms, need to unpack

                uint bMode_oMode_unused;
                
                // maybe move to float buffer
              
                
                float outlineWidth;
                uint unused;
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
                        
            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;            
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;            
            StructuredBuffer<ElementMaterialInfo> _UIForiaMaterialBuffer;            
            StructuredBuffer<float4> _UIForiaFloat4Buffer;
            
            float4 SampleGradient(float sampleValue, fixed4 colors[8], fixed4 alphas[8]) {
                float3 color = colors[0].rgb;
                float alpha = alphas[0].x;
                int fixedOrBlend = 0;
                int gradientCount = 8;
                sampleValue *= 0.5 + .01;
                [unroll]
                for (int c = 1; c < 8; c ++) {
                    float prevTimeKey = colors[c - 1].w;
                    float colorPos = saturate((sampleValue - prevTimeKey) / (colors[c].w - prevTimeKey)) * step(c, gradientCount - 1);
                    color = lerp(color, colors[c].rgb, lerp(colorPos, step(0.01, colorPos), fixedOrBlend));
                }
                
                // [unroll]
                // for (int a = 1; a < 8; a ++) {
                //     float alphaPos = saturate((Time - alphas[a - 1].y) / (alphas[a].y - alphas[a - 1].y)) * step(a, 8 - 1);
                //     alpha = lerp(alpha, alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), fixedOrBlend));
                // }
            
                return float4(color, 1);
            }

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
                
               o.size = vertex.size;
               int matrixIndex = UnpackMatrixId(vertex.indices);
               int materialIndex = 0; //UnpackMaterialId(vertex.indices);
               
               //ElementMaterialInfo material = _UIForiaMaterialBuffer[materialIndex];
               
               float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
               o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos.xyz, 1.0)));

               // todo -- snapping is terrible when moving/ rotating 
               o.vertex = UIForiaPixelSnap(o.vertex);
                
               o.indices = uint4(UnpackClipRectId(vertex.indices.x), vertex.indices.y, 0, 0);
                
               return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // this could be done in the vertex shader too but this way we can support correct
                // sdf uv offsets for non quad meshes, which we'll probably want eventually to reduce overdraw
                float2 halfUV = float2(0.5, 0.5) / i.size;
                i.texCoord0.x += i.texCoord0.x > 0.5 ? halfUV.x : -halfUV.x;
                i.texCoord0.y += i.texCoord0.y > 0.5 ? halfUV.y : -halfUV.y;
                
                ElementMaterialInfo material = _UIForiaMaterialBuffer[i.indices.y];
                float2 size = i.size;
                float minSize = min(size.x, size.y);
                
                fixed4 color = UnpackColor(material.backgroundColor);
                fixed4 outlineColor = UnpackColor(material.outlineColor);
                fixed4 tintColor = UnpackColor(material.backgroundTint);
                
                uint packedRadii = material.radius;
                uint bodyColorMode = ExtractByte(material.bMode_oMode_unused, 0);
                
                // todo -- put these in a constant buffer or add to material
                float pieDirection = 1; // can be a sign bit or flag elsewhere
                float pieOpenAmount = 1; 
                float pieRotation = 0; //frac(_Time.y) * PI * 2;
                float pieRadius = max(size.x, size.y);
                float2 pieOffset = (size * 0.5) + halfUV;
                float invertPie = 1;
                // todo -- put these in a UVTransform constant buffer that is indexed
                float2 uvOffset = float2(0, 0);
                float2 uvScale = float2(1, 1);
                float uvRotation = 0; //lerp(-360, 360, frac(_Time.y * 0.1)) * Deg2Rad;
                half4 uvBounds = half4(0, 0, 1, 1);
         
                // i.texCoord1 = TransformUV(i.texCoord1, uvOffset * _MainTex_TexelSize.xy, uvScale, uvRotation, uvBounds, _SpriteAtlasPadding);
                half outlineWidth = material.outlineWidth * 0.5;
        
                if(outlineWidth == 0 || outlineColor.a <= 0.01) {
                    outlineColor = fixed4(color.rgb, 0);
                }
                
                half4 radius = UnpackRadius(packedRadii, minSize);
                
                float t = (pieOpenAmount * 180) * Deg2Rad;
                float v = t + (pieRotation * (2 * PI)) * pieDirection;
                
                float2 angleSinCos = float2(sin(t), cos(t));
                float2 samplePoint = (i.texCoord0.xy - 0.5) * size;
                float2 radialSamplePoint = RotateUV((i.texCoord0.xy * size) - pieOffset, -v * pieDirection);

 //                fixed4 colors[8] = {
 //                    fixed4(1, 0, 0, 0),
 //                    fixed4(1, 1, 0, 0.2),
 //                    fixed4(1, 0, 1, 0.4),
 //                    fixed4(0, 1, 0, 0.5),
 //                    
 //                    fixed4(1, 0, 0, 0.6),
 //                    fixed4(1, 1, 0, 0.7),
 //                    fixed4(1, 0, 0, 0.8),
 //                    fixed4(1, 1, 1, 0.9),
 //                };
 
 
                // gradient offset, gradient rotation, gradient scale, gradient type, gradient count
                // float gradRotation = 0; //frac(_Time.y) * 2 * PI;
                // half gradientTime = cos(gradRotation) * (i.texCoord0.x) + sin(gradRotation) * (i.texCoord0.y) + 0.5;
                // half4 grad = SampleGradient(gradientTime, colors, colors);

                
                float sdf = sdRoundBox(samplePoint, size * 0.5, radius);
                float radialSDF = sdPie(radialSamplePoint, angleSinCos, pieRadius);

                float bevelAmount = UnpackCornerBevel(material.bevelTop, material.bevelBottom, i.texCoord0);

                float2 bevelOffset = float2(size.x * 0.5 * (i.texCoord0.x > 0.5 ? 1 : -1), size.y * 0.5 * (i.texCoord0. y > 0.5 ? 1 : -1));
                float sdfBevel = sdRect(RotateUV(samplePoint - bevelOffset, 45 * Deg2Rad), float2(bevelAmount, bevelAmount));
                
                sdf = max(-sdfBevel, sdf);
                // todo -- move max up here to draw pie instead of using as a clipping bounds
                // sdf = max(radialSDF * invertPie, sdf);
                float sdfOutline = outlineWidth > 0 ? abs(sdf) - outlineWidth : 0;
                sdf = max(radialSDF * invertPie, sdf);
                
                color = ComputeColor(color, tintColor, bodyColorMode, i.texCoord1, _MainTex);
                color = lerp(color, outlineColor, outlineWidth == 0 ? 0 : 1 - saturate(sdfOutline));
                color.a *= 1 - smoothstep(0, fwidth(sdf), sdf);
                               
                float2 clipPos = float2(i.vertex.x, _ProjectionParams.x > 0 ? i.vertex.y : _ScreenParams.y - i.vertex.y); //* _UIForiaDPIScale;
                float4 clipRect = _UIForiaFloat4Buffer[i.indices.x]; // x = xMin, y = yMin, z = xMax, w = yMax
                float2 s = step(clipRect.xw, clipPos) - step(clipRect.zy, clipPos);
                color.a *= (s.x * s.y) != 0;
                
                color = UIForiaColorSpace(color);
                clip(color.a - 0.01);
                return color;
                
            }
            
           ENDCG
        }
    }
}
