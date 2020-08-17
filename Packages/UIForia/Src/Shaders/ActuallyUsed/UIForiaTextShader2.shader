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
           
             struct appdata {
                uint vid : SV_VertexID;
            };

            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texCoord0 : TEXCOORD0;
                float2 texCoord1 : TEXCOORD1;
                nointerpolation float3 param : TEXCOORD2;
                nointerpolation uint4 indices : TEXCOORD3;
                nointerpolation float4 underlay : TEXCOORD4;
                nointerpolation float2 ratios : TextCoord5;
            };
            
            struct UIForiaVertex {
                float2 position;
                float2 texCoord0;  // x = character scale, y = currently unused, maybe z position (move to actual position field in this case)
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

                // todo --
                    // underline & strikethrough
                    // verify texturing works
                    // opacity modifier
                    // clipping & masking
                
                int vertexId = v.vid & 0xffffff; // 3 bytes
                int cornerIdx = (v.vid >> 24) & (1 << 24) - 1; // 1 byte, 2 bits for corner idx, 6 bits free
                // cornerIdx = cornerIdx & 0xfffffff3;
                
                UIForiaVertex vertex = _UIForiaVertexBuffer[vertexId];
                uint baseEffectIdx = UnpackEffectIdx(vertex.indices);
                uint effectIdx = baseEffectIdx + cornerIdx;
                uint matrixIndex =  UnpackMatrixId(vertex.indices);
                uint materialIndex = UnpackMaterialId(vertex.indices);
                uint fontIndex =  UnpackFontIdx(vertex.indices);
                uint glyphIndex = UnpackGlyphIdx(vertex.indices);
                
                GPUFontInfo fontInfo = _UIForiaFontBuffer[fontIndex];
                GPUGlyphInfo glyphInfo = _UIForiaGlyphBuffer[fontInfo.glyphOffset + glyphIndex];
                
                TextMaterialInfo materialInfo = _UIForiaMaterialBuffer[materialIndex];
            
                float left = glyphInfo.atlasX / fontInfo.atlasWidth;
                float top = glyphInfo.atlasY / fontInfo.atlasHeight;
                float right = left + (glyphInfo.atlasWidth / fontInfo.atlasWidth);
                float bottom = top + (glyphInfo.atlasHeight / fontInfo.atlasHeight);
                
                // can get mirrored text by flipping left and right
                uint displayBits = GetByteN(vertex.indices.w, 3);
                
                int invertHorizontal = (displayBits & TEXT_DISPLAY_FLAG_INVERT_HORIZONTAL_BIT) != 0;
                int invertVertical = (displayBits & TEXT_DISPLAY_FLAG_INVERT_VERTICAL_BIT) != 0;
                float2 shear = float2(0, 0);
                
                if ((displayBits & TEXT_DISPLAY_FLAG_ITALIC_BIT) != 0) {
                    shear = fontInfo.italicStyle * 0.01f;
                    shear.x *= glyphInfo.yOffset;
                    shear.y *= glyphInfo.yOffset - glyphInfo.height;
                }
                
                float tmp = lerp(left, right, invertHorizontal);
                right = lerp(right, left, invertHorizontal);
                left = tmp;
                
                tmp = lerp(top, bottom, invertVertical);
                bottom = lerp(bottom, top, invertVertical);
                top = tmp;
                
                TextMaterialInfoDecompressed textStyle = DecompressTextMaterialInfo(materialInfo);
                // invert the underlay dilation when doing an inner underlay
                textStyle.underlayDilate = lerp(textStyle.underlayDilate, -textStyle.underlayDilate, (displayBits & TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT) != 0);

                float3 ratios = ComputeSDFTextScaleRatios(fontInfo, textStyle);
                float padding = GetTextSDFPadding(fontInfo.gradientScale, textStyle, ratios);
                float weight = 0;
                float stylePadding = 0;
               
                if ((displayBits & TEXT_DISPLAY_FLAG_BOLD_BIT) != 0) {
                    weight = fontInfo.weightBold / 4.0;
                    stylePadding = fontInfo.boldStyle / 4.0 * fontInfo.gradientScale * ratios.x;
                }
                else {
                    weight = fontInfo.weightNormal;
                    stylePadding = fontInfo.normalStyle / 4.0 * fontInfo.gradientScale;
                }
                
                if (stylePadding + padding > fontInfo.gradientScale) {
                    padding = fontInfo.gradientScale - stylePadding;
                }
                
                padding += stylePadding;

                float4 effectData = _UIForiaFloat4Buffer[effectIdx];
                float3 vpos = float3(vertex.position.xy, 0); // todo -- read z from somewhere if used
                
                float scaleMultiplier = baseEffectIdx == 0 ? 1: effectData.w;
                if(scaleMultiplier <= 0) scaleMultiplier = 1;
                 
                float elementScale = vertex.texCoord0.x * scaleMultiplier;
                float scaledPaddingWidth = (padding / fontInfo.atlasWidth) * (invertHorizontal == 1 ? -1 : 1);
                float scaledPaddingHeight = (padding / fontInfo.atlasHeight) * (invertVertical == 1 ? -1 : 1);
 
                float charWidth = glyphInfo.width * elementScale;
                float charHeight = glyphInfo.height * elementScale;
                                
                if(baseEffectIdx != 0) {
                     vpos = effectData.xyz;
                     charWidth = 0;
                     charHeight = 0; 
                }
         
                // vpos.y -= (vertex.texCoord0.y - ascender); // push character down onto the baseline for when a single line has mutlipe fonts or font sizes 
                
                if(cornerIdx == TOP_LEFT) {
                    vpos.x -= padding * elementScale;
                    vpos.x += shear.x * elementScale;
                    vpos.y += padding * elementScale;
                    o.texCoord0 = float2(left - scaledPaddingWidth, bottom  + scaledPaddingHeight);
                    o.texCoord1 = float2(0, 1);
                }
                else if(cornerIdx == TOP_RIGHT) {
                    vpos.x += charWidth;
                    vpos.x += shear.x * elementScale;
                    vpos.x += padding * elementScale;
                    vpos.y += padding * elementScale;
                    o.texCoord0 = float2(right + scaledPaddingWidth, bottom + scaledPaddingHeight);
                    o.texCoord1 = float2(1, 1);
                }
                else if(cornerIdx == BOTTOM_RIGHT) {
                    vpos.x += charWidth;
                    vpos.x += shear.y * elementScale;
                    vpos.x += padding * elementScale;
                    vpos.y -= charHeight;
                    vpos.y -= padding * elementScale;
                    o.texCoord0 = float2(right + scaledPaddingWidth, top - scaledPaddingHeight);
                    o.texCoord1 = float2(1, 0);
                }
                else { // BOTTOM_LEFT
                    vpos.x += shear.y * elementScale;
                    vpos.x -= padding * elementScale;
                    vpos.y -= charHeight;
                    vpos.y -= padding * elementScale;
                    o.texCoord0 = float2(left - scaledPaddingWidth, top - scaledPaddingHeight);
                    o.texCoord1 = float2(0, 0);
                }
             
                float4x4 transform = mul(_UIForiaOriginMatrix, _UIForiaMatrixBuffer[matrixIndex]);
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
                o.ratios = float2(ratios.x, ratios.y);
                o.underlay = float4(x, y, underlayScale, underlayBias);
                o.param = float3(alphaClip, scale, bias);
                o.indices.x = UnpackClipRectId(vertex.indices.x);
                o.indices.y = vertex.indices.y;
                o.indices.z = 0;
                o.indices.w = vertex.indices.w;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target {
                half opacityMultiplier = GetByteNToFloat(i.indices.y, 3);
                uint displayBits = GetByteN(i.indices.w, 3);

                TextMaterialInfo materialInfo = _UIForiaMaterialBuffer[UnpackMaterialId(i.indices.y & 0xffff)];

                float c = tex2Dlod(_FontTexture, float4(i.texCoord0, 0, 0)).a;

                float alphaClip = i.param.x;
                float scale = i.param.y;
                float bias = i.param.z;
                
                float sd = (bias - c) * scale;

                float outlineWidth = GetByteNToFloat(materialInfo.glowOffsetOutlineWS, 2);
                float outlineSoftness = GetByteNToFloat(materialInfo.glowOffsetOutlineWS, 3);
                
                float outline = (outlineWidth * i.ratios.x) * scale;
			    float softness = (outlineSoftness * i.ratios.x) * scale;
			    
                half4 faceColor = UnpackColor(materialInfo.faceColor);
                half4 outlineColor = UnpackColor(materialInfo.outlineColor);
                half4 underlayColor = UnpackColor(materialInfo.underlayColor);
                half4 glowColor = UnpackColor(materialInfo.glowColor);
                
                faceColor.a *= opacityMultiplier; 
                outlineColor.a *= opacityMultiplier;

                float4 sampleCoord = float4(i.texCoord1, 0, 0);
                faceColor *= tex2Dlod(_MainTex, sampleCoord);
                outlineColor *= tex2D(_OutlineTex, sampleCoord);
                
                faceColor = GetTextColor(sd, faceColor, outlineColor, outline, softness);
                
                float d = tex2D(_FontTexture, i.texCoord0.xy + i.underlay.xy).a * i.underlay.z;
			    float saturatedUnderlay = saturate(d - i.underlay.w);
                // note -- inner underlay only works when face alpha is 0, I think i want to model this different to enable a character render mode = Default | InsetUnderlay or something like that
			    float innerUnderlayMultiplier = (1 - saturatedUnderlay) * saturate(1 - sd) * (1 - faceColor.a);
			    float outerUnderlayMultiplier = saturatedUnderlay * (1 - faceColor.a);
			    faceColor += underlayColor * lerp(outerUnderlayMultiplier, innerUnderlayMultiplier, (displayBits & TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT) != 0);

                // negative glow offset is only visible when face has alpha
                half glowOffset = Remap(materialInfo.glowOffsetOutlineWS & 0xffff, 0, (float)0xffff, -1, 1);
                half glowPower = GetByteNToFloat(materialInfo.glowPIOUnderlayS, 0);
                half glowInner = GetByteNToFloat(materialInfo.glowPIOUnderlayS, 1);
                half glowOuter = GetByteNToFloat(materialInfo.glowPIOUnderlayS, 2);
                // glowColor = lerp(glowColor, faceColor,  faceColor.a); // -- this gives an interesting effect when using underlay and glow together
                glowColor = GetGlowColor(sd, scale, glowColor, glowOffset * i.ratios.y, glowInner, glowOuter * i.ratios.y, glowPower);
			    faceColor.rgb += glowColor.rgb * glowColor.a;
                
                float2 clipPos = float2(i.vertex.x, _ProjectionParams.x > 0 ? i.vertex.y : _ScreenParams.y - i.vertex.y); //* _UIForiaDPIScale;
                float4 clipRect = _UIForiaFloat4Buffer[i.indices.x]; // x = xMin, y = yMin, z = xMax, w = yMax
                float2 s = step(clipRect.xw, clipPos) - step(clipRect.zy, clipPos);
                
                clip(c - alphaClip);
                
                // this is not at all scientific, but I was seeing cases where the letter had an unwanted background tint
                // when using dilate or softness values that were too high. this fixes that, but very much not proven
                // remove if it causes weirdness. Could be that alpha cut off is just wrong, but tmp exhibits the same behavior
                return ((c < 0.05 || s.x * s.y == 0) ? fixed4(0, 0, 0, 0) : faceColor);
            
            }
            
            ENDCG
        }
    }
}
