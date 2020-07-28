Shader "UIForia/UIForiaShape"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        
        // required for UIForia Clipping Mask
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { 
           
        }
        
        LOD 100

        Pass {
        
            Cull Back // configurable is probably the best approach for this
            
//            ColorMask [_ColorMask]
//            Stencil {
//                Ref [_Stencil]
//                Comp [_StencilComp]
//                Pass [_StencilOp] 
//            }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5
            
            #include "UnityCG.cginc"
            #include "UIForia.cginc"
           
            struct appdata {
                uint vid : SV_VertexID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texCoord0 : TEXCOORD0;
                float2 texCoord1 : TEXCOORD1;
                int2 indices : TEXCOORD2;
            };

            // layout must EXACTLY match ElementMaterialInfo in C#
            struct ElementMaterialInfo {
                
                uint backgroundColor;
                float2 size;
                float zPosition;

                uint backgroundTint;
                uint outlineColor;
                uint outlineTint;

                uint radius;
                uint bevel;
                uint meshPie;
                float2 meshPieOffset;
                
                float opacity;
                float outlineWidth;
                uint colorModeAndUnused;
            };
        
            // todo -- this format is wrong since I changed text, fix it!
            struct UIForiaVertex {
                float2 position;
                float2 texCoord0; // sdf uvs (inset by half a pixel)
                float2 texCoord1; // texture uvs
                int2 indices;     // lookup indices in buffers packed into ushorts
            };
            
            sampler2D _MainTex;
            sampler2D _OutlineTex;
            
            float4 _MainTex_TexelSize; 
            
            float _SpriteAtlasPadding;
            float _UIForiaDPIScale;
            float4x4 _UIForiaOriginMatrix;
            
            // todo -- none of this stuff works atm
            
            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;            
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;            
            StructuredBuffer<AxisAlignedBounds2D> _UIForiaClipRectBuffer;            
            StructuredBuffer<ElementMaterialInfo> _UIForiaMaterialBuffer;            
            
            v2f vert (appdata v) {
               v2f o;
               UIForiaVertex vertex = _UIForiaVertexBuffer[v.vid];
               int matrixIndex = 0; //UnpackMatrixId(vertex.indices);
               int materialIndex = 0; //UnpackMaterialId(vertex.indices);
               
               ElementMaterialInfo material = _UIForiaMaterialBuffer[materialIndex];
               
               float4x4 transform = mul(_UIForiaMatrixBuffer[matrixIndex], _UIForiaOriginMatrix);
                                              
               o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vertex.position.xy, material.zPosition, 1.0)));
               o.texCoord0 = vertex.texCoord0;
               o.texCoord1 = vertex.texCoord1;
               o.indices = vertex.indices;
         
               return o;
            } 
            
            fixed4 frag (v2f i) : SV_Target {
                float2 screenPosition = float2(0, 0); // todo 
                //UNITY_VPOS_TYPE screenPosition : VPOS
                //screenPosition.y = _ScreenParams.y - screenPosition.y;
                
                ElementMaterialInfo material = _UIForiaMaterialBuffer[UnpackMaterialId(i.indices)];
                float2 size = material.size;
                float4 color = UnpackColor(material.backgroundColor);
                fixed4 tintColor = UnpackColor(material.backgroundTint);
                
                uint packedRadii = material.radius;
                uint bodyColorMode = ExtractByte(material.colorModeAndUnused, 0);
                
                // todo -- put these in a constant buffer or add to material
                float pieDirection = 1;
                float pieOpenAmount = 1;
                float pieRotation = 0; //frac(_Time.y * 0.5);
                float pieRadius = max(size.x, size.y);
                float2 pieOffset = float2(size.x * 0.5, size.y * 0.5);
                
                // todo -- put these in a UVTransform constant buffer that is indexed
                float2 uvOffset = float2(0, 0);
                float2 uvScale = float2(1, 1);
                float uvRotation = 0; //lerp(-360, 360, frac(_Time.y * 0.1)) * Deg2Rad;
                half4 uvBounds = half4(0, 0, 1, 1);
                
                i.texCoord1 = TransformUV(i.texCoord1, uvOffset * _MainTex_TexelSize.xy, uvScale, uvRotation, uvBounds, _SpriteAtlasPadding);
                
                half outlineWidth = material.outlineWidth * 0.5;

                half4 radius = UnpackRadius(packedRadii, size);
                
                float t = (pieOpenAmount * 180) * Deg2Rad;
                float v = t + (pieRotation * (2 * PI)) * pieDirection;
                
                float2 angleSinCos = float2(sin(t), cos(t));
                float2 samplePoint = (i.texCoord0.xy - 0.5) * size;
               
                float2 radialSamplePoint = RotateUV((i.texCoord0.xy * size) - pieOffset, -v * pieDirection);
                
                float sdf = sdRoundBox(samplePoint, (size * 0.5) - outlineWidth, radius - outlineWidth);
                float radialSDF = sdPie(radialSamplePoint, angleSinCos, pieRadius);
                
                sdf = max(radialSDF, sdf);
                
                float distanceChange = fwidth(sdf); 
               
                sdf = lerp(sdf, abs(sdf) - outlineWidth, outlineWidth != 0); // handle outline
                
                float4 c = color;
                
                c = ComputeColor(color, tintColor, bodyColorMode, i.texCoord1, _MainTex);
                
                c.a *= 1 - smoothstep(0, distanceChange, sdf);
                
                c.a *= UIForiaOverflowClip(screenPosition, _UIForiaClipRectBuffer[UnpackClipRectId(i.indices)]);
                
                // return lerp(fixed4(1, 1, 1, 1), c,c.a); //todo -- remove
                
                return UIForiaColorSpace(c);
                
            }
            
           ENDCG
        }
    }
}
