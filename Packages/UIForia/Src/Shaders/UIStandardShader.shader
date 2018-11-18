Shader "UIForia/Default"
{
    Properties
    {

        _MainTex ("Sprite Texture", 2D) = "white" {}
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
            "DisableBatching" = "True" 
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Front
        Lighting Off
        ZWrite On
        ZTest LEqual
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0

            #pragma multi_compile UIFORIA_FILLTYPE_COLOR UIFORIA_FILLTYPE_TEXTURE UIFORIA_FILLTYPE_LINEAR_GRADIENT UIFORIA_FILLTYPE_CYLINDRICAL_GRADIENT UIFORIA_FILLTYPE_RADIAL_GRADIENT UIFORIA_FILLTYPE_STRIPES UIFORIA_FILLTYPE_GRID UIFORIA_FILLTYPE_CHECKER
            #pragma multi_compile __ UIFORIA_USE_BORDER 
            
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "UIForia.cginc"
            
            sampler2D _MainTex;
            uniform fixed4 _Color;
            uniform float4 _ClipRect;         
            uniform float4 _Size;
            uniform float4 _BorderSize;
            uniform float4 _BorderRadius;
            uniform fixed4 _BorderColor;
            uniform float4 _FillOffsetScale;
            uniform float4 _ContentRect;
            
            uniform float4 _Pivot;
            uniform float _Rotation;
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
            };

            v2f vert(appdata_t v)  {
                v2f OUT;
                
                if(_Rotation != 0) {
                    
                    float4 rotated = v.vertex;
                    float s = sin(_Rotation);
                    float c = cos(_Rotation);
                    
                    rotated.x -= _Pivot.x;
                    rotated.y -= _Pivot.y;
                    
                    float newX = (c * rotated.x) - (s * rotated.y);
                    float newY = (s * rotated.x) + (c * rotated.y);
                    
                    rotated.x = newX + _Pivot.x;
                    rotated.y = newY + _Pivot.y;
                    OUT.vertex = UnityObjectToClipPos(rotated);
                    
                }
                else {
                    OUT.vertex = UnityObjectToClipPos(v.vertex);
                }
                
                OUT.texcoord = v.texcoord;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target {
                #include "UIForiaFragDefines.cginc"
                #include "UIForiaFragBody.cginc"
            }
            
        ENDCG
        }
    }
}
