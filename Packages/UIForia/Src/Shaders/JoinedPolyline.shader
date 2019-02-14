Shader "UIForia/JoinedPolyline" {
    Properties {    }
    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off // todo set this to Back
        Blend SrcAlpha OneMinusSrcAlpha
        ZClip Off
        
        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma enable_d3d11_debug_symbols

           #include "UnityCG.cginc"
           
           #define antialias 2
           
           struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 flags : TEXCOORD1;
                float4 prevNext : TEXCOORD2;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD0;
                float4 uv : TEXCOORD1;
           };

           v2f vert (appdata input) {
               v2f o;
               
               float2 prev = input.prevNext.xy;
               float2 next = input.prevNext.zw;
               float2 curr = input.vertex.xy;
               
               float strokeWidth = 15; //input.flags.w;
               int idx = input.flags.y;
               
               float w = (strokeWidth * 0.5) + antialias;
               float dir = input.flags.x;
               
               float2 v0 = normalize(curr - prev);
               float2 v1 = normalize(next - curr);
               
               float2 n0 = float2(-v0.y, v0.x);
               float2 n1 = float2(-v1.y, v1.x);
               
               float2 miter = normalize(n0 + n1);
                        
               float miterLength = w / dot(miter, n1);
               float2 pos = curr + (miter * miterLength * dir);
               
               o.color = input.color;
               o.uv = float4(w, w * dir, 0, 0);
               o.flags = float4(strokeWidth, w * dir, 0, 0);
               o.vertex = UnityObjectToClipPos(float3(pos.xy, input.vertex.z));
//               if(dir < 0) {
//               }
//               else {
//               o.vertex = UnityObjectToClipPos(float3(input.vertex.xy + strokeWidth, input.vertex.z));
//               }
               
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
               
               float thickness = i.flags.x;
               float w = (thickness * 0.5) - antialias; // todo -- scale AA by strokeWidth somehow

               float d = abs(i.uv.y) - w;
               
               if(d <= 0) {
                   return i.color;
               }
               
               d /= antialias;
               float threshold = 1;
               float afwidth = length(float2(ddx(d), ddy(d)));
               float aa = smoothstep(threshold - afwidth, threshold + afwidth, d);
               return fixed4(i.color.rgb, i.color.a * (1 - aa));
               
           }
           ENDCG
        }
    }
}
