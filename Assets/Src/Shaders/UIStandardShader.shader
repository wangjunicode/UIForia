Shader "UIForia/Default"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1) // todo -- maybe remove

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

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _TextureSampleAdd; // unity sets this if texture is format alpha8
            float4 _ClipRect;         // rect used to clip
            float4 _MainTex_ST;       // used for Stencil
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR; // todo -- probably best to take this from material and not the vertex
                float2 texcoord  : TEXCOORD0;
                float4 world  : TEXCOORD1;
            };

            v2f vert(appdata_t v)  {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
                OUT.color = v.color * _Color;
                OUT.world = v.vertex;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target {
                fixed4 color = (tex2D(_MainTex, IN.texcoord) + _TextureSampleAdd) * IN.color;
                
                // clip rect is in texture space with origin top left
                // texcoord origin is bottom left, invert to match
                float2 p = float2(IN.texcoord.x, 1 - IN.texcoord.y);
                // step is basically the function a < b ? 0 : 1;
                // if either coord of inside is 0, pixel is not contained in clip rect
                float2 inside = step(_ClipRect.xy, p) * step(p, _ClipRect.zw);    

                color.a *= inside.x * inside.y;
                clip (color.a - 0.001);
                
                return color;
            }
            
        ENDCG
        }
    }
}
