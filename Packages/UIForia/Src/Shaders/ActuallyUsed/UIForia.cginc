#ifndef __UIForiaInc__
#define __UIForiaInc__

#define ColorMode_Color (1 << 0)
#define ColorMode_Texture (1 << 1)
#define ColorMode_TextureTint (1 << 2)
#define ColorMode_LetterBoxTexture (1 << 3)
#define Deg2Rad 0.01745329
#define PI 3.1415926535897932384626433832795
#define RED fixed4(1, 0, 0, 1)
#define GREEN fixed4(0, 1, 0, 1)
#define BLUE fixed4(0, 0, 1, 1)
#define WHITE fixed4(1, 1, 1, 1)
#define BLACK fixed4(0, 0, 0, 1)

#define TEXT_DISPLAY_FLAG_INVERT_HORIZONTAL_BIT (1 << 0)
#define TEXT_DISPLAY_FLAG_INVERT_VERTICAL_BIT (1 << 1)
#define TEXT_DISPLAY_FLAG_ITALIC_BIT (1 << 2)
#define TEXT_DISPLAY_FLAG_BOLD_BIT (1 << 3)
#define TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT (1 << 4)

struct AxisAlignedBounds2D {
    float xMin;
    float yMin;
    float xMax;
    float yMax;
};

int UnpackMatrixId(uint4 indices) {
    return indices.x & 0xffff;
}

int UnpackEffectIdx(uint4 indices) {
    return indices.w;
}

int UnpackMaterialId(uint4 indices) {
    return indices.y & 0xffff; //(asuint(indices.y) >> 16) & (1 << 16) - 1; 
}
     
int UnpackUVTransformId(uint4 indices) {
    return 0; // asuint(indices.y) & 0xffff;
}
         
uint UnpackGlyphIdx(uint4 indices) {
    return (indices.z >> 16) & (1 << 16) - 1; 
}
         
int UnpackClipRectId(int4 indices) {
    return (indices.x >> 16) & (1 << 16) - 1; 
}
            
uint ExtractByte(uint value, int byteIndex) {
    return ((value >> 8 * byteIndex) & 0xff);
}

float remap(float s, float a1, float a2, float b1, float b2) {
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
}
            
half remapHalf(half s, half a1, half a2, half b1, half b2) {
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
}

float4 GetGlowColor(float d, float scale, float4 glowColor, float glowOffset, float glowInner, float glowOuter, float glowPower) {
    float glow = d - glowOffset * 0.5 * scale;
    float t = lerp(glowInner, glowOuter, step(0.0, glow)) * 0.5 * scale;
    glow = saturate(abs(glow/(1.0 + t)));
    glow = 1.0 - pow(glow, glowPower);
    glow *= sqrt(min(1.0, t)); // Fade off glow thinner than 1 screen pixel
    return float4(glowColor.rgb, saturate(glowColor.a * glow * 2));
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
                        
float2 UnpackUV(float uv){ 
	float2 output;
	output.x = floor(uv / 4096);
	output.y = uv - 4096 * output.x;

	return output * 0.001953125;
}     

fixed4 UIForiaColorSpace(fixed4 color) {
    #ifdef UNITY_COLORSPACE_GAMMA
    return color;
    #else
    return fixed4(GammaToLinearSpace(color.rgb), color.a);
    #endif
}

uint GetByteN(uint value, int n) {
    return ((value >> 8 * n) & 0xff);
}
        
float4 UnpackColor(uint input) {
    return float4(
        uint((input >> 0) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 24) & 0xff) / float(0xff)
    );
}

float4 UnpackColor32(uint input) {
    return float4(
        uint((input >> 24) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 0) & 0xff) / float(0xff)
    );
}

//float4 SampleGradient(float Time,) {
//    float3 color = colors[0].rgb;
//    
//    [unroll]
//    for (int c = 1; c < 8; c ++) {
//        float colorPos = saturate((Time - colors[c - 1].w) / (colors[c].w - colors[c - 1].w)) * step(c, _GradientColorLength - 1);
//        color = lerp(color, colors[c].rgb, lerp(colorPos, step(0.01, colorPos), _GradientInterpolationType));
//    }
//    
//    float alpha = alphas[0].x;
//    
//    [unroll]
//    for (int a = 1; a < 8; a ++) {
//        float alphaPos = saturate((Time - alphas[a - 1].y) / (alphas[a].y - alphas[a - 1].y)) * step(a, _GradientAlphaLength - 1);
//        alpha = lerp(alpha, alphas[a].x, lerp(alphaPos, step(0.01, alphaPos), _GradientInterpolationType));
//    }
//    
//    return float4(color, alpha);
//}
                
bool PointInTriangle(float2 test, float2 p0, float2 p1, float2 p2) {
    float s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * test.x + (p0.x - p2.x) * test.y;
    float t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * test.x + (p1.x - p0.x) * test.y;
    
    if ((s < 0) != (t < 0)) {
        return false;
    }

    float area = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

    return area < 0
        ? (s <= 0 && s + t >= area)
        : (s >= 0 && s + t <= area);
}
            
