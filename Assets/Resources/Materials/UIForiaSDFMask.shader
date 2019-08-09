Shader "Unlit/UIForiaSDFMask"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        ZTest Off
        ColorMask A
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
            };

            struct v2f {
                float4 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                
                return o;
            }

            inline float RectSDF(float2 p, float2 size, float r) {
               float2 d = abs(p) - size + float2(r, r);
               return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
            }
            
            fixed4 frag (v2f i) : SV_Target {
            
                float2 size = i.uv.zw;
                float minSize = min(size.x, size.y);
               
                float radius = clamp(minSize * 0.5, 0, minSize);
                
                // upscale size by that factor, the size passed in is the layout box size, not the geometry size which is why this works
                float2 center = ((i.uv.xy - 0.5) * size);
                // need to give padding to these sdf things or they may get cut off in an ugly way
                
                float sdf = RectSDF(center, (size * 0.5), radius);
                //float sdf = length(i.uv.zw * (i.uv.xy - 0.5)) - (min(i.uv.z, i.uv.w) *0.5);
                sdf = 1 - smoothstep(-1, 1, sdf);
                return lerp(fixed4(1, 0, 0, 0), fixed4(sdf, sdf, sdf, 1), sdf); //fixed4(sdf, 0, 0, 1);
            }
            
            ENDCG
        }
    }
}
