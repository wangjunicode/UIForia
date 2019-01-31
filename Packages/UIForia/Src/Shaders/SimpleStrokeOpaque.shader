Shader "UIForia/SimpleStrokeOpaque"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Off // todo set this to Back
        
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
                float2 uv : TEXCOORD0;
                float4 prevNext : TEXCOORD1;
                float4 flags : TEXCOORD2;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
           };

            v2f vert (appdata v)
           {
                v2f o;
                float strokeWidth = 15;
                
                float2 toCurrent = normalize(curr - prev);
                float2 toNext = normalize(next - curr);
                float2 tangent = normalize(toNext + toCurrent);
                float2 miter = float2(-tangent.y, tangent.x);
                float2 normal = float2(-toCurrent.y, toCurrent.x) * extrude;
                
                float miterLength = strokeWidth / dot(miter, normal);
                
                float2 vertWithOffset = v.vertex + miter * miterLength;
                
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(float3(vertWithOffset.xy, v.vertex.z)); 
                return o;
                
           }

           fixed4 frag (v2f i) : SV_Target
           {
               return i.color;
           }
           ENDCG
        }
    }
}
