// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)


Shader "GUITexture/Internal-GUITexture"
{
    Properties { 
        _MainTex ("Texture", any) = "" {} 
        _Color ("Color", color) = (1,1,1,1)
        _Radius ("Radius", float) = 0
        _Width ("Width", float) = 0
        _Height ("Height", float) = 0
    }
    
    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.0

    #include "UnityCG.cginc"
    
    // Quality level
    // 2 == high quality
    // 1 == medium quality
    // 0 == low quality
    #define QUALITY_LEVEL 2
    
    struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_INPUT_INSTANCE_ID
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        UNITY_VERTEX_OUTPUT_STEREO
    };

    float _Radius;
    sampler2D _MainTex;
    
    uniform float4 _MainTex_ST;

    v2f vert (appdata_t v)
    {
        v2f o;
        UNITY_SETUP_INSTANCE_ID(v);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
        o.vertex = UnityObjectToClipPos(v.vertex);
//        o.color = v.color;
        o.color = half4(1,1,1,1);
        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex) - 0.5;
        return o;
    }
   

    fixed4 frag (v2f input) : SV_Target {
    
           float dist = length(input.texcoord);
 
            #if QUALITY_LEVEL == 2
                // length derivative, 1.5 pixel smoothstep edge
                float pwidth = length(float2(ddx(dist), ddy(dist)));
                float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);
            #elif QUALITY_LEVEL == 1
                // fwidth, 1.5 pixel smoothstep edge
                float pwidth = fwidth(dist);
                float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);
            #else // Low
                // fwidth, 1 pixel linear edge
                float pwidth = fwidth(dist);
                float alpha = saturate((0.5 - dist) / pwidth);
            #endif
 
            return fixed4(input.color.rgb, input.color.a * alpha);

    }
    ENDCG

    SubShader {

        Tags { "ForceSupported" = "True" "RenderType"="Overlay" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha, One One
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {

        Tags { "ForceSupported" = "True" "RenderType"="Overlay" }

        Lighting Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    Fallback off
}