fixed UIForiaOverflowClip(float2 screenPos, AxisAlignedBounds2D bounds) {
    return 1; // todo 
}
            
inline fixed4 ComputeColor(fixed4 mainColor, fixed4 tintColor, uint colorMode, float2 texCoord, sampler2D textureToRead) {
            
    int useColor = (colorMode & ColorMode_Color) != 0;
    int useTexture = (colorMode & ColorMode_Texture) != 0;
    int tintTexture = (colorMode & ColorMode_TextureTint) != 0;

    fixed4 textureColor = tex2Dlod(textureToRead, float4(texCoord, 0, 0));

    textureColor = lerp(textureColor, textureColor * tintColor, tintTexture);
    
    if(useTexture && useColor) {
        return lerp(textureColor, mainColor, 1 - textureColor.a);
    }
                    
    return lerp(mainColor, textureColor, useTexture);
}

float sdPie( in float2 p, in float2 c, in float r ) {
    p.x = abs(p.x);
    float l = length(p) - r;
    float m = length(p - c*clamp(dot(p,c),0.0,r) );
    return max(l,m*sign(c.y*p.x-c.x*p.y));
}

float sdRoundBox( in float2 p, in float2 b, in half4 r) {
    r.xy = (p.x < 0) ? r.xw : r.yz;
    r.x  = (p.y > 0) ? r.x  : r.y;
    
    float2 q = abs(p) - b + r.x;
    return min(max(q.x, q.y), 0) + length(max(q, 0)) - r.x;
}

half4 UnpackRadius(uint packedRadii, float2 size) {
    float minSize = min(size.x, size.y);
    half4 radius = 0.002 * half4(
        (packedRadii >>  0) & 0xff,
        (packedRadii >>  8) & 0xff,
        (packedRadii >> 16) & 0xff,
        (packedRadii >> 24) & 0xff
    );
    radius *= minSize;
    return radius;
}
     
