Shader "UIForia/UIForiaShadow" {
    Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader {
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
            };

            struct UIForiaVertex
            {
                float2 position;
                float2 size;
                int4 indices;
            };

            sampler2D _MainTex;
            sampler2D _OutlineTex;

            float4 _MainTex_TexelSize;

            float _UIForiaDPIScale;
            float4x4 _UIForiaOriginMatrix;

            struct GradientInfo
            {
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

                o.texCoord0.x = vpos.x / mainSize.z;
                o.texCoord0.y = -vpos.y / mainSize.w;
                o.size = mainSize.zw;

                // sdf uv offsets for non quad meshes, which we'll probably want eventually to reduce overdraw
                float2 halfUV = float2(0.5, 0.5) / mainSize.zw; // todo -- this might need to be dpi scaled
                o.texCoord0.x += o.texCoord0.x > 0.5 ? halfUV.x : -halfUV.x;
                o.texCoord0.y += o.texCoord0.y > 0.5 ? halfUV.y : -halfUV.y;

                o.uvTransform = material.uvTransformIdx != 0 ? _UIForiaFloat4Buffer[material.uvTransformIdx] : float4(1, 1, 0, 0);

                float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
                o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos.xyz, 1.0)));

                // todo -- snapping is terrible when moving/ rotating 
                // o.vertex = UIForiaPixelSnap(o.vertex);

                o.indices = uint4(UnpackClipRectId(vertex.indices.x), vertex.indices.y, vertex.indices.z, vertex.indices.w);

                return o;
            }

            float easeInOutQuad(float x)
            {
                return x < 0.5 ? 2 * x * x : 1 - pow(-2 * x + 2, 2) / 2;
            }

            fixed4 Blur(float2 texCoord, half2 blur, float s, half4 radius)
            {
                const int KERNEL_SIZE = 7;
                const float KERNEL_[7] = {0.1719, 0.4566, 0.8204, 1.0, 0.8204, 0.4566, 0.1719};

                float4 o = 0;
                float sum = 0;
                float2 shift = 0;
                for (int x = 0; x < KERNEL_SIZE; x++)
                {
                    shift.x = blur.x * (float(x) - KERNEL_SIZE / 2);
                    for (int y = 0; y < KERNEL_SIZE; y++)
                    {
                        shift.y = blur.y * (float(y) - KERNEL_SIZE / 2);
                        float2 uv = texCoord + shift;
                        float weight = KERNEL_[x] * KERNEL_[y];
                        sum += weight;

                        float sdf = sdRoundBox(uv - 0.5, float2(0.5, 0.5), radius);
                        fixed4 color = BLACK;
                        color.a *= 1 - smoothstep(0, 0.02, sdf);
                        o += color * weight;
                    }
                }
                return o / sum;
            }

            // A standard gaussian function, used for weighting samples
            float gaussian(float x, float sigma)
            {
                const float pi = 3.141592653589793;
                return exp(-(x * x) / (2.0 * sigma * sigma)) / (sqrt(2.0 * pi) * sigma);
            }

            // This approximates the error function, needed for the gaussian integral
            float2 erf(float2 x)
            {
                float2 s = sign(x), a = abs(x);
                x = 1.0 + (0.278393 + (0.230389 + 0.078108 * (a * a)) * a) * a;
                x *= x;
                return s - s / (x * x);
            }

            // Return the blurred mask along the x dimension
            float roundedBoxShadowX(float x, float y, float sigma, float corner, float2 halfSize)
            {
                float delta = min(halfSize.y - corner - abs(y), 0.0);
                float curved = halfSize.x - corner + sqrt(max(0.0, corner * corner - delta * delta));
                float2 integral = 0.5 + 0.5 * erf((x + float2(-curved, curved)) * (sqrt(0.5) / sigma));
                return integral.y - integral.x;
            }

            // Return the mask for the shadow of a box from lower to upper
            float roundedBoxShadow(float2 lower, float2 upper, float2 pt, float sigma, float corner)
            {
                // Center everything to make the math easier
                float2 center = (lower + upper) * 0.5;
                float2 halfSize = (upper - lower) * 0.5;
                pt -= center;

                // The signal is only non-zero in a limited range, so don't waste samples
                float low = pt.y - halfSize.y;
                float high = pt.y + halfSize.y;
                float start = clamp(-3.0 * sigma, low, high);
                float end = clamp(3.0 * sigma, low, high);

                // Accumulate samples (we can get away with surprisingly few samples)
                float step = (end - start) / 4.0;
                float y = start + step * 0.5;
                float value = 0.0;
                for (int i = 0; i < 4; i++)
                {
                    value += roundedBoxShadowX(pt.x, pt.y - y, sigma, corner, halfSize) * gaussian(y, sigma) * step;
                    y += step;
                }

                return value;
            }

            float easeOutBack(float x)
            {
                const float c1 = 1.70158;
                const float c3 = c1 + 1;

                return 1 + c3 * pow(x - 1, 3) + c1 * pow(x - 1, 2);
            }
            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 color = RED;
                return color;
                // float2 size = i.size;
                // float minSize = min(size.x, size.y);
                // ElementMaterialInfo material = _UIForiaMaterialBuffer[i.indices.y & 0xffffff];
                //
                // uint packedRadii = material.radius;
                // half4 radius = UnpackRadius(packedRadii, minSize);
                //
                // float s = 0.25;
                // radius = float4(0.5, 0.5, 0.0, 0.2) * s;
                //
                // float sdf = sdRoundBox(i.texCoord0 - 0.5, float2(s, s), radius);
                // // color.rgb = col * 1 - exp(-10 * abs(sdf));
                // // color.a = 1 - easeInOutQuad(4 * sdf);
                // float bevelAmount = 10 * (1 / size.x);
                //
                // float2 bevelOffset = float2(0.5 * (i.texCoord0.x > 0.5 ? 1 : -1), 0.5 * (i.texCoord0.y > 0.5 ? 1 : -1));
                //
                // float sdfBevel = sdRect(RotateUV(i.texCoord0 - bevelOffset, 45 * Deg2Rad), float2(bevelAmount, bevelAmount));
                // sdf = max(-sdfBevel, sdf);
                //
                // float sigma = 100;
                // fixed4 boxShadowColor = drawRectShadow(i.texCoord0.xy * size, float4(0 + sqrt(sigma / s), 20 - sqrt(sigma / 2), size.xy), fixed4(0, 0, 0, 1), sigma);
                // // sdf = abs(sdf) - 0.0015;
                // float glow = 1.0 / (abs(sdf));
                // // sdf *= s /(0.5 *s); //1.5; //lerp(1, 10, saturate(sin(_Time.y)));
                // // sdf = pow(sdf, 3); //lerp(1, 3, frac(_Time.y)));
                // //color.rgb = glow * color.rgb; //float3(1.0, 0.5, 0.25);
                // // color.rgb = 1.0 - exp(-color.rgb);
                // color.a *= 1.0 - smoothstep(0, lerp(s * 0.5, fwidth(sdf), cos(_Time.y)), sdf);
                // return UIForiaColorSpace(color);
               

                // if(sdf > s) color.a = 0;

                clip(color.a - 0.005);
                return color; //(blend(WHITE, boxShadowColor));
                // color.a *= 1 - smoothstep(0, 1, sdf);
                //  color.a = Remap(color.a, 0, 1, 0, 0.5);
                return color;

                return (color);
            }
            ENDCG
        }
    }
}