Shader "UIForia/Instanced"
{
    Properties
    {
//        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

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
        ZClip False
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            name "default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile UIFORIA_FILLTYPE_COLOR UIFORIA_FILLTYPE_LINEAR_GRADIENT UIFORIA_FILLTYPE_CYLINDRICAL_GRADIENT UIFORIA_FILLTYPE_RADIAL_GRADIENT UIFORIA_FILLTYPE_STRIPES UIFORIA_FILLTYPE_GRID UIFORIA_FILLTYPE_CHECKER
            #pragma multi_compile UIFORIA_USE_BORDER 
            #pragma multi_compile_instancing
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "UIForia.cginc"
                        
            uniform sampler2D _MainTex;

            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _PrimaryColor) 
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _SecondaryColor) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _ClipRect) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _BorderSize) 
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _BorderColor) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _BorderRadius) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _SizeRotationGradientStart) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _FillOffsetAndScale) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _GridAndLineSize) 
            UNITY_INSTANCING_BUFFER_END(Props)
            
            
            struct appdata_t {
                float4 vertex   : POSITION;
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
                UNITY_TRANSFER_INSTANCE_ID(v, OUT);
                float2 size = UNITY_ACCESS_INSTANCED_PROP(Props, _SizeRotationGradientStart).xy;
                v.vertex.x += v.normal.x * size.x;
                v.vertex.y -= v.normal.y * size.y;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);
                #define UIFORIA_USE_INSTANCING
                #include "UIForiaFragDefines.cginc"
                #include "UIForiaFragBody.cginc"
            }
            
        ENDCG
        }
    }
}
