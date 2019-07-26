Shader "UIForia/Standard"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTexture ("Mask", 2D) = "white" {}
        _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle] _InvertMask ("Invert Mask",  Int) = 0
    }
    SubShader {
        Tags {
         "RenderType"="Transparent"
         "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend One OneMinusSrcAlpha
         
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
            sampler2D _MaskTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _MaskSoftness;
            float _InvertMask;
             
            // todo -- set as vector4s instead of floats
            sampler2D _FontTexture;
            float _FontScaleX;
            float _FontScaleY;
        
            float4 _FontScales[BATCH_SIZE];
            float4 _FontWeightsAndSizes[BATCH_SIZE];
            float4 _FontTextData[BATCH_SIZE];
            float4 _ColorData[BATCH_SIZE];
            float4 _ObjectData[BATCH_SIZE];
            float4 _TransformData[BATCH_SIZE];
                                                          
            #define Vert_BorderSize 0
            //v.texCoord1.x
            #define Vert_BorderRadii objectInfo.y
            #define Vert_BorderSizeColor v.texCoord1
                       
            #define _FontGradientScale fontScales.x
            #define _FontScaleRatioA fontScales.y
            #define _FontScaleRatioB fontScales.z
            #define _FontScaleRatioC fontScales.w
            #define _FontWeightNormal fontWeightsAndSizes.x
            #define _FontWeightBold fontWeightsAndSizes.y
            #define _FontTextureWidth fontWeightsAndSizes.z
            #define _FontTextureHeight fontWeightsAndSizes.w
            
            // todo -- move to texCoord2
            #define _CharacterScale fontTextData.x
            #define _CharacterPackedOutline fontTextData.z
            #define _CharacterPackedUnderlay fontTextData.w
                    
            #define PaintMode_Color 1 << 0
            #define PaintMode_Texture 1 << 1
            #define PaintMode_TextureTint 1 << 2
            
            #define Frag_SDFSize i.texCoord1.xy
            #define Frag_SDFBorderRadii i.texCoord1.z
            #define Frag_SDFStrokeWidth i.texCoord1.w
            
            #define Frag_ShapeType i.texCoord2.x
                    
            inline float4 UnpackUnderlay(float scale, float weight, float4 fontScales, float4 fontTextData, float4 fontWeightsAndSizes) {
                fixed4 unpackedUnderlay = UnpackColor(asuint(_CharacterPackedUnderlay));
             
                fixed underlayX = (unpackedUnderlay.x * 2) - 1;
                fixed underlayY = (unpackedUnderlay.y * 2) - 1;
                fixed underlayDilate = (unpackedUnderlay.z * 2) - 1;
                fixed underlaySoftness = unpackedUnderlay.w;
             
                float underlayScale = scale;
                underlayScale /= 1 + ((underlaySoftness * _FontScaleRatioC) * underlayScale);
                float underlayBias = (0.5 - weight) * underlayScale - 0.5 - ((underlayDilate * _FontScaleRatioC) * 0.5 * underlayScale);
            
                float2 underlayOffset = float2(
                    -(underlayX * _FontScaleRatioC) * _FontGradientScale / _FontTextureWidth,
                    -(underlayY * _FontScaleRatioC) * _FontGradientScale / _FontTextureHeight
                );
                
                return float4(underlayOffset, underlayScale, underlayBias);
            }
            
            v2f vert (appdata v) {
                v2f o;
                                
                int transformIndex = (int)v.texCoord0.z;
                int objectIndex = (int)v.texCoord0.w;

                float4 objectInfo = _ObjectData[objectIndex];
                float4 transform = _TransformData[transformIndex];
                o.color = _ColorData[objectIndex];
                
                int shapeType = objectInfo.x;
                
                half2 size = transform.zw;// UnpackToHalf2(objectInfo.z);
                half2 pos = transform.xy;
                half2 scale =  float2(1, 1); 
                //transform.zw;
                float rotation = objectInfo.w;
                
                float3x3 m = mul(TRS2D(0, scale, rotation), TRS2D(-0.5 * size, scale, 0));
                float3 outputVertXY = mul((m), float3(v.vertex.xy, 1));
                
                // o.vertex = UnityObjectToClipPos(float3(outputVertXY.xy, v.vertex.z));
                o.vertex = UnityObjectToClipPos(v.vertex); //float3(outputVertXY.xy, v.vertex.z));
                
                if(shapeType != ShapeType_Text) {
                    //o.vertex = SDFPixelSnap(o.vertex); // pixel snap is bad for text rendering
                    o.texCoord0 = float4(v.texCoord0.xy, 0, 0);
                    o.texCoord1 = float4(size, Vert_BorderRadii, Vert_BorderSize);
                    o.texCoord2 = float4(shapeType, 0, 0, 0);
                    o.texCoord3 = Vert_BorderSizeColor;
                }
                else {             
                    int spanIndex = (int)v.texCoord1.z;
                    float4 fontScales = _FontScales[spanIndex];    
                    float4 fontWeightsAndSizes = _FontWeightsAndSizes[spanIndex];
                    float4 fontTextData = _FontTextData[spanIndex];
                               
                    _FontScaleX = 1;
                    _FontScaleY = 1;
                    
                    fixed4 unpackedOutline = UnpackColor(asuint(_CharacterPackedOutline));
                    float outlineWidth = unpackedOutline.x;
                    float outlineSoftness = unpackedOutline.y;
                    
                    // todo -- glow
                    
                    // scale stuff can be moved to cpu
                    float2 pixelSize = o.vertex.w;
                    
                    pixelSize /= float2(_FontScaleX, _FontScaleY) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                    float scale = rsqrt(dot(pixelSize, pixelSize));
                    scale *= abs(_CharacterScale) * _FontGradientScale * 1.5;
                    
                    float weight = _FontWeightNormal; 
                    float bias = (0.5 - weight) + (0.5 / scale);
                    float alphaClip = (1.0 - outlineWidth * _FontScaleRatioA - outlineSoftness * _FontScaleRatioA);
                     
                    alphaClip = alphaClip / 2.0 - ( 0.5 / scale) - weight;
                    
                    o.texCoord0 = float4(v.texCoord0.xy, 0, 0);
                    o.texCoord1 = float4(alphaClip, scale, bias, weight);
                    o.texCoord2 = float4(shapeType, outlineWidth, outlineSoftness, _FontScaleRatioA);
                    o.texCoord3 = UnpackUnderlay(scale, weight, fontScales, fontTextData, fontWeightsAndSizes);
                    
                }
                return o;
            }            
            
            inline fixed4 ComputeColor(float4 packedColor, float2 texCoord) {
            
                return packedColor; // temp
                uint colorMode = packedColor.b;

                int useColor = (colorMode & PaintMode_Color) != 0;
                int useTexture = (colorMode & PaintMode_Texture) != 0;
                int tintTexture = (colorMode & PaintMode_TextureTint) != 0;
                
                fixed4 bgColor = UnpackColor(asuint(packedColor.r));
                fixed4 tintColor = UnpackColor(asuint(packedColor.g));
                fixed4 textureColor = tex2D(_MainTex, texCoord);

                bgColor.rgb *= bgColor.a;
                tintColor.rgb *= tintColor.a;
                textureColor.rgb *= textureColor.a;
                
                textureColor = lerp(textureColor, textureColor + tintColor, tintTexture);
                
                if(useTexture && useColor) {
                    return lerp(textureColor, bgColor, 1 - textureColor.a);
                }
                
                return lerp(bgColor, textureColor, useTexture);
            }
            
            fixed4 GetTextColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness) {
                half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
                half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));
            
                faceColor.rgb *= faceColor.a;
                outlineColor.rgb *= outlineColor.a;
            
                faceColor = lerp(faceColor, outlineColor, outlineAlpha);
            
                faceColor *= faceAlpha;
            
                return faceColor;
            }
 
            fixed4 frag (v2f i) : SV_Target {            
                
                if(Frag_ShapeType != ShapeType_Text) {
                
                    SDFData sdfData;
                    sdfData.uv = i.texCoord0.xy;
                    sdfData.size = Frag_SDFSize;
                    sdfData.radius = GetBorderRadius(i.texCoord0.xy, Frag_SDFBorderRadii);
                    sdfData.strokeWidth = Frag_SDFStrokeWidth;
                    sdfData.shapeType = ShapeType_RectLike;
                    fixed4 contentColor = UnpackColor(asuint(i.color.x));//fixed4(0, 0, 0, 1);
                    fixed4 borderColor = GetBorderColor(i.texCoord0.xy, contentColor, i.texCoord3);
                    fixed4 alpha = contentColor;    

                    // we get bad blending at the edges of our SDF, this ensure rects are not smooth stepped                                       
                    if(sdfData.radius != 0) {
                        alpha = SDFColor(sdfData, borderColor, contentColor);
                        alpha.rgb *= alpha.a;
                    }

                    fixed4 mainColor = ComputeColor(i.color, i.texCoord0.xy);
                   // mainColor.a = alpha.a;
                    //mainColor.rgb *=  mainColor.a;
                    return mainColor;
                }
                
                float outlineWidth = i.texCoord2.y;
                float outlineSoftness = i.texCoord2.z;
                float c = tex2D(_FontTexture, i.texCoord0.xy).a;

               // clip(c - i.texCoord1.x);
                float scaleRatio = i.texCoord2.w;
                
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
                outlineColor.rgb *= outlineColor.a;
                faceColor.rgb *= faceColor.a;
                glowColor.rgb *= glowColor.a;
                
                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);
              
                #define underlayOffset i.texCoord3.xy
                #define underlayScale i.texCoord3.z
                #define underlayBias i.texCoord3.w
						    
                float d = tex2D(_FontTexture, i.texCoord0.xy + underlayOffset).a * underlayScale;
                faceColor += underlayColor * saturate(d - underlayBias) * (1 - faceColor.a);
                
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
