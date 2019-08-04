Shader "UIForia/Standard"
{
    Properties {
      //  _MainTex ("Texture", 2D) = "white" {}
       // _MaskTexture ("Mask", 2D) = "white" {}
       //// _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
       // _Radius ("Radius",  Range (1, 200)) = 0
       // [Toggle] _InvertMask ("Invert Mask",  Int) = 0
    }
    SubShader {
        Tags {
         "RenderType"="Transparent"
         "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend One OneMinusSrcAlpha
         ZTest Off
         ZClip Off
        // this stencil setting solves self-blending
        // does mean we have to issue the draw call twice probably
        // if we want to reset the stencil
//        Stencil {
//            Ref 0
//            Comp Equal
//            Pass IncrSat 
//            Fail IncrSat
//        }
        Pass {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE

            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
            #include "./UIForiaSDFUtil.cginc"
            
            sampler2D _MainTex;
            sampler2D _MainTexture;
            sampler2D _MaskTexture;
            sampler2D _FontTexture;
            sampler2D _Texture;
            
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _MaskSoftness;
            float _InvertMask;
             
            float _FontScaleX;
            float _FontScaleY;
            float4 _FontScales;
            float4 _FontTextureSize;
            
            float4 _ColorData[BATCH_SIZE];
            float4 _ObjectData[BATCH_SIZE];
            float4x4 _TransformData[BATCH_SIZE];
                                                          
            #define Vert_BorderSize 0
            

                       
            #define _FontGradientScale _FontScales.x
            #define _FontScaleRatioA _FontScales.y
            #define _FontScaleRatioB _FontScales.z
            #define _FontScaleRatioC _FontScales.w
            
            #define _FontTextureWidth _FontTextureSize.x
            #define _FontTextureHeight _FontTextureSize.y
            
            #define Vert_BorderRadii objectInfo.z
            #define Vert_BorderSizeColor v.texCoord1
            #define Vert_PackedSize objectInfo.y
            
            #define Vert_CharacterScale v.texCoord1.x
            #define Vert_ShapeType objectInfo.x
            #define Vert_CharacterPackedOutline objectInfo.y
            #define Vert_CharacterPackedUnderlay objectInfo.z
            #define Vert_CharacterWeight objectInfo.w
                    
            #define PaintMode_Color 1 << 0
            #define PaintMode_Texture 1 << 1
            #define PaintMode_TextureTint 1 << 2
            #define PaintMode_LetterBoxTexture 1 << 3
            
            #define Frag_SDFSize i.texCoord1.xy
            #define Frag_SDFBorderRadii i.texCoord1.z
            #define Frag_SDFStrokeWidth i.texCoord1.w
            #define Frag_SDFCoords i.texCoord0.zw
            #define Frag_ShapeType i.texCoord2.x
            #define Frag_BorderColors i.texCoord3
                    
            float2 UnpackSize(float packedSize) {
                uint input = asuint(packedSize);
                uint high = (input >> 16) & (1 << 16) - 1;
                uint low =  input & 0xffff;
                return float2(high / 10, low / 10);
            }
            
            v2f vert (appdata v) {
                v2f o;
                                
                int objectIndex = (int)v.texCoord1.w; // can be a byte, consider packing this if needed

                float4 objectInfo = _ObjectData[objectIndex];
                float4x4 transform = _TransformData[objectIndex];
                
                int shapeType = objectInfo.x;
                
                half2 size = UnpackSize(Vert_PackedSize);

                v.vertex = mul(transform, float4(v.vertex.xyz, 1));
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texCoord0 = v.texCoord0;
                o.color = _ColorData[objectIndex];

                if(shapeType != 1) {
                    o.vertex = SDFPixelSnap(o.vertex); // pixel snap is bad for text rendering
                    o.texCoord1 = float4(size.x, size.y, Vert_BorderRadii, Vert_BorderSize);
                    o.texCoord2 = float4(shapeType, 0, 0, 0);
                    o.texCoord3 = Vert_BorderSizeColor;
                }
                else {             
                    _FontScaleX = 1;
                    _FontScaleY = 1;
                    
                    float weight = Vert_CharacterWeight; 

                    fixed4 unpackedOutline = UnpackColor(asuint(Vert_CharacterPackedOutline));
                    float outlineWidth = unpackedOutline.x;
                    float outlineSoftness = unpackedOutline.y;
                    
                    // todo -- glow
                    
                     fixed4 unpackedUnderlay = UnpackColor(asuint(Vert_CharacterPackedUnderlay));
                     
                     fixed underlayX = 1; //(unpackedUnderlay.x * 2) - 1;
                     fixed underlayY = 1; //(unpackedUnderlay.y * 2) - 1;
                     fixed underlayDilate = 1; //(unpackedUnderlay.z * 2) - 1;
                     fixed underlaySoftness = 1;//unpackedUnderlay.w;
                    
                     // scale stuff can be moved to cpu, alpha clip & bias too
                     float2 pixelSize = o.vertex.w;
                    
                     pixelSize /= float2(_FontScaleX, _FontScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                     float scale = rsqrt(dot(pixelSize, pixelSize));
                     scale *= abs(Vert_CharacterScale) * _FontGradientScale * 1.5;

                     float underlayScale = scale;
                     underlayScale /= 1 + ((underlaySoftness * _FontScaleRatioC) * underlayScale);
                     float underlayBias = (0.5 - weight) * underlayScale - 0.5 - ((underlayDilate * _FontScaleRatioC) * 0.5 * underlayScale);
                    
                     float2 underlayOffset = float2(
                         -(underlayX * _FontScaleRatioC) * _FontGradientScale / _FontTextureWidth,
                         -(underlayY * _FontScaleRatioC) * _FontGradientScale / _FontTextureHeight
                     );
                                    
                     float bias = (0.5 - weight) + (0.5 / scale);
                     float alphaClip = (1.0 - outlineWidth * _FontScaleRatioA - outlineSoftness * _FontScaleRatioA);
                     
                     alphaClip = alphaClip / 2.0 - ( 0.5 / scale) - weight;
                    
                     o.texCoord1 = float4(alphaClip, scale, bias, weight);
                     o.texCoord2 = float4(ShapeType_Text, outlineWidth, outlineSoftness, 0);
                     o.texCoord3 = float4(underlayOffset, underlayScale, underlayBias);

                }
                
                // todo -- more unpacking can be done in the vertex shader
                
                return o;
            }            
            
            inline fixed4 ComputeColor(float4 packedColor, float2 texCoord) {
            
                uint colorMode = packedColor.b;
                
                int useColor = (colorMode & PaintMode_Color) != 0;
                int useTexture = (colorMode & PaintMode_Texture) != 0;
                int tintTexture = (colorMode & PaintMode_TextureTint) != 0;
                int letterBoxTexture = (colorMode & PaintMode_LetterBoxTexture) != 0;
                
                fixed4 bgColor = UnpackColor(asuint(packedColor.r));
                fixed4 tintColor = UnpackColor(asuint(packedColor.g));
                fixed4 textureColor = tex2D(_MainTexture, texCoord);

                bgColor.rgb *= bgColor.a;
                tintColor.rgb *= tintColor.a;
                textureColor.rgb *= textureColor.a;
                
                textureColor = lerp(textureColor, textureColor + tintColor, tintTexture);
                
                if (useTexture && letterBoxTexture && texCoord.y < 0 || texCoord.y > 1) {
                    return bgColor; // could return a letterbox color instead
                }
                
                if(useTexture && useColor) {
                    return lerp(textureColor, bgColor, 1 - textureColor.a);
                }
                                
                return lerp(bgColor, textureColor, useTexture);
            }
            
            inline fixed4 GetTextColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
                half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
                half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));
            
                faceColor.rgb *= faceColor.a;
                outlineColor.rgb *= outlineColor.a;
            
                faceColor = lerp(faceColor, outlineColor, outlineAlpha);
            
                faceColor *= faceAlpha;
            
                return faceColor;
            }
 
            fixed4 frag (v2f i) : SV_Target {            
                
                // todo -- use color mode effectively
                
                fixed4 mainColor = ComputeColor(i.color, i.texCoord0.xy);
                mainColor.rgb *= mainColor.a;
                                
                if(Frag_ShapeType != ShapeType_Text) {

                    SDFData sdfData;
                    
                    sdfData.uv = Frag_SDFCoords;
                    sdfData.size = Frag_SDFSize;
                    sdfData.radius = GetBorderRadius(Frag_SDFCoords, Frag_SDFBorderRadii);
                    sdfData.strokeWidth = Frag_SDFStrokeWidth;
                    
                    fixed4 contentColor = mainColor;
                    fixed4 borderColor = fixed4(mainColor.rgb, 0);//GetBorderColor(Frag_SDFCoords, contentColor, Frag_BorderColors);

                    // we get bad blending at the edges of our SDF, this ensure rects are not smooth stepped                                       
//                     mainColor = lerp(mainColor, SDFColor(sdfData, borderColor, contentColor), 1);//sdfData.radius != 0);
                     mainColor = SDFColor(sdfData, borderColor, contentColor);
                     mainColor.rgb *=  mainColor.a;
                     return mainColor;
                }
                
                float outlineWidth = i.texCoord2.y;
                float outlineSoftness = i.texCoord2.z;
                float c = tex2D(_FontTexture, i.texCoord0.zw).a;

//                clip(c - i.texCoord1.x);
                float scaleRatio = _FontScaleRatioA;
                
                float scale	= i.texCoord1.y;
			    float bias = i.texCoord1.z;
			    float weight = i.texCoord1.w;
			    float sd = (bias - c) * scale;
			    
                float outline = (outlineWidth * scaleRatio) * scale;
			    float softness = (outlineSoftness * scaleRatio) * scale;

                fixed4 faceColor = UnpackColor(asuint(i.color.r));
			    fixed4 outlineColor = UnpackColor(asuint(i.color.g));
                fixed4 underlayColor = UnpackColor(asuint(i.color.b));
                fixed4 glowColor = UnpackColor(asuint(i.color.a));
                
                underlayColor.rgb *= underlayColor.a;
                faceColor.rgb *= faceColor.a;
                outlineColor.rgb *= outlineColor.a;
                glowColor.rgb *= glowColor.a;
                
                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);
              
                #define underlayOffset i.texCoord3.xy
                #define underlayScale i.texCoord3.z
                #define underlayBias i.texCoord3.w
						    
						    // todo -- pull underlay into a seperate shader
              //  float d = tex2D(_FontTexture, i.texCoord0.xy + underlayOffset).a * underlayScale;
              //  faceColor += underlayColor * saturate(d - underlayBias) * (1 - faceColor.a);
                
                return faceColor;// * i.color.a; 
                
             
                
                // clip(textureColor.a - 0.01);
                // fixed maskAlpha = saturate(tex2D(_MaskTexture, i.texCoord0.xy).a / _MaskSoftness);
                // maskAlpha = lerp(1 - maskAlpha, maskAlpha, _InvertMask);
                // mainColor.a *= maskAlpha;
                // return textureColor;// * fixed4(bgColor.rgb, 1) * 2;

            }

            ENDCG
        }
    }
}
