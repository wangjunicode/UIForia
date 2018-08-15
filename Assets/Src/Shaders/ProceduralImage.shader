Shader "UI/Procedural UI Image"
{
	Properties
	{
		[PerRendererData]_MainTex ("Base (RGB)", 2D) = "white" {}
		_Width("width", float) = 100
		_Height("height", float) = 100
		_BorderWidth("borderWidth", float) = 0.1
		_Radius("radius", Vector) = (0,0,0,0)
		_ContentRect("contentRect", Vector) = (0, 0, 0, 0)
		_ContentColor("contentColor", Color) = (1, 1, 1, 1)
		_LineWeight("line weight", float) = 0
		_PixelWorldScale("Pixel world scale", float) = 1
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
// Upgrade NOTE: excluded shader from DX11; has structs without semantics (struct appdata_t members contenttrWorld,contentblWorld)
#pragma exclude_renderers d3d11
			#pragma vertex vert
			#pragma fragment frag
			//#pragma exclude_renderers gles3 metal d3d11_9x xbox360 xboxone ps3 ps4 psp2 
			#include "UnityCG.cginc"
			#include "UnityUI.cginc"
			
			//#pragma target 3.0
			
			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;

			};

			struct v2f
			{
				float4 vertex   : POSITION;
				fixed4 color    : COLOR;
				half2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;

			};
			
			fixed4 _TextureSampleAdd;
	
			bool _UseClipRect;
			float4 _ClipRect;

			bool _UseAlphaClip;
			
			half _Width;
			half _Height;
			half _PixelWorldScale;
			half4 _Radius;
			half4 _ContentColor;
			half4 _ContentRect;
			half _LineWeight;
			float _BorderWidth;
			sampler2D _MainTex;
			
			v2f vert(appdata_t IN){
				v2f OUT;
				OUT.worldPosition = IN.vertex;
				OUT.vertex = UnityObjectToClipPos(OUT.worldPosition);
				_Width = _Width - 1;
				_Height = _Height - 1;
				OUT.texcoord = IN.texcoord * float2(_Width, _Height);
				#ifdef UNITY_HALF_TEXEL_OFFSET
				OUT.vertex.xy += (_ScreenParams.zw-1.0)*float2(-1,1);
				#endif
				OUT.color = IN.color*(1+_TextureSampleAdd);
				return OUT;
			}

			half visible(half2 pos,half4 r){
				half4 p = half4(pos,_Width-pos.x,_Height-pos.y);
				half v = min(min(min(p.x,p.y),p.z),p.w);
				bool4 b = bool4(all(p.xw<r[0]),all(p.zw<r[1]),all(p.zy<r[2]),all(p.xy<r[3]));
				half4 vis = r-half4(length(p.xw-r[0]),length(p.zw-r[1]),length(p.zy-r[2]),length(p.xy-r[3]));
				half4 foo = min(b*max(vis,0),v)+(1-b)*v;
				v = any(b)*min(min(min(foo.x,foo.y),foo.z),foo.w)+v*(1-any(b));
				return v;
			}
			
			half insideRect(float2 input, half2 topRight, half2 bottomLeft) {
			    half2 retn = step(bottomLeft, input) - step(topRight, input);
			    return retn.x * retn.y;
			}
			
			fixed4 frag (v2f IN) : SV_Target {
				half4 color = IN.color;
//                half2 topRight = half2(_ContentRect.x + _ContentRect.z, _ContentRect.y);
//                half2 bottomLeft = half2(_ContentRect.y + _ContentRect.w, _ContentRect.x);
//                half insideContentRect = insideRect(IN.worldPosition.xy, topRight, bottomLeft);
////                half minX = _ContentRect.x;
////                half minY = minX;
////                half maxX = 1 - 5;
////                
//               
//               if(insideContentRect) {
//                    color = _ContentColor;
//                }
//                
                half2 texCoord = IN.texcoord;
                float maxX = 95;//1 - _BorderWidth;
                float minX = 5; //_BorderWidth;
                float minY = 5;//minX / (_Width / _Height);
                float maxY = 95;//maxX / (_Width / _Height);
                
                if(texCoord.x < maxX && texCoord.x >= minX && texCoord.y < maxY && texCoord.y > minY) {
                    color = _ContentColor;
                }
                else {
                    color = half4(0, 0, 0, 1);
                }
                return color;
//                
//				if (_UseClipRect)
//					color *= UnityGet2DClipping(IN.worldPosition.xy, _ClipRect);
//				
//				if (_UseAlphaClip)
//					//clip (color.a - 0.001);
//					
//				if(_LineWeight>0){
//					half l = (_LineWeight+1/_PixelWorldScale)/2;
//					color.a *= saturate((l-distance(visible(IN.texcoord,_Radius),l))*_PixelWorldScale);
//				}
//				else{
//					color.a *= saturate(visible(IN.texcoord,_Radius)*_PixelWorldScale);
//				}
//				
//				return color;
			}
			ENDCG
		}
	}
}

