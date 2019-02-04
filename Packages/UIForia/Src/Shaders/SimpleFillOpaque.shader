Shader "UIForia/SimpleFillOpaque"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        // Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA
        
        // todo -- handle clip stencil
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

                
            float GetPixelInRowUV(int targetY, float textureHeight) {
                return (targetY + 0.5) / textureHeight;
            }
            
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD1;
                float4 fragmentData : TEXCOORD2;
            };

        
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.flags = v.flags;
                o.fragmentData = float4(0, 16, 0, 0);
                return o;
            }
            
            uniform sampler2D _globalGradientAtlas;
            
            fixed4 frag (v2f i) : SV_Target
            {
                #define ShapeType i.flags.x
                #define Rect 1
                #define Path 2
                #define Circle 3
                #define Ellipse 4
                                
                if(ShapeType > Path) {
                    float dist = length(i.uv - 0.5);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
    
                    #if USE_DISCARD
                        clip(alpha - 0.01);
                    #endif
                    
                    return fixed4(i.color.rgb, i.color.a * alpha);
                }
                
                float t = i.uv.x;
                // 0.5 is to target center of texel, otherwise we get bad neighbor blending
                float textureHeight = 32;
                float targetY = 0;
                float sine = 0.0;
                float cosine = 0.0;
                float gradientAngle = 1.5708;
                sincos(gradientAngle, sine, cosine);
                
                float2 gradCoords = float2(i.uv.x, 0);
                // use x for horizontal, y for vertical
                t = i.uv.y;//gradCoords.x * sine + gradCoords.y * cosine;
                return tex2Dlod(_globalGradientAtlas, float4(t, GetPixelInRowUV(targetY, textureHeight), 0, 0)); 
                
            }

                 
            ENDCG
        }
        
    }
}
