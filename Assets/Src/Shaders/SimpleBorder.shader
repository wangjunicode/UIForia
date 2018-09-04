Shader "UIElement/BorderedImage"
{
    Properties
    {
		[PerRendererData]_MainTex ("Base (RGB)", 2D) = "white" {}
        _RectVector("ScreenSpaceRect", Vector) = (0, 0, 0, 0)
        _BorderWidthVector("BorderWidths", Vector) = (0, 0, 0, 0)
        _BorderColor("BorderColor", Color) = (1,1,1,1)
        _ContentColor("ContentColor", Color) = (1,1,1,1)

       // _BorderRadii("BorderRadii", Vector) = (0, 0, 0, 0)
        
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
			#include "UnityUI.cginc"

            sampler2D _MainTex;
            
            bool _UseClipRect;
			bool _UseAlphaClip;
			
			float4 _ClipRect;
            float4 _MainTex_ST;
			
			fixed4 _TextureSampleAdd;
			
			uniform Vector _BorderColor;
            uniform Vector _ContentColor;
            
			uniform Vector _RectVector;
			uniform Vector _BorderWidthVector;
			
            struct appdata {
                float4 vertex : POSITION;
                float4 color    : COLOR;
                float2 uv : TEXCOORD0;

            };

            struct v2f {
                float2 uv : TEXCOORD0;
   				fixed4 color : COLOR;
                float4 vertex : SV_POSITION;
   				float4 worldPosition : TEXCOORD1;

            };
            
             bool IsPointInside(float2 p, float4 rect) {
                return p.x >= rect.x 
                && p.x <= (rect.x + rect.z) 
                && p.y >= rect.y 
                && p.y <= (rect.y + rect.w);
            }
            
            v2f vert (appdata v) {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * (1 + _TextureSampleAdd);
                return o;
            }
         

            fixed4 frag (v2f i) : SV_Target {
            
                float2 p = i.uv;
                p.x *= _RectVector.z;
                p.y *= _RectVector.w;
                
                float4 centerRect = float4(
                    _RectVector.x + _BorderWidthVector.x,
                    _RectVector.y + _BorderWidthVector.y,
                    _RectVector.z - (_BorderWidthVector.x + _BorderWidthVector.z),
                    _RectVector.w - (_BorderWidthVector.y + _BorderWidthVector.w)
                );

                fixed4 color = lerp(_BorderColor, _ContentColor, IsPointInside(p, centerRect));
                
              if (_UseClipRect) {
			     color *= UnityGet2DClipping(i.worldPosition, _ClipRect);
			  }	
		
		      if (_UseAlphaClip) {
			    clip (color.a - 0.001);
			  }
			  		
			  return color;
            }
            
           
    
            ENDCG
        }
    }
}