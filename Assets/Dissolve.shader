Shader "UIForia/Dissolve"
{
	Properties
	{
		_Color ("Tint", Color) = (1,1,1,1)
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
			#pragma vertex vert1
			#pragma fragment frag1
			#pragma target 2.0
			
			#define DISSOLVE 1
	//		#pragma multi_compile __ UNITY_UI_ALPHACLIP
	//		#pragma shader_feature __ ADD SUBTRACT FILL

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

		//	#define UI_DISSOLVE 1
		//	#include "./UIEffect.cginc"
		//	#include "./UIEffect-Sprite.cginc"

            sampler2D _MainTex;
            sampler2D _NoiseTex;
            
            struct a2v {
                float4 vertex   : POSITION;
	            float4 color    : COLOR;
	            float2 texcoord : TEXCOORD0;
            };
            
            struct v2f {
                float4 vertex   : SV_POSITION;
	            fixed4 color    : COLOR;
	            half2 texcoord  : TEXCOORD0;
             	float4 worldPosition : TEXCOORD1;
            };
            
            v2f vert1(a2v IN) {
                v2f OUT;
                OUT.worldPosition = IN.vertex;
	            OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
	            OUT.texcoord = IN.texcoord;
	            return OUT;
            }
                        
            fixed4 _DissolveColor;
            fixed _Factor;
            fixed _Width;
            fixed _Softness;
            
            float Remap (float value, float from1, float to1, float from2, float to2) {
               return (value - from1) / (to1 - from1) * (to2 - from2) + from2;
            }
            
			fixed4 frag1(v2f IN) : SV_Target {
                _Width = 0.1;
                _Softness = 1;
                _Factor = Remap(_SinTime.w, -1, 1, 0, 1); //0.2;
                _DissolveColor = fixed4(1, 0, 0, 1);
                
              fixed4 color = tex2D(_MainTex, IN.texcoord.xy);
              float alpha = tex2D(_NoiseTex, IN.texcoord.xy).r;
              
              fixed width = _Width / 4;
              fixed softness = _Softness;
              fixed3 dissolveColor = _DissolveColor.rgb;
              float factor = alpha - _Factor * ( 1 + width ) + width;
              fixed edgeLerp = step(factor, color.a) * saturate((width - factor) * 16 / softness);
              
              color.rgb = lerp(_DissolveColor.rgb, color.rgb * fixed3(factor, factor, factor), factor);

//	              color.rgb = lerp(color.rgb, fixed3(factor, factor, factor), factor);
              color.a = factor;
              
              color.a *= saturate((factor) * 32 / softness);
	
			    return color;
				
			}
		ENDCG
		}
	}
}