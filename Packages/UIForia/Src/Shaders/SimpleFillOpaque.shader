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
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGBA
        
        // todo -- handle clip stencil
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

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
            };

        
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv - 0.5;
                o.color = v.color;
                o.flags = v.flags;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                #define ShapeType i.flags.x
                #define Rect 1
                #define Path 2
                #define Circle 3
                #define Ellipse 4
                                
                if(ShapeType > Path) {
                    float dist = length(i.uv);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
    
                    #if USE_DISCARD
                        clip(alpha - 0.01);
                    #endif
                    
                    return fixed4(i.color.rgb, i.color.a * alpha);
                }
                
                return i.color;
                
            }
            ENDCG
        }
    }
}
