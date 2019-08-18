Shader "UIForia/Standard"
{
    Properties {
        //  _MainTex ("Texture", 2D) = "white" {}
        // _MaskTexture ("Mask", 2D) = "white" {}
        //// _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        // _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle(UIFORIA_TEXTURE_CLIP)] _TextureClip ("Texture Clip",  Int) = 1
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
            #pragma shader_feature UIFORIA_TEXTURE_CLIP

            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE

            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
            #include "./UIForiaSDFUtil.cginc"
            
            sampler2D _MainTexture;
            sampler2D _MaskTexture;
            sampler2D _FontTexture;
            
            float4 _Color;
            
            float _FontScaleX;
            float _FontScaleY;
            float4 _FontScales;
            float4 _FontTextureSize;
            
            float4 _ColorData[BATCH_SIZE];
            float4 _MiscData[BATCH_SIZE];
            float4 _ObjectData[BATCH_SIZE];
            float4 _ClipUVs[BATCH_SIZE];
            float4 _ClipRects[BATCH_SIZE];
            float4x4 _TransformData[BATCH_SIZE];
            
            #define _FontGradientScale _FontScales.x
            #define _FontScaleRatioA _FontScales.y
            #define _FontScaleRatioB _FontScales.z
            #define _FontScaleRatioC _FontScales.w
            
            #define _FontTextureWidth _FontTextureSize.x
            #define _FontTextureHeight _FontTextureSize.y
            
            #define Vert_BorderRadii objectInfo.z
            #define Vert_PackedSize objectInfo.y
            #define Vert_BorderColors v.texCoord1.xy
            
            #define Vert_CharacterScale v.texCoord1.x
            #define Vert_ShapeType objectInfo.x
            #define Vert_CharacterPackedOutline objectInfo.y
            #define Vert_CharacterPackedUnderlay objectInfo.z
            #define Vert_CharacterWeight objectInfo.w
            
            #define Frag_SDFSize i.texCoord1.xy
            #define Frag_SDFBorderRadii i.texCoord1.z
            #define Frag_SDFStrokeWidth i.texCoord1.w
            #define Frag_SDFCoords i.texCoord0.zw
            #define Frag_ShapeType i.texCoord2.x
            #define Frag_BorderColors i.texCoord3
            #define Frag_BorderSize i.color.zw
            #define Frag_ColorMode i.texCoord2.y
            
         
            
            v2f vert (appdata v) {
                v2f o;
                
                int objectIndex = (int)v.texCoord1.w; // can be a byte, consider packing this if needed

                float4 objectInfo = _ObjectData[objectIndex];
                float4x4 transform = _TransformData[objectIndex];
                
                uint shapeType = objectInfo.x;
                uint colorMode = objectInfo.w;
                fixed4 colors = _ColorData[objectIndex];
                
                half2 size = UnpackSize(Vert_PackedSize);
                v.vertex = mul(transform, float4(v.vertex.xyz, 1));
                
                o.vertex = UnityObjectToClipPos(v.vertex);
                float4 screenPos = ComputeScreenPos(o.vertex);
                o.texCoord0 = v.texCoord0;
                o.color = colors;
                
                // this only works for 'flower' configuration meshes, not for quads. use a flag for the quad
                o.texCoord4 = float4(lerp(0, 1, v.texCoord0.x == 0.5 && v.texCoord0.y == 0.5), screenPos.xyw);
                
                if(shapeType != ShapeType_Text) {
                    // o.vertex = UIForiaPixelSnap(o.vertex); // pixel snap is bad for text rendering
                    o.texCoord1 = float4(size.x, size.y, Vert_BorderRadii, objectIndex);
                    o.texCoord2 = float4(shapeType, colorMode, 0, 0);
                    o.texCoord3 = _MiscData[objectIndex];
                }
                else {             
                    _FontScaleX = 1;
                    _FontScaleY = 1;
                    
                    float weight = Vert_CharacterWeight; 

                    fixed4 unpackedOutline = UnpackColor(asuint(Vert_CharacterPackedOutline));
                    float outlineWidth = 0; unpackedOutline.x;
                    float outlineSoftness = 0; // unpackedOutline.y;
                    
                    // todo -- glow
                    
                    fixed4 unpackedUnderlay = UnpackColor(asuint(Vert_CharacterPackedUnderlay));
                    
                    fixed underlayX = 1; //(unpackedUnderlay.x * 2) - 1;
                    fixed underlayY = 1; //(unpackedUnderlay.y * 2) - 1;
                    fixed underlayDilate = 1; //(unpackedUnderlay.z * 2) - 1;
                    fixed underlaySoftness = 1;//unpackedUnderlay.w;
                    
                    // scale stuff can be moved to cpu, alpha clip & bias too
                    float2 pixelSize = o.vertex.w;
                    pixelSize /= float2(1, 1) * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                    
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
          
            fixed4 frag (v2f i) : SV_Target {           
                // return fixed4(i.texCoord0.z, i.texCoord0.z, i.texCoord0.z, 1);
                
                float2 screenUV = i.texCoord4.yz / i.texCoord4.w;
                
                float4 clipRect = _ClipRects[(uint)i.texCoord1.w];
                float4 clipUvs = _ClipUVs[(uint)i.texCoord1.w];
                // todo -- returns cause branching here
                // get rid of text and we can get rid of branching
                fixed4 mainColor = ComputeColor(i.color.r, i.color.g, Frag_ColorMode, i.texCoord0.xy, _MainTexture);
                if(Frag_ShapeType != ShapeType_Text) {
                    
                    BorderData borderData = GetBorderData(Frag_SDFCoords, Frag_SDFSize, Frag_BorderColors, Frag_BorderSize, Frag_SDFBorderRadii, mainColor);
                    SDFData sdfData;
                    
                    sdfData.uv = Frag_SDFCoords;
                    sdfData.size = Frag_SDFSize;
                    sdfData.strokeWidth = borderData.size;
                    sdfData.radius = borderData.radius;
                    mainColor = SDFColor(sdfData, borderData.color, mainColor, i.texCoord4.x);
                  //  mainColor = UIForiaAlphaClipColor(mainColor, _MaskTexture, screenUV, clipRect, clipUvs);
                    mainColor.rgb *= mainColor.a;
                    return mainColor;
                }

                float outlineWidth = 0; //i.texCoord2.y;
                float outlineSoftness = 0; //i.texCoord2.z;
                float c = tex2D(_FontTexture, i.texCoord0.zw).a;
                float scaleRatio = _FontScaleRatioA;
                
                float scale	= i.texCoord1.y;
                float bias = i.texCoord1.z;
                float weight = i.texCoord1.w;
                float sd = (bias - c) * scale;

                float outline = 0; // (outlineWidth * scaleRatio) * scale;
                float softness = 0; //(outlineSoftness * scaleRatio) * scale;

                fixed4 faceColor = UnpackColor(asuint(i.color.r)); // could just be mainColor?
                // faceColor.a *= UIForiaAlphaClip(faceColor.a, _MaskTexture, screenUV, clipRect, clipUvs);
                
                fixed4 outlineColor = Green;//UnpackColor(asuint(i.color.g));
                //fixed4 underlayColor = UnpackColor(asuint(i.color.b));
                //fixed4 glowColor = UnpackColor(asuint(i.color.a));
                
                // underlayColor.rgb *= underlayColor.a;
                faceColor.rgb *= faceColor.a;
                // outlineColor.rgb *= outlineColor.a;
                // glowColor.rgb *= glowColor.a;
                
                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);
                
                #define underlayOffset i.texCoord3.xy
                #define underlayScale i.texCoord3.z
                #define underlayBias i.texCoord3.w
                
                // todo -- pull underlay into a seperate shader
                //  float d = tex2D(_FontTexture, i.texCoord0.xy + underlayOffset).a * underlayScale;
                //  faceColor += underlayColor * saturate(d - underlayBias) * (1 - faceColor.a);
                return faceColor;
                
                
                
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
