Shader "UIForia/UIForiaText"
{
    Properties
    {
        _FaceTex ("Face Texture", 2D) = "white" {}
        _OutlineTex	("Outline Texture", 2D) = "white" {}
        _MainTex ("Texture", 2D) = "white" {}
        // required for UI.Mask
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
        
        _FaceUVSpeedX ("Face UV Speed X", Range(-5, 5)) = 0.0
	    _FaceUVSpeedY ("Face UV Speed Y", Range(-5, 5)) = 0.0
	    _OutlineUVSpeedX ("Outline UV Speed X", Range(-5, 5)) = 0.0
	    _OutlineUVSpeedY ("Outline UV Speed Y", Range(-5, 5)) = 0.0
	  
    }
    SubShader
    {
        Tags { 
            "RenderType"="Transparent" 
            "UIFoira::SupportClipRectBuffer"="TEXCOORD1.x"
            "UIForia::SupportMaterialBuffer"="8"
        }
        LOD 100

        Pass {
            Cull off
            ColorMask [_ColorMask]
            Blend One OneMinusSrcAlpha
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
                float4 uv0 : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                // float4 underlay : TEXCOORD2;
                fixed4 color : COLOR;
            };

            struct v2f {
                float4 uv : TEXCOORD0; // zw unused?
                fixed4 color : COLOR;
                nointerpolation half4 fontInfo : TEXCOORD1;
                half4 textures : TEXCOORD2;
                half4 worldPosition : TEXCOORD3;
                nointerpolation half4 underlay : TEXCOORD4;
                nointerpolation fixed4 outline : TEXCOORD5; // zw unused?
                nointerpolation float4 glow : TEXCOORD6;
            };
            
            float4 _gUIForiaMaskUVBuffer[32];
            float4 _gUIForiaClipRectBuffer[128];
            float4 _gUIForiaGradientBuffer[128];
            float4 _gUIForiaTextDataBuffer[128];
            float4 _gUIForiaShapeDataBuffer[128];

            fixed4 _TextureSampleAdd;
            fixed4 _Color;
            
            sampler2D _MainTex;
            sampler2D _FaceTex;
            sampler2D _OutlineTex;
            
            float4 _MainTex_ST;
            float4 _FaceTex_ST;
            float4 _OutlineTex_ST;
            
            float _FaceUVSpeedX;
            float _FaceUVSpeedY;
            float _OutlineUVSpeedX;
            float _OutlineUVSpeedY;
            
            float4 _OverflowClippers[12];            
                       
            #define Vert_PackedUV v.uv0.z
            

            int _ObjectOffset;
            
            v2f vert (appdata v, out float4 vertex : SV_POSITION) {
                v2f o;
              
    
                float4 screenPos = ComputeScreenPos(v.vertex);
                vertex = UnityObjectToClipPos(v.vertex);
                o.worldPosition = screenPos;
                o.uv.xy = TRANSFORM_TEX(v.uv0.xy, _MainTex);
                o.color = fixed4(0, 0, 0, 1); //v.color;
                              
			    uint packedOutlineAndAlpha = asuint(v.uv0.z);
			    uint packedScaleAndWeight = asuint(v.uv0.w);
			    
			    fixed outlineWidth = ((packedOutlineAndAlpha >> 0) & 0xff) / 255.0;
			    fixed outlineSoftness = ((packedOutlineAndAlpha >> 8) & 0xff) / 255.0;
			    float alphaClip = remap(((packedOutlineAndAlpha >> 16) & 0xffff), 0, 65535, 0, 1);
			    
			    half weight = remap(((packedScaleAndWeight >> 0) & 0xffff), 0, 65535, 0, 32);
			    half vertScale = remap(((packedScaleAndWeight >> 16) & 0xffff), 0, 65535, 0, 32);
			    
			    float2 pixelSize = vertex.w;
                pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float scale = rsqrt(dot(pixelSize, pixelSize));
                scale *= vertScale;
                               
                float bias = (0.5 - weight) + (0.5 / scale);
                
                // float2 textureUV = UnpackUV(v.uv0.z);
			    float2 faceUV = float2(0, 0); //TRANSFORM_TEX(textureUV, _FaceTex);
			    float2 outlineUV = float2(0, 0); //TRANSFORM_TEX(textureUV, _OutlineTex);
 
 float4 underlay = float4(0, 0, 0, 0);
                uint packedDilateSoftness = 0; //asuint(v.underlay.z);
                half underlayDilate = 0; //v.underlay.z; // remap(((packedDilateSoftness >> 0) & 0xff) * 1.0, 0, 65535, 0, 1);
                half underlaySoftness = 0; //v.underlay.w; //remap(((packedDilateSoftness >> 16) & 0xff) * 1.0, 0, 65535, 0, 1);
 
			    float bScale = scale;
			    bScale /= 1 + (underlaySoftness * bScale);
	
			    float bBias = (0.5 - weight) * bScale - 0.5 - (underlayDilate * 0.5 * bScale);
			    o.underlay = half4(0, 0, bScale, bBias);
 
  			    uint packedGlow = asuint(v.uv1.w);
  			    fixed glowOffset = remap(((packedGlow >> 0) & 0xff) * 1.0, 0, 255, -1, 1);
			    fixed glowInner = ((packedGlow >> 8) & 0xff) / 255.0;
			    fixed glowOuter = ((packedGlow >> 16) & 0xff) / 255.0;
			    fixed glowPower = ((packedGlow >> 24) & 0xff) / 255.0;
 
			    alphaClip = alphaClip / 2.0 - (0.5 / scale) - weight;
 
                o.fontInfo = half4(alphaClip, scale, bias, weight);
                o.outline = fixed4(outlineWidth, outlineSoftness, 0, 0);
                o.textures = half4(v.uv0.xy, outlineUV);
                o.glow = fixed4(glowOffset, glowInner, glowOuter, glowPower);
                
                return o;
            }


            fixed4 frag (v2f i, UNITY_VPOS_TYPE screenPos : VPOS) : SV_Target {
                half opacity = 1;
                half scale	= i.fontInfo.y;
                half bias = i.fontInfo.z;
                half weight = i.fontInfo.w;
                float c = tex2D(_MainTex, i.uv.xy).a;
                
                // this is only when underlay is OFF!
                clip(c - i.fontInfo.x);
                
                half sd = (bias - c) * scale;
                half outline =  i.outline.x * scale; 
                half softness = i.outline.y * scale;
                
                fixed4 outlineColor = fixed4(1, 1, 1, 1);
                fixed4 underlayColor = fixed4(1, 1, 1, 1);
                fixed4 glowColor = fixed4(1, 0, 0, 1);
                
                fixed4 faceColor = i.color; //UIForiaColorSpace(i.color);
       			
       			faceColor *= tex2D(_FaceTex, i.textures.xy + float2(_FaceUVSpeedX, _FaceUVSpeedY) * _Time.y);
       			outlineColor *= tex2D(_OutlineTex, i.textures.zw + float2(_OutlineUVSpeedX, _OutlineUVSpeedY) * _Time.y);
       			
                // faceColor.a *= opacity;

                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);
               	
               	// #if USE_UNDERLAY
               	half underlayScale = i.underlay.z;
               	half underlayBias = i.underlay.w;
               	
               	// float2 underlayUV = i.uv.xy + float2(i.underlay.xy);
              	// float d = tex2D(_MainTex, underlayUV).a * underlayScale;
			    // faceColor += underlayColor * saturate(d - underlayBias) * (1 - faceColor.a);
			    
			    glowColor = GetGlowColor(sd, scale, glowColor, i.glow.x, i.glow.y, i.glow.z, i.glow.w);
			    faceColor.rgb += glowColor.rgb * glowColor.a;
                screenPos.y = _ScreenParams.y - screenPos.y;
                fixed4 vertexColor = i.color; //UIForiaColorSpace(i.color);
                
                #if UNITY_UI_ALPHACLIP
    			    clip(faceColor.a - 0.001);
		        #endif

                return faceColor;
            }
            
            ENDCG
        }
    }
}
