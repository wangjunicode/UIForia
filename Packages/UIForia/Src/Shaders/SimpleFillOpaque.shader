Shader "UIForia/SimpleFillOpaque"
{
    Properties
    {
        _StencilComp ("Stencil Comparison", Float) = 8
        _StencilRef ("Stencil ID", Float) = 1
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }

        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA
                   
        Stencil {
            Ref [_StencilRef]
            Comp Equal
        }
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UIForiaInc.cginc"

            uniform sampler2D _MainTex;
            uniform sampler2D _globalGradientAtlas;
            uniform float _globalGradientAtlasSize;
         
            struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
            };

            // 0.5 is to target center of texel, otherwise we get bad neighbor blending
            float GetPixelInRowUV(int targetY, float textureHeight) {
                return (targetY + 0.5) / textureHeight;
            }
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.flags = v.flags;
                return o;
            }
            
            float2 Unpack(float input, int precision) {
                return float2(floor(input / precision), input % precision) / precision - 1;
            }

            fixed4 frag (v2f i) : SV_Target {
                float2 unpackedFillColorMode = Unpack(i.flags.y, 4096);
                
                #define ShapeType i.flags.x
                #define GradientId i.flags.z
                #define GradientDirection i.flags.w
                
                #define FillMode (unpackedFillColorMode.x)
                #define ColorMode ((int)unpackedFillColorMode.y)
                
                //((int)unpackedFillColorMode.y)
                                                
                #define ColorMode_Gradient 1
                
//                if(FillMode != 0) return fixed4(0, 1, 0, 1);
//                if(ColorMode > 0) return fixed4(1, 1, 0, 1);
                
                float t = lerp(i.uv.x, i.uv.y, GradientDirection);
                    
                fixed4 color = fixed4(1, 1, 1, 1);
                fixed4 textureColor = tex2D(_MainTex, i.uv);
                fixed4 gradientColor = tex2Dlod(_globalGradientAtlas, float4(t, GetPixelInRowUV(GradientId, _globalGradientAtlasSize), 0, 0));
                
                textureColor = lerp(textureColor, color, 1);
                gradientColor = lerp(gradientColor, color, 0);
                
                color = i.color;//lerp(textureColor, gradientColor, 1);
                
//                bool colorMode = fmod(flags, 2) == 1);
//                bool colorMode = fmod(flags, 4) >= 2);
//                bool colorMode = fmod(flags, 8) >= 4);
//                bool colorMode = fmod(flags, 16) >= 8);
                
                if(ShapeType > ShapeType_Path) {
                    float dist = length(i.uv - 0.5);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
                    
                    if(alpha - 0.01 <= 0) discard;
                    
                    color = fixed4(color.rgb, color.a * alpha);
                }
               
                return color;
            }

                 
            ENDCG
        }
        
    }
}
