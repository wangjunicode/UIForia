Shader "Unlit/UIForiaSDFMask"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        Cull Back
        ZTest Off
        Blend One One
        BlendOp Min
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile __ MASK_SDF MASK_TEXT MASK_TEXTURE MASK_MESH

            #include "UnityCG.cginc"
            #include "./UIForiaSDFUtil.cginc"
            #include "./BatchSize.cginc"

            float4 objectInfo[BATCH_SIZE];
            
            struct MaskAppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 objectInfo : TEXCOORD1;
            };

            struct MaskFragData {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 objectInfo : TEXCOORD1;
            };

            MaskFragData vert (MaskAppData v) {
                MaskFragData o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = UIForiaPixelSnap(o.vertex);
                o.texCoord0 = v.texCoord0;
                o.objectInfo = v.objectInfo;
                
                return o;
            }

            fixed4 frag (MaskFragData i) : SV_Target {
               
                uint packedRadiiUInt = asuint(i.objectInfo.y);
                float percentRadius = 0;
                
                float left = step(i.texCoord0.x, 0.5); // 1 if left
                float bottom = step(i.texCoord0.y, 0.5); // 1 if bottom
                
                #define top (1 - bottom)
                #define right (1 - left) 
                
                percentRadius += (top * left) * uint((packedRadiiUInt >> 0) & 0xff);
                percentRadius += (top * right) * uint((packedRadiiUInt >> 8) & 0xff);
                percentRadius += (bottom * left) * uint((packedRadiiUInt >> 16) & 0xff);
                percentRadius += (bottom * right) * uint((packedRadiiUInt >> 24) & 0xff);
                // radius comes in as a byte representing 0 to 50 of our width, remap 0 - 250 to 0 - 0.5
                percentRadius = (percentRadius * 2) / 1000; 
                
                #undef top
                #undef right
                
                float2 size = i.texCoord0.zw;
                float minSize = min(size.x, size.y);
                float padding = 0;
                float pixelRadius = (minSize * percentRadius) - padding;
                float radius = pixelRadius / minSize;
            
                float2 center = (i.texCoord0.xy - 0.5);
                
                float sdf = 0;
                sdf = RectSDF(center, float2(0.5, 0.5), radius);
                float distanceChange = fwidth(sdf) * 0.5;
                float aa = smoothstep(distanceChange, -distanceChange, sdf);
            
                fixed4 inner = fixed4(1, 1, 1, 1);
                fixed4 outer = fixed4(0, 0, 0, 0);
                
             
                
                return lerp(inner, outer, 1 - aa);

              
            }
            
            ENDCG
        }
    }
}
