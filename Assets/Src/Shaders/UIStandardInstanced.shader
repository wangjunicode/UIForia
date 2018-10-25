// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "UIForia/Instanced"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
//            fixed4 _Color;
            fixed4 _TextureSampleAdd; // unity sets this if texture is format alpha8
//            float4 _ClipRect;         // rect used to clip
            float4 _MainTex_ST;       // used for Stencil
            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _Color) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Width) 
           // UNITY_DEFINE_INSTANCED_PROP(float, _Height) 
            UNITY_INSTANCING_BUFFER_END(Props)
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata_t v)  {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
//              float width = UNITY_ACCESS_INSTANCED_PROP(_Width)
//               // float height = UNITY_ACCESS_INSTANCED_PROP(_Width)
                v.vertex.x += 100;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                //OUT.vertex += float4(100, 0, 0, 0);
                //OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);
                fixed4 color = UNITY_ACCESS_INSTANCED_PROP(Props, _Color);//(tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                // clip rect is in texture space with origin top left
                // texcoord origin is bottom left, invert to match
              //  float2 p = float2(IN.texcoord.x, 1 - IN.texcoord.y);
                // step is basically the function a < b ? 0 : 1;
                // if either coord of inside is 0, pixel is not contained in clip rect
               // float2 inside = step(_ClipRect.xy, p) * step(p, _ClipRect.zw);    

                //color.a *= inside.x * inside.y;
                //clip (color.a - 0.001);
                
                return color;
            }
            
        ENDCG
        }
    }
}
