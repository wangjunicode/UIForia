Shader "UIForia/UIForiaText2"
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
            #pragma target 4.5
            
            #include "UnityCG.cginc"
            #include "UIForia.cginc"
            #include "Quaternion.cginc"
           
             struct appdata {
                uint vid : SV_VertexID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texCoord0 : TEXCOORD0;
                float2 texCoord1 : TEXCOORD1;
                float4 param : TEXCOORD2;
                int4 indices : TEXCOORD3;
                float4 underlay : TEXCOORD4; // todo -- remove if possible
            };
            
            struct UIForiaVertex {
                float2 position;
                float2 texCoord0;  // sdf uvs (inset by half a pixel)
                uint4 indices;     // lookup indices in buffers packed into ushorts
            };
             
            sampler2D _MainTex;
            sampler2D _FontTexture;
            sampler2D _OutlineTex;
            
            float _SpriteAtlasPadding;
            float _UIForiaDPIScale;
            float4x4 _UIForiaOriginMatrix;

            StructuredBuffer<float4x4> _UIForiaMatrixBuffer;            
            StructuredBuffer<UIForiaVertex> _UIForiaVertexBuffer;            
            StructuredBuffer<float4> _UIForiaFloat4Buffer;            
            StructuredBuffer<TextMaterialInfo> _UIForiaMaterialBuffer; 
            StructuredBuffer<GPUGlyphInfo> _UIForiaGlyphBuffer; 
            StructuredBuffer<GPUFontInfo> _UIForiaFontBuffer;
            
            #define TOP_LEFT 0
            #define TOP_RIGHT 1
            #define BOTTOM_RIGHT 2
            #define BOTTOM_LEFT 3
                     
          
            v2f vert (appdata v) {
                v2f o;
                
                int vertexId = v.vid & 0xffffff; // 3 bytes
                int cornerIdx = (v.vid >> 24) & (1 << 24) - 1; // 1 byte, 2 bits for corner idx, 6 bits free
                // cornerIdx = cornerIdx & 0xfffffff3;
                
                UIForiaVertex vertex = _UIForiaVertexBuffer[vertexId];
                uint baseEffectIdx = UnpackEffectIdx(vertex.indices);
                uint effectIdx = baseEffectIdx + cornerIdx;
                uint matrixIndex = UnpackMatrixId(vertex.indices);
                uint materialIndex = UnpackMaterialId(vertex.indices);
                uint fontIndex = 0; // todo -- pack font index somewhere, probably in the material? (uint)(vertex.texCoord0.y);
                uint glyphIndex = UnpackGlyphIdx(vertex.indices);
                
                GPUFontInfo fontInfo = _UIForiaFontBuffer[fontIndex];
                GPUGlyphInfo glyphInfo = _UIForiaGlyphBuffer[fontInfo.glyphOffset + glyphIndex];
                
                TextMaterialInfo materialInfo = _UIForiaMaterialBuffer[materialIndex];
            
                float left = glyphInfo.atlasX / fontInfo.atlasWidth;
                float top = glyphInfo.atlasY / fontInfo.atlasHeight;
                float right = left + (glyphInfo.atlasWidth / fontInfo.atlasWidth);
                float bottom = top + (glyphInfo.atlasHeight / fontInfo.atlasHeight);
                
                // can get mirrored text by flipping left and right
                uint displayBits = GetByteN(vertex.indices.y, 3);
                
                int invertHorizontal = (displayBits & TEXT_DISPLAY_FLAG_INVERT_HORIZONTAL_BIT) != 0;
                int invertVertical = (displayBits & TEXT_DISPLAY_FLAG_INVERT_VERTICAL_BIT) != 0;
                
                float tmp = lerp(left, right, invertHorizontal);
                right = lerp(right, left, invertHorizontal);
                left = tmp;
                
                tmp = lerp(top, bottom, invertVertical);
                bottom = lerp(bottom, top, invertVertical);
                top = tmp;
                
                TextMaterialInfoDecompressed textStyle = DecompressTextMaterialInfo(materialInfo);
                textStyle.underlayDilate = 0;
                textStyle.underlayDilate = lerp(textStyle.underlayDilate, -textStyle.underlayDilate, (displayBits & TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT) == 0);
                
                float3 ratios = ComputeSDFTextScaleRatios(fontInfo, textStyle);
                float padding = GetTextSDFPadding(fontInfo.gradientScale, textStyle, ratios);
                int isBold = 0;
                
                float weight = 0;
                float stylePadding = 0;
               
                if (isBold != 0) {
                    weight = fontInfo.weightBold;
                    stylePadding = fontInfo.boldStyle / 4.0 * fontInfo.gradientScale * ratios.x;
                }
                else {
                    weight = fontInfo.weightNormal;
                    stylePadding = fontInfo.normalStyle / 4.0 * fontInfo.gradientScale;
                }
                
                if (stylePadding + padding > fontInfo.gradientScale) {
                    padding = fontInfo.gradientScale - stylePadding;
                }
                
                float4 effectData = _UIForiaFloat4Buffer[effectIdx];
                float3 vpos = float3(vertex.position.xy, 0); // todo -- read z from somewhere if used
                
                float scaleMultiplier = lerp(1, effectData.w, baseEffectIdx != 0 && effectData.w != 0);
                
                // treat position as baseline position to rest on (so lower left corner)
 
                // float smallCapsMultiplier = 1;
                // float m_fontScale = fontSize * smallCapsMultiplier / fontInfo.pointSize * fontInfo.scale;
                float elementScale = vertex.texCoord0.x * scaleMultiplier; //m_fontScale * 1; // font scale multiplier
                padding += stylePadding;
                float scaledPaddingWidth = padding / fontInfo.atlasWidth;
                float scaledPaddingHeight = padding / fontInfo.atlasHeight;
                
                float charWidth = glyphInfo.width * elementScale;
                float charHeight = glyphInfo.height * elementScale;
                float ascender = fontInfo.ascender * elementScale;
                                
                if(baseEffectIdx != 0) {
                     vpos = effectData.xyz;
                     charWidth = 0;
                     charHeight = 0; 
                }
         
                // vpos.y -= (vertex.texCoord0.y - ascender); // push character down onto the baseline for when a single line has mutlipe fonts or font sizes 
                
                if(cornerIdx == TOP_LEFT) {
                    // vpos.x -= halfCharWidth;
                    vpos.x -= padding * elementScale;
                    vpos.y += padding * elementScale;
                    o.texCoord0 = float2(left - scaledPaddingWidth, bottom  + scaledPaddingHeight);
                    o.texCoord1 = float2(0, 1);
                }
                else if(cornerIdx == TOP_RIGHT) {
                    vpos.x += charWidth; 
                    vpos.x += padding * elementScale;
                    vpos.y += padding * elementScale;
                    o.texCoord0 = float2(right + scaledPaddingWidth, bottom + scaledPaddingHeight);
                    o.texCoord1 = float2(1, 1);
                }
                else if(cornerIdx == BOTTOM_RIGHT) {
                    vpos.x += charWidth;
                    vpos.y -= charHeight;
                    vpos.x += padding * elementScale;
                    vpos.y -= padding * elementScale;
                    o.texCoord0 = float2(right + scaledPaddingWidth, top - scaledPaddingHeight);
                    o.texCoord1 = float2(1, 0);
                }
                else { // BOTTOM_LEFT
                    // vpos.x -= halfCharWidth;
                    vpos.y -= charHeight;
                    vpos.x -= padding * elementScale;
                    vpos.y -= padding * elementScale;
                    o.texCoord0 = float2(left - scaledPaddingWidth, top - scaledPaddingHeight);
                    o.texCoord1 = float2(0, 0);
                }
             
                float4x4 transform = mul(_UIForiaMatrixBuffer[matrixIndex], _UIForiaOriginMatrix);
                
                o.vertex = mul(UNITY_MATRIX_VP, mul(transform, float4(vpos, 1.0)));
                
                float2 pixelSize = o.vertex.w;
                float2 scaleXY = float2(1, 1);

                pixelSize /= scaleXY * abs(mul((float2x2)UNITY_MATRIX_P, _ScreenParams.xy));
                float scale = rsqrt(dot(pixelSize, pixelSize));
                
                scale *= abs(elementScale) * fontInfo.gradientScale; // * (sharpness + 1);
                
			    weight = (weight + textStyle.faceDilate) * ratios.x * 0.5;

		    	float bias = (0.5 - weight) + (0.5 / scale);
	    		float alphaClip = (1.0 - textStyle.outlineWidth * ratios.x - textStyle.outlineSoftness * ratios.x);

                if(textStyle.glowPower != 0) {
 				    alphaClip = min(alphaClip, 1.0 - textStyle.glowOffset * ratios.y - textStyle.glowOuter * ratios.y);
                } 

				alphaClip = alphaClip / 2.0 - (0.5 / scale) - weight;
				
                float underlayScale = scale;
    
                underlayScale /= 1 + ((textStyle.underlaySoftness * ratios.z) * underlayScale);
                float underlayBias = (0.5 - weight) * underlayScale - 0.5 - ((textStyle.underlayDilate *  ratios.z) * 0.5 * underlayScale);
    
                float x = -(textStyle.underlayX * ratios.z) * fontInfo.gradientScale / fontInfo.atlasWidth;
                float y = -(textStyle.underlayY * ratios.z) * fontInfo.gradientScale / fontInfo.atlasHeight; 
                
                o.indices = vertex.indices;
                o.underlay = float4(x, y, underlayScale, underlayBias);
                o.param = float4(alphaClip, scale, bias, ratios.x);
                
                return o;
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
            
            fixed4 frag (v2f i) : SV_Target {
                
                // might not need the full uint4 of indices, can just get the 2 that I need
                TextMaterialInfo materialInfo = _UIForiaMaterialBuffer[UnpackMaterialId(i.indices)];
                half opacityMultiplier = GetByteN(i.indices.y, 2) / (float)0xff;

                float c = tex2Dlod(_FontTexture, float4(i.texCoord0, 0, 0)).a;
                uint displayBits = GetByteN(i.indices.y, 3);

                float alphaClip = i.param.x;
                float scale = i.param.y;
                float bias = i.param.z;
                float ratioA = i.param.w;
                float sd = (bias - c) * scale;

                float outlineWidth = GetByteN(materialInfo.glowOffsetOutlineWS, 2) / (float)0xff;
                float outlineSoftness = GetByteN(materialInfo.glowOffsetOutlineWS, 3) / (float)0xff;
                
                float outline = (outlineWidth * ratioA) * scale;
			    float softness = (outlineSoftness * ratioA) * scale;
			    
                half4 faceColor = UnpackColor(asuint(materialInfo.faceColor));
                half4 outlineColor = UnpackColor(asuint(materialInfo.outlineColor));
                half4 underlayColor = fixed4(1, 0, 0, 1);
                
                faceColor.a *= 0; //opacityMultiplier; 
                outlineColor.a *= opacityMultiplier; 
        
                faceColor = GetColor(sd, faceColor, outlineColor, outline, softness);
                
                float d = tex2D(_FontTexture, i.texCoord0.xy + i.underlay.xy).a * i.underlay.z;
			    float saturatedUnderlay = saturate(d - i.underlay.w);
			    float innerUnderlayMultiplier = (1 - saturatedUnderlay) * saturate(1 - sd) * (1 - faceColor.a);
			    float outerUnderlayMultiplier = saturatedUnderlay * (1 - faceColor.a);
			    faceColor += underlayColor * lerp(innerUnderlayMultiplier, outerUnderlayMultiplier, 0); //(displayBits & TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT) != 0);
			    
                // if not using underlay
                // clip(c - alphaClip);
                
                // this is not at all scientific, but I was seeing cases where the letter had an unwanted background tint
                // when using dilate or softness values that were too high. this fixes that, but very much not proven
                // remove if it causes weirdness. Could be that alpha cut off is just wrong, but tmp exibits the same behavior
                if(c < 0.05) {
                    return fixed4(0, 0, 0, 0);
                }
                return faceColor;
                
                // faceColor *= tex2D(_FaceTex, i.texCoord1 + float2(0, 0) * _Time.y);
                // outlineColor *= tex2D(_OutlineTex, i.texCoord1 + float2(0, 0) * _Time.y);
            
            }
            
            ENDCG
        }
    }
}
