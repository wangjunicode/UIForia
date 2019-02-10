Shader "UIForia/JoinedPolyline" {
    Properties {    }
    SubShader {
        Tags { "RenderType"="Transparent" }
        LOD 100
        Cull Back // todo set this to Back
        Blend SrcAlpha OneMinusSrcAlpha

        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma enable_d3d11_debug_symbols

           #include "UnityCG.cginc"
           
           #define antialias 2
           
           float ComputeU(float2 p0, float2 p1, float2 p) {
               float2 v = p1 - p0;
               return ((p.x - p0.x) * v.x + (p.y - p0.y) * v.y) / length(v); 
           }
           
           float LineDistance(float2 p0, float2 p1, float2 p) {
               float2 v = p1 - p0;
               float l2 = v.x * v.x + v.y * v.y;
               float u = ((p.x - p0.x) * v.x + (p.y - p0.y) * v.y) / l2;
               
               float2 h = p0 + u * v;
               return length(p - h);
           }
           
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
                float uvY : TEXCOORD1;
           };

           v2f vert (appdata input) {
               v2f o;
               
               float2 prev = input.prevNext.xy;
               float2 next = input.prevNext.zw;
               float2 curr = input.vertex.xy;
               int idx = input.flags.y;
               
               float linewidth = 30;
               
               float w = (linewidth * 0.5) + antialias;
               float dir = input.flags.x;
               
               float2 v0 = normalize(curr - prev);
               float2 v1 = normalize(next - curr);
               
               float2 n0 = float2(-v0.y, v0.x);
               float2 n1 = float2(-v1.y, v1.x);
               
               float2 miter = normalize(n0 + n1);
               
               float segmentLength = length(curr - next);
               float prevSegmentLength = length(curr - prev);
               
               float miterLength = w / dot(miter, n1);
               
               if(idx == 0 || idx == 2) {
                if(miterLength > prevSegmentLength) miterLength = prevSegmentLength;
               }
               if(idx == 1 || idx == 3) {
                if(miterLength > segmentLength) miterLength = segmentLength;
               }
               
               float2 pos = float2(0, 0);
               

               o.flags = float4(linewidth, w * dir, 0, 0);
               o.uvY = w * dir;
               o.color = input.color;
               if(input.flags.z == 1) {
                   o.vertex = UnityObjectToClipPos(float3(input.vertex.xy + n0 * w * dir, input.vertex.z));
                   return o;
               }
               
               // body
               if(input.flags.y < 4) {
                   pos = curr + (miter * miterLength * dir);
                   
               }
               //join
               else {
                   pos = input.vertex.xy;
               }
               
               
               o.vertex = UnityObjectToClipPos(float3(pos.x, pos.y, input.vertex.z));
               return o;
           }

           fixed4 frag (v2f i) : SV_Target {
               return fixed4(1, 1, 1, 1);
               //return i.color;
               float d = 0;
               float thickness = i.flags.x;
               
               float w = (thickness / 2) - antialias;
               d = abs(i.uvY) - w;
               if(d < 0) {
                   return i.color;
               }
               d /= antialias;
               return fixed4(i.color.rgb, i.color.a * exp(-d * d));
           }
           ENDCG
        }
    }
}
