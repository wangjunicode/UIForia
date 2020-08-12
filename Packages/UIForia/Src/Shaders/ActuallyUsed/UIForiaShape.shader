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
                uint4 indices : TEXCOORD2;
                float2 size : TEXCOORD3;
                half4 uvTransform : TEXCOORD4;
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
                uint uvRotation_unused;
                
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
                        
            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;            
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;            
            StructuredBuffer<ElementMaterialInfo> _UIForiaMaterialBuffer;            
            StructuredBuffer<float4> _UIForiaFloat4Buffer;
           

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
               
               int matrixIndex = UnpackMatrixId(vertex.indices);
               int materialIndex = UnpackMaterialId(vertex.indices);
               ElementMaterialInfo material = _UIForiaMaterialBuffer[materialIndex];

               o.uvTransform = material.uvTransformIdx != 0 ? _UIForiaFloat4Buffer[material.uvTransformIdx] : float4(1, 1, 0, 0);
                
               o.size = vertex.size;
               
               float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
               o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos.xyz, 1.0)));

               // todo -- snapping is terrible when moving/ rotating 
               o.vertex = UIForiaPixelSnap(o.vertex);
                
               o.indices = uint4(UnpackClipRectId(vertex.indices.x), vertex.indices.y, vertex.indices.z, vertex.indices.w);
                
               return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                // this could be done in the vertex shader too but this way we can support correct
                // sdf uv offsets for non quad meshes, which we'll probably want eventually to reduce overdraw
               float2 halfUV = float2(0.5, 0.5) / i.size;
               i.texCoord0.x += i.texCoord0.x > 0.5 ? halfUV.x : -halfUV.x;
               i.texCoord0.y += i.texCoord0.y > 0.5 ? halfUV.y : -halfUV.y;
                
                float opacity = 1; // (float)i.indices.z / 255.0; // todo --- maybe unpack from elsewhere
                
                ElementMaterialInfo material = _UIForiaMaterialBuffer[i.indices.y];
                float2 size = i.size;
                float minSize = min(size.x, size.y);
                
                fixed4 color = UnpackColor(material.backgroundColor);
                fixed4 outlineColor = UnpackColor(material.outlineColor);
                fixed4 tintColor = UnpackColor(material.backgroundTint);
                
                uint packedRadii = material.radius;
                uint bodyColorMode = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 0);
                
                // todo -- put these in a constant buffer or add to material
                
                 // can be a sign bit or flag elsewhere
                float fillDirection = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 2) == 0 ? 1 : -1;
                float invertFill = ExtractByte(material.bMode_oMode_meshFillDirection_meshFillInvert, 3) == 0 ? 1 : -1; 
                float fillAmount = UnpackLowUShortPercentageToFloat(material.fillOpenAndRotation);
                float fillRotation = UnpackHighUShortPercentageToFloat(material.fillOpenAndRotation); //frac(_Time.y) * PI * 2;
                float fillRadius = material.fillRadius;
                float2 fillOffset = float2(material.fillOffsetX, material.fillOffsetY) - halfUV;
                 
                float2 uvScale = i.uvTransform.xy;
                float2 uvOffset = i.uvTransform.zw;
                float uvRotation = UnpackLowUShortPercentageToFloat(material.uvRotation_unused) * 2 * PI;
                
                float4 uvBounds = float4(
                    UnpackLowBytes(i.indices.z) * _MainTex_TexelSize.x,
                    UnpackHighBytes(i.indices.z) * _MainTex_TexelSize.y,
                    UnpackLowBytes(i.indices.w) * _MainTex_TexelSize.x,
                    UnpackHighBytes(i.indices.w) * _MainTex_TexelSize.y
                );
         
                float2 originalUV = i.texCoord1;
                i.texCoord1 = TransformUV(i.texCoord1, uvOffset * _MainTex_TexelSize.xy, uvScale, uvRotation, uvBounds);
                
                half outlineWidth = material.outlineWidth * 0.5;
                if(outlineWidth == 0 || outlineColor.a <= 0.01) {
                    outlineColor = fixed4(color.rgb, 0);
                }
                
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
                float gradRotation = 0 * Deg2Rad;
                float2 gradientOffset = float2(0, 0);
                int wrapGradient = 0;
                int hardBlend = 0;
                int gradientType = GradientType_Linear;
                
                float2 gradientTexCoord = gradientOffset + i.texCoord0.xy;
                half gradientTime = LinearGradient(gradientTexCoord, gradRotation);
                gradientTime = lerp(gradientTime, RadialGradient(gradientTexCoord), gradientType == GradientType_Radial);
                gradientTime = lerp(gradientTime, ConicalGradient(gradientTexCoord), gradientType == GradientType_Conical); 
                gradientTime *= gradientScale;
                
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
                sdf = max(radialSDF * invertFill, sdf);
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
                //color.a *= (s.x * s.y) != 0; todo -- fix this
                color = UIForiaColorSpace(color); // todo -- video texture wont want to adjust color space 
                clip(color.a - 0.01);
                return color;
                
            }
            
           ENDCG
        }
    }
}
