Shader "UIForia/BatchedTransparent" {
    
    Properties {  
      _StencilRef ("Stencil ID", Float) = 1
    }
    
    SubShader {

        Tags { "RenderType"="Transparent" "DisableBatching"="True" }
        Cull Back 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ColorMask RGBA
        
//        Stencil {
//            Ref [_StencilRef]
//            Comp Equal
//        }
//         
        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma enable_d3d11_debug_symbols
           #pragma debug
           
           #include "UnityCG.cginc"
           #include "UIForiaInc.cginc"
           
           #define antialias 2
           
           uniform sampler2D _MainTex;
           uniform sampler2D _globalGradientAtlas;
           uniform sampler2D _globalFontTexture;
           
           uniform float _globalGradientAtlasSize;
           
           uniform float4 _globalFontData1; // WeightNormal, WeightBold, TextureSizeX, TextureSizeY
           uniform float4 _globalFontData2; // GradientScale, ScaleRatioA, ScaleRatioB, ScaleRatioC
                    
           struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 flags : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD0;
                float4 uv : TEXCOORD1;
                float4 colorFlags : TEXCOORD2;
                fixed4 secondaryColor : COLOR1;
           };

           v2f FillVertex(appdata v, uint shapeType) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                // todo -- support scaling and repeating uvs
                //if(v.flags.x == ShapeType_Circle) {
                    o.uv = float4(v.uv.xy, 0, 0);
                //}
//                else {
//                    o.uv = float4((v.vertex.x - v.uv.x) / (v.uv.z + 1), (v.vertex.y + v.uv.y) / (v.uv.w + 1), 0, 0);
//                }
                
                o.color = v.color;
                o.flags = float4(shapeType, v.flags.yzw);
                
                uint fillFlags = (uint)v.flags.y;
                
                uint texFlag = (fillFlags & FillMode_Texture) != 0;
                uint gradientFlag = (fillFlags & FillMode_Gradient) != 0;
                uint tintFlag = (fillFlags & FillMode_Tint) != 0;
                uint gradientTintFlag = (fillFlags & FillMode_GradientTint) != 0;
                o.secondaryColor = fixed4(0, 0, 0, 0);
                o.colorFlags = float4(texFlag, gradientFlag, tintFlag, gradientTintFlag);
                return o;
           }
           
           v2f LineVertex(appdata input) {
               v2f o;
               
               uint flags = input.flags.y;
               
               #define prevNext input.uv2
               
               float2 prev = prevNext.xy;
               float2 next = prevNext.zw;
               float2 curr = input.vertex.xy;
               
               float strokeWidth = input.flags.w;
               
               float aa = strokeWidth < 2 ? 1 : antialias;
               
               int dir = GetByte0(flags) * 2 - 1; // remap [0, 1] to [-1, 1]
               uint isNear = GetByte1(flags);
               uint isCap = GetByte2(flags);
               
               float w = (strokeWidth * 0.5) + aa;
               
               float2 v0 = normalize(curr - prev);
               float2 v1 = normalize(next - curr);
               
               float2 n0 = float2(-v0.y, v0.x);
               float2 n1 = float2(-v1.y, v1.x);
               
               float2 miter = normalize(n0 + n1);
                        
               float miterLength = w / dot(miter, n1);
               float2 pos = float2(0, 0);
               o.color = input.color;
               o.uv = float4(w, w * dir, 0, 0);
               
               o.flags = float4(strokeWidth, w * dir, aa, curr.x == next.x || curr.y == next.y);

               if(isCap) {
                    if(isNear) {
                        pos = curr + w * v1 + dir * w * n1;
                    }
                    else {
                        pos = curr - w * v0 + dir * w * n0;
                    }
               }
               else {
                   pos = curr + (miter * miterLength * dir);
               }
               
               // todo -- support flags for pushing stroke to inside or outside of shape
               // for pushing stroke outwards: pos.xy - (n1 * strokeWidth * 0.5)
               // for pushing stroke inwards: pos.xy + (n1 * strokeWidth * 0.5)
               o.secondaryColor = fixed4(0, 0, 0, 0);
               o.vertex = UnityObjectToClipPos(float3(pos, input.vertex.z));
               return o;
           }
           
           fixed4 LineFragment(v2f i) {
               return i.color;
               float thickness = i.flags.x;
               float aa = i.flags.z;
               float w = (thickness * 0.5) - aa;

               float d = abs(i.uv.y) - w;
               
               if(d <= 0) {
                   return i.color;
               }

               d /= aa;
               float threshold = 1;
               float afwidth = length(float2(ddx(d), ddy(d)));
               float alpha = smoothstep(threshold - afwidth, threshold + afwidth, d);
               return fixed4(i.color.rgb, i.color.a * (1 - alpha));
           }
           
           fixed4 FillFragment(v2f i) {
                #define ShapeType i.flags.x
                #define GradientId i.flags.z
                #define GradientDirection i.flags.w
                                                
                float t = lerp(i.uv.x, 1 - i.uv.y, GradientDirection);
                float y = GetPixelInRowUV(GradientId, _globalGradientAtlasSize);

                fixed4 color = i.color;
                fixed4 textureColor = tex2D(_MainTex, i.uv);
                fixed4 gradientColor = tex2Dlod(_globalGradientAtlas, float4(t, y, 0, 0));
                fixed4 tintColor = lerp(White, color, i.colorFlags.z);                
                
                textureColor = lerp(color, textureColor, i.colorFlags.x);
                gradientColor = lerp(White, gradientColor, i.colorFlags.y);
                tintColor = lerp(tintColor, gradientColor, i.colorFlags.w);
                
                fixed4 tintedTextureColor = lerp(textureColor, textureColor * tintColor, tintColor.a);
            
                color = lerp(tintedTextureColor, gradientColor, 0);
                
                if(ShapeType > ShapeType_Path) {
                    float dist = length(i.uv.xy - 0.5);
                    float pwidth = length(float2(ddx(dist), ddy(dist)));
                    float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);                
                                        
                    color = fixed4(color.rgb, color.a * alpha);
                }
                             
                if(color.a - 0.001 <= 0) {
                    discard;
                }
                   
                return color;
           }
           
           fixed4 GetColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
                half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
                half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));
            
                faceColor.rgb *= faceColor.a;
                outlineColor.rgb *= outlineColor.a;
            
                faceColor = lerp(faceColor, outlineColor, outlineAlpha);
            
                faceColor *= faceAlpha;
            
                return faceColor;
           }
           
           #define gWeightNormal _globalFontData1.x
           #define gWeightBold _globalFontData1.y
           #define gFontTextureWidth _globalFontData1.z
           #define gFontTextureHeight _globalFontData1.w
           #define gGradientScale _globalFontData2.x
           #define gScaleRatioA _globalFontData2.y
           #define gScaleRatioB _globalFontData2.z
           #define gScaleRatioC _globalFontData2.w
           #define _ScaleX 1
           #define _ScaleY 1
           #define _FaceDilate 0
           #define _OutlineWidth input.uv2.y
           #define _OutlineSoftness input.uv2.z
           #define _GlowOuter 0
           #define _GlowOffset 0
           #define _UnderlayColor 0
           #define _UnderlaySoftness 0
           #define _UnderlayDilate 0
           #define _UnderlayOffsetX 0
           #define _UnderlayOffsetY 0
               
                    
           inline float4 ColorFromFloat( float v ) {
                float4 kEncodeMul = float4(1.0, 255.0, 65025.0, 160581375.0);
                float kEncodeBit = 1.0/255.0;
                float4 enc = kEncodeMul * v;
                enc = frac (enc);
                enc -= enc.yzww * kEncodeBit;
                return enc;
           }
           
           v2f TextVertex(appdata input) {
           
               float4 vPosition = UnityObjectToClipPos(input.vertex);
               float2 pixelSize = vPosition.w;
               
               pixelSize /= float2(_ScaleX, _ScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
               float scale = rsqrt(dot(pixelSize, pixelSize));
			   scale *= abs(input.uv.z) * gGradientScale * 1.5; 
			   
			   int bold = 0;
			   
			   float weight = lerp(gWeightNormal, gWeightBold, 0) / 4.0;
			   weight = (weight + _FaceDilate) * gScaleRatioA * 0.5;
			   
			   float bias =(.5 - weight) + (.5 / scale);
			   float alphaClip = (1.0 - _OutlineWidth * gScaleRatioA - _OutlineSoftness * gScaleRatioA);

			   alphaClip = min(alphaClip, 1.0 - _GlowOffset * gScaleRatioB - _GlowOuter * gScaleRatioB);
			   alphaClip = alphaClip / 2.0 - ( .5 / scale) - weight;
			   
			   float4 underlayColor = _UnderlayColor;
			   underlayColor.rgb *= underlayColor.a;

			   float bScale = scale;
			   bScale /= 1 + ((_UnderlaySoftness * gScaleRatioC) * bScale);
			   float bBias = (0.5 - weight) * bScale - 0.5 - ((_UnderlayDilate *  gScaleRatioC) * 0.5 * bScale);

			   float x = -(_UnderlayOffsetX *  gScaleRatioC) * gGradientScale / gFontTextureWidth;
			   float y = -(_UnderlayOffsetY *  gScaleRatioC) * gGradientScale / gFontTextureHeight;
			   float2 bOffset = float2(x, y);

               fixed4 c = ColorFromFloat(input.uv2.x) / 255;
               
			   v2f o;
			   o.vertex = vPosition;
               o.color = input.color;
               o.uv = input.uv;
               o.flags = float4(alphaClip, scale, bias, weight);
               o.colorFlags = float4(_OutlineWidth, _OutlineSoftness, input.uv2.x, 0);
               o.secondaryColor = c;// fixed4(1, 0, 0, 1);
               
			   return o;
           }
      

           fixed4 TextFragment(v2f input) {
           
               float c = tex2Dlod(_globalFontTexture, float4(input.uv.xy, 0, 0)).a;
               
               float scale = input.flags.y;
			   float bias = input.flags.z;
			   float weight	= input.flags.w;
               float sd = (bias - c) * scale;

               float outline = (input.colorFlags.x * gScaleRatioA) * scale;
               float softness = (input.colorFlags.y * gScaleRatioA) * scale;
               
               half4 faceColor = input.color;
               fixed4 outlineColor = input.secondaryColor; //input.colorFlags.z;//ColorFromFloat(input.colorFlags.z);
               //input.secondaryColor; //fixed4(0, 0, 0, 1); //input.colorFlags.x;
               
               faceColor.rgb *= input.color.rgb;
			   faceColor = GetColor(sd, faceColor, outlineColor, outline, softness);
			   
			   return faceColor * input.color.a;
           }
           
           v2f vert (appdata input) {
               return TextVertex(input);
               uint renderData = (uint)input.flags.x;
               int isFill = (renderData & 0xffff) == 1;
               uint shapeType = (renderData >> 16) & (1 << 16) - 1;
               if(isFill) {
                   return FillVertex(input, shapeType);
               }
               else 
               return LineVertex(input);
           }

           fixed4 frag (v2f i) : SV_Target {
               return TextFragment(i);

               if(i.flags.x == 1) {
                   return FillFragment(i);
               }
               else if(i.flags.x == 2) {
                   return TextFragment(i);
               }
               else {
                   return LineFragment(i);
               }
           }
           
           ENDCG
        }
    }
}
