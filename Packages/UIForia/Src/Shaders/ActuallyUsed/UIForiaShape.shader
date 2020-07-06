Shader "UIForia/UIForiaShape"
{
    Properties
    {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Texture", 2D) = "white" {}
        // required for UI.Mask
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 100

        Pass {
            Cull off
            ColorMask [_ColorMask]
            Stencil {
                Ref [_Stencil]
                Comp [_StencilComp]
                Pass [_StencilOp] 
                ReadMask [_StencilReadMask]
                WriteMask [_StencilWriteMask]
            }
            
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            
            #include "UnityCG.cginc"
            #include "UIForia.cginc"
           
            struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                fixed4 color : COLOR;
            };

            struct v2f {
               float4 vertex : SV_POSITION;
                float4 uv : TEXCOORD0;
                fixed4 color : COLOR;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _TextureSampleAdd;
            fixed4 _Color;
            
            float4 _OverflowClippers[12];            
            
           v2f vert (appdata v) {
               v2f o;
               float4 screenPos = ComputeScreenPos(v.vertex);
               o.vertex = UnityObjectToClipPos(v.vertex);
               o.worldPosition = screenPos;
               o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
               o.color = v.color;
               //o.vertex = v.vertex.xyzw;
               return o;
           }

            //fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target {
            fixed4 frag (v2f i) : SV_Target {
           
              //  screenPos.y = _ScreenParams.y - screenPos.y;
                fixed4 vertexColor = i.color; //UIForiaColorSpace(i.color);
                
                //if(!UIForiaOverflowClip(screenPos, 0)) {
                //   discard;
                //}
                
                half4 color = tex2D(_MainTex, i.uv.xy); // + _TextureSampleAdd);// * vertexColor;
                return i.color;
            }
            
            ENDCG
        }
    }
}
