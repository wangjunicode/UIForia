Shader "Unlit/UIForiaLine"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off
        
        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
    
            #define prev v.prevNext.xy
            #define curr v.vertex.xy
            #define next v.prevNext.zw
            #define extrude v.flags.x
            #define leftRight v.flags.y
            
            struct appdata {
                float4 vertex : POSITION;
                float3 uvNormal : TEXCOORD0;
                float4 prevNext : TEXCOORD1;
                float4 flags : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            v2f vert (appdata v) {
                v2f o;
                
                float strokeWidth = 5;
                
                float2 toCurrent = normalize(curr - prev);
                float2 toNext = normalize(next - curr);
                float2 tangent = normalize(toNext + toCurrent);
                float2 miter = float2(-tangent.y, tangent.x);
                float2 normal = float2(-toCurrent.y, toCurrent.x) * extrude;
                
                float miterLength = strokeWidth / dot(miter, normal);
                
                float3 vertWithOffset = float3(v.vertex.x, v.vertex.y, 0);
                vertWithOffset.x += miter.x * miterLength;
                vertWithOffset.y += miter.y * miterLength;
                
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(vertWithOffset);
//                o.pixelCoord.xy = curr;
//                o.pixelCoord.zw = vertWithOffset.xy;
//                o.pixelFlags.w = distance(v.vertex.xy, vertWithOffset.xy);
//                o.pixelFlags.z = extrude;
                return o;
            }
            
            fixed MapMinusOneOneToZeroOne(fixed val) {
                return val * 0.5 + 0.5;
            }

            #define red fixed4(1, 0, 0, 1)
            #define green fixed4(0, 1, 0, 1)

            fixed4 frag (v2f i) : SV_Target {
//                float l = length(i.pixelCoord.xy);
//                float d = l / 15;
//                float dist = distance(i.pixelCoord.xy, i.pixelCoord.zw);
//                if(i.pixelFlags.w > 5) return green;
//                
//                fixed e = MapMinusOneOneToZeroOne( i.pixelCoord.z);
//                fixed lr = MapMinusOneOneToZeroOne(i.pixelCoord.w);
//                
//                if(e < 0.5) {
//                    return fixed4(1, 1, 1, 1);
//                }
//                
//                else {
//                //    if(i.pixelCoord.w > 0.5) return green;
//                    return red;
//                }
//                
//                return fixed4(d, d, d, 1);
                    return red;
                
            }
            
            ENDCG
        }
    }
}