float2 RotateUV(float2 uv, float rotation, float2 mid) {
    return float2(
      cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
      cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

float2 RotateUV(float2 uv, float rotation) {
    return float2(
      cos(rotation) * uv.x + sin(rotation) * uv.y,
      cos(rotation) * uv.y - sin(rotation) * uv.x
    );
}

float2 TransformUV(float2 uv, float2 offset, float2 scale, float rotation, half4 uvBounds, float atlasPadding) {
    uv += offset;
    uv *= scale;
    uv = RotateUV(uv, rotation, float2(0.5, 0.5));
    uv.x = (frac(uv.x) % (uvBounds.z - atlasPadding * 2)) + uvBounds.x + atlasPadding;
    uv.y = (frac(uv.y) % (uvBounds.w - atlasPadding * 2)) + uvBounds.y + atlasPadding;
    return uv;
}

// ---------------------- Text Structs and Functions ------------------------------------------------


// note -- must match non shader version 
struct GPUGlyphInfo {
     float atlasX;
     float atlasY;
     float atlasWidth;
     float atlasHeight;

     float xOffset;
     float yOffset;
     float width;
     float height;
};
            
// note -- must match non shader version 
struct GPUFontInfo {
    float gradientScale;
    float normalStyle;
    float boldStyle;
    float italicStyle;
    
    float weightNormal;
    float weightBold;
    float pointSize;
    float scale;
    
    int glyphOffset;
    float atlasWidth;
    float atlasHeight;
    float ascender;
};

struct TextMaterialInfoDecompressed {

    float faceDilate;
    float outlineWidth;
    float outlineSoftness;
    
    float glowPower;
    float glowInner;
    float glowOuter;
    float glowOffset;
    
    float underlayDilate;
    float underlaySoftness;
    float underlayX;
    float underlayY;
};
// float remap(float s, float a1, float a2, float b1, float b2) {
//     return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
// }
//
//uint UnpackGlyphIdx(uint4 indices) {
//    return (indices.z >> 16) & (1 << 16) - 1; 
//}
//         
//int UnpackClipRectId(int4 indices) {
//    return (indices.x >> 16) & (1 << 16) - 1; 
//}



// layout must EXACTLY match TextMaterialInfo in C#
struct TextMaterialInfo {
    
    uint faceColor;
    uint outlineColor;
    uint glowColor;
    uint underlayColor;
    
    uint faceUnderlayDilate;
    
    float underlayX;
    float underlayY;
    
    uint glowPIOUnderlayS;
    uint glowOffsetOutlineWS;
    
    uint unused0;
    uint unused1;
    uint unused2;
};
            
TextMaterialInfoDecompressed DecompressTextMaterialInfo(in TextMaterialInfo textMaterialInfo) {

    TextMaterialInfoDecompressed retn;
    uint faceDilate = (textMaterialInfo.faceUnderlayDilate) & 0xffff;
    uint underDilate = (textMaterialInfo.faceUnderlayDilate >> 16) & (1 << 16) - 1;
    retn.faceDilate = remap(faceDilate, 0, (float)0xffff, -1, 1);
    retn.outlineWidth = GetByteN(textMaterialInfo.glowOffsetOutlineWS, 2) / (float)0xff;
    retn.outlineSoftness = GetByteN(textMaterialInfo.glowOffsetOutlineWS, 3) / (float)0xff;
    retn.glowPower = 0;
    retn.glowInner = 0;
    retn.glowOuter = 0;
    retn.glowOffset = 0;
    retn.underlaySoftness = GetByteN(textMaterialInfo.glowPIOUnderlayS, 3) / (float)0xff;
    retn.underlayDilate = remap(underDilate, 0, (float)0xffff, -1, 1);
    retn.underlayX = textMaterialInfo.underlayX;
    retn.underlayY = textMaterialInfo.underlayY;
    
    return retn;
 
}

float3 ComputeSDFTextScaleRatios(in GPUFontInfo fontInfo, in TextMaterialInfoDecompressed textStyle) {
    float gradientScale = fontInfo.gradientScale;
    float faceDilate = textStyle.faceDilate;
    float outlineThickness = textStyle.outlineWidth;
    float outlineSoftness = textStyle.outlineSoftness;
    float weight = (fontInfo.weightNormal > fontInfo.weightBold ? fontInfo.weightNormal : fontInfo.weightBold) / 4.0;
    float ratioA_t = max(1, weight + faceDilate + outlineThickness + outlineSoftness);
    float ratioA = (gradientScale - 1) / (gradientScale * ratioA_t);

    float glowOffset = textStyle.glowOffset;
    float glowOuter = textStyle.glowOuter;
    float ratioBRange = (weight + faceDilate) * (gradientScale - 1.0);

    float ratioB_t = glowOffset + glowOuter > 1 ? glowOffset + glowOuter : 1;
    float ratioB = max(0, gradientScale - 1 - ratioBRange) / (gradientScale * ratioB_t);
    if (ratioB < 0) ratioB = 0;
    float underlayOffsetX = textStyle.underlayX;
    float underlayOffsetY = textStyle.underlayY;
    float underlayDilate = textStyle.underlayDilate;
    float underlaySoftness = textStyle.underlaySoftness;

    float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
    float ratioC_t = max(1, max(abs(underlayOffsetX), abs(underlayOffsetY)) + underlayDilate + underlaySoftness);

    float ratioC = max(0, gradientScale - 1.0 - ratioCRange) / (gradientScale * ratioC_t);

    return float3(ratioA, ratioB, ratioC);
}
            
float GetTextSDFPadding(float gradientScale, in TextMaterialInfoDecompressed textStyle, in float3 ratios) {
    float4 padding = float4(0, 0, 0, 0);

    float scaleRatio_A = ratios.x;
    float scaleRatio_B = ratios.y;
    float scaleRatio_C = ratios.z;

    float faceDilate = textStyle.faceDilate * scaleRatio_A;
    float faceSoftness = textStyle.outlineSoftness * scaleRatio_A;
    float outlineThickness = textStyle.outlineWidth * scaleRatio_A;

    float uniformPadding = outlineThickness + faceSoftness + faceDilate;

    float glowOffset = textStyle.glowOffset * scaleRatio_B;
    float glowOuter = textStyle.glowOuter * scaleRatio_B;

    float dilateOffsetGlow = faceDilate + glowOffset + glowOuter;
    uniformPadding = uniformPadding > dilateOffsetGlow ? uniformPadding : dilateOffsetGlow;

    float offsetX = textStyle.underlayX * scaleRatio_C;
    float offsetY = textStyle.underlayY * scaleRatio_C;
    float dilate =  textStyle.underlayDilate * scaleRatio_C;
    float softness = textStyle.underlaySoftness * scaleRatio_C;

    // tmp does a max check here with 0, I don't think we need it though
    padding.x = faceDilate + dilate + softness - offsetX;
    padding.y = faceDilate + dilate + softness - offsetY;
    padding.z = faceDilate + dilate + softness + offsetX;
    padding.w = faceDilate + dilate + softness + offsetY;

    padding = max(padding, uniformPadding);

    padding.x = padding.x < 1 ? padding.x : 1;
    padding.y = padding.y < 1 ? padding.y : 1;
    padding.z = padding.z < 1 ? padding.z : 1;
    padding.w = padding.w < 1 ? padding.w : 1;
    
    padding *= gradientScale;

    // Set UniformPadding to the maximum value of any of its components.
    uniformPadding = padding.x > padding.y ? padding.x : padding.y;
    uniformPadding = padding.z > uniformPadding ? padding.z : uniformPadding;
    uniformPadding = padding.w > uniformPadding ? padding.w : uniformPadding;

    return uniformPadding + 1.25;
}

#endif