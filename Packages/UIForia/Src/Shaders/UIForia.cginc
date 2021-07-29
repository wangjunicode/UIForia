#ifndef __UIForiaInc__

#define __UIForiaInc__

#define ColorMode_Color (1 << 0)
#define ColorMode_Texture (1 << 1)
#define ColorMode_TextureTint (1 << 2)
#define ColorMode_LetterBoxTexture (1 << 3)
#define ColorMode_CoverTexture (1 << 4)
#define ColorMode_ApplyColorSpaceCorrection (1 << 5)

#define MaskFlags_UseTextureMask (1 << 0)
#define MaskFlags_UseTextureMaskInverted (1 << 1)

#define GradientMode_TextureColor (1 << 1)
#define GradientMode_TextureAlpha (1 << 2)
#define GradientMode_Opacity (1 << 3)
#define GradientMode_Color (1 << 4)
#define GradientMode_ColorTint (1 << 5)
#define GradientMode_ColorAlpha (1 << 6)

#define GradientType_Conical 1
#define GradientType_Linear 2
#define GradientType_Radial 3
#define GradientType_Corner 4

#define Deg2Rad 0.01745329
#define PI 3.1415926535897932384626433832795
#define RED fixed4(1, 0, 0, 1)
#define GREEN fixed4(0, 1, 0, 1)
#define BLUE fixed4(0, 0, 1, 1)
#define WHITE fixed4(1, 1, 1, 1)
#define BLACK fixed4(0, 0, 0, 1)
#define CLEAR fixed4(0, 0, 0, 0)
#define PINK fixed4(1.0, 0.05, 0.3, 1)

// Mirrors CharacterDisplayFlags in C#
#define TEXT_DISPLAY_FLAG_INVERT_HORIZONTAL_BIT (1 << 0)
#define TEXT_DISPLAY_FLAG_INVERT_VERTICAL_BIT (1 << 1)
#define TEXT_DISPLAY_FLAG_BOLD_BIT (1 << 2)
#define TEXT_DISPLAY_FLAG_ITALIC_BIT (1 << 3)
#define TEXT_DISPLAY_FLAG_UNDERLAY_INNER_BIT (1 << 4)

#define ElementShaderFlags_IsNineSliceBorder (1 << 0)
#define ElementShaderFlags_IsNineSliceContent (1 << 1)

#define TOP_LEFT 0
#define TOP_RIGHT 1
#define BOTTOM_RIGHT 2
#define BOTTOM_LEFT 3

#define ToneEffect_Grayscale (1 << 0)
#define ToneEffect_Sepia (1 << 1)
#define ToneEffect_Negate (1 << 2)

#define ColorEffect_Fill (1 << 0)
#define ColorEffect_Add (1 << 1)
#define ColorEffect_Subtract (1 << 2)
#define ColorEffect_Cutoff (1 << 3)
#define ColorEffect_Default (ColorEffect_Fill | ColorEffect_Add | ColorEffect_Subtract)

float4 DebugSDF(float sdf)
{
    float3 col = float3(1.0, 1, 1) - sign(sdf) * float3(0.1, 0.4, 0.7);
    col *= 1.0 - exp(-3.0 * abs(sdf));
    col *= 0.8 + 0.2 * cos(150.0 * sdf);
    col = lerp(col, float3(1.0, 0, 0), 1.0 - smoothstep(0.0, 0.02, abs(sdf)));
    return float4(col, 1);
}

fixed4 ApplyColorEffect(half4 color, half4 factor, uint colorEffectFlags)
{
    fixed3 fill = lerp(color.rgb, factor.rgb, factor.a);
    fixed3 add = color.rgb + factor.rgb * factor.a;
    fixed3 sub = color.rgb - factor.rgb * factor.a;
    fixed3 def = lerp(color.rgb, color.rgb * factor.rgb, factor.a);

    color.rgb = lerp(color.rgb, fill, (colorEffectFlags & ColorEffect_Fill) != 0);
    color.rgb = lerp(color.rgb, add, (colorEffectFlags & ColorEffect_Add) != 0);
    color.rgb = lerp(color.rgb, sub, (colorEffectFlags & ColorEffect_Subtract) != 0);
    color.rgb = lerp(color.rgb, def, (colorEffectFlags & ColorEffect_Default) == 0);

    color.a = lerp(color.a, factor.a, (colorEffectFlags & ColorEffect_Cutoff) != 0);

    return color;
}

fixed4 ApplyToneEffect(fixed4 color, fixed factor, uint toneEffectFlags)
{
    fixed3 lum = Luminance(color.rgb);
    fixed3 greyscale = lerp(color.rgb, lum, factor);
    fixed3 sepia = lerp(color.rgb, lum * half3(1.07, 0.74, 0.43), factor);
    fixed3 negate = lerp(color.rgb, 1 - color.rgb, factor);

    color.rgb = lerp(color.rgb, greyscale, (toneEffectFlags & ToneEffect_Grayscale) != 0);
    color.rgb = lerp(color.rgb, sepia, (toneEffectFlags & ToneEffect_Sepia) != 0);
    color.rgb = lerp(color.rgb, negate, (toneEffectFlags & ToneEffect_Negate) != 0);

    return color;
}

struct AxisAlignedBounds2D
{
    float xMin;
    float yMin;
    float xMax;
    float yMax;
};

float UnpackHighUShortPercentageToFloat(uint value)
{
    return ((value >> 16) & (1 << 16) - 1) / 65536.0;
}

float UnpackLowUShortPercentageToFloat(uint value)
{
    return (value & 0xffff) / 65536.0;
}

int UnpackMatrixId(uint4 indices)
{
    return indices.x & 0xffff;
}

int UnpackEffectIdx(uint4 indices)
{
    return indices.w & 0xffffff; // 3 bytes
}

int UnpackMaterialId(uint4 indices)
{
    return indices.y & 0xffff; // 3 bytes
}

int UnpackFontIdx(uint4 indices)
{
    return indices.z & 0xffff;
}

int UnpackUVTransformId(uint4 indices)
{
    return 0;
}

uint UnpackHighBytes(uint value)
{
    return (value >> 16) & (1 << 16) - 1;
}

uint UnpackLowBytes(uint value)
{
    return value & 0xffff;
}

uint UnpackGlyphIdx(uint4 indices)
{
    return (indices.z >> 16) & (1 << 16) - 1;
}

int UnpackClipRectId(int4 indices)
{
    return (indices.x >> 16) & (1 << 16) - 1;
}

uint ExtractByte(uint value, int byteIndex)
{
    return ((value >> 8 * byteIndex) & 0xff);
}

float Remap(float s, float a1, float a2, float b1, float b2)
{
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
}

half remapHalf(half s, half a1, half a2, half b1, half b2)
{
    return b1 + (s - a1) * (b2 - b1) / (a2 - a1);
}

// same as UnityPixelSnap except that we add 0.5 to pixelPos after rounding
inline float4 UIForiaPixelSnap(float4 pos)
{
    float2 hpc = _ScreenParams.xy * 0.5f;
    float2 adjustment = float2(0, 0);
    adjustment.x = (_ScreenParams.x % 2 != 0) * 0.5;
    adjustment.y = (_ScreenParams.y % 2 != 0) * 0.5;
    float2 pixelPos = round((pos.xy / pos.w) * hpc) + adjustment;
    pos.xy = pixelPos / hpc * pos.w;
    return pos;
}

fixed4 UIForiaColorSpace(fixed4 color)
{
    #ifdef UNITY_COLORSPACE_GAMMA
    return color;
    #else
    return fixed4(GammaToLinearSpace(color.rgb), color.a);
    #endif
}

half4 GetGlowColor(float d, float scale, half4 glowColor, half glowOffset, half glowInner, half glowOuter,
                   half glowPower)
{
    half glow = d - glowOffset * 0.5 * scale;
    glowInner = max(0.1, glowInner);
    glowOuter = max(0.1, glowOuter);
    half t = lerp(glowInner, glowOuter, step(0.0, glow)) * 0.5 * scale;
    glow = saturate(abs(glow / (1.0 + t)));
    glow = 1.0 - pow(glow, glowPower);
    glow *= sqrt(min(1.0, t));

    return half4(glowColor.rgb, saturate(glowColor.a * glow * 2));
}

fixed4 GetTextColor(half d, fixed4 faceColor, fixed4 outlineColor, half outline, half softness)
{
    half faceAlpha = 1 - saturate((d - outline * 0.5 + softness * 0.5) / (1.0 + softness));
    half outlineAlpha = saturate((d + outline * 0.5)) * sqrt(min(1.0, outline));

    faceColor = UIForiaColorSpace(faceColor);
    outlineColor = UIForiaColorSpace(outlineColor);

    faceColor.rgb *= faceColor.a;
    outlineColor.rgb *= outlineColor.a;

    faceColor = lerp(faceColor, outlineColor, outlineAlpha);

    faceColor *= faceAlpha;

    return faceColor;
}

float2 UnpackUV(float uv)
{
    float2 output;
    output.x = floor(uv / 4096);
    output.y = uv - 4096 * output.x;

    return output * 0.001953125;
}


fixed4 UIForiaTextColorSpace(fixed4 color)
{
    #ifdef UNITY_COLORSPACE_GAMMA
    return color;
    #else
    return fixed4(GammaToLinearSpace(color.rgb), color.a);
    #endif
}

uint GetByteN(uint value, int n)
{
    return ((value >> 8 * n) & 0xff);
}

float GetByteNToFloat(uint value, int n)
{
    return ((value >> (8 * n)) & 0xff) / (float)0xff;
}

float4 UnpackColor(uint input)
{
    return float4(
        uint((input >> 0) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 24) & 0xff) / float(0xff)
    );
}

float4 UnpackColor32(uint input)
{
    return float4(
        uint((input >> 24) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 0) & 0xff) / float(0xff)
    );
}

bool PointInTriangle(float2 test, float2 p0, float2 p1, float2 p2)
{
    float s = p0.y * p2.x - p0.x * p2.y + (p2.y - p0.y) * test.x + (p0.x - p2.x) * test.y;
    float t = p0.x * p1.y - p0.y * p1.x + (p0.y - p1.y) * test.x + (p1.x - p0.x) * test.y;

    if ((s < 0) != (t < 0))
    {
        return false;
    }

    float area = -p1.y * p2.x + p0.y * (p2.x - p1.x) + p0.x * (p1.y - p2.y) + p1.x * p2.y;

    return area < 0
               ? (s <= 0 && s + t >= area)
               : (s >= 0 && s + t <= area);
}

fixed UIForiaOverflowClip(float2 screenPos, AxisAlignedBounds2D bounds)
{
    return 1; // todo 
}


inline float4 ComputeColor(fixed4 mainColor, fixed4 gradientColor, fixed4 tintColor, uint colorMode, float2 texCoord,
                           sampler2D textureToRead, float4 uvBounds, float2 originalUV)
{
    int useColor = (colorMode & ColorMode_Color) != 0;
    int useTexture = (colorMode & ColorMode_Texture) != 0;
    int tintTexture = (colorMode & ColorMode_TextureTint) != 0;
    int coverTexture = (colorMode & ColorMode_CoverTexture) != 0;
    float4 textureColor = tex2D(textureToRead, texCoord); // float4(texCoord, 0, 0));
    int gradientMode = 0;

    // textureColor.rgb = lerp(textureColor.rgb, gradientColor.rgb, (gradientMode & GradientMode_TextureColor) != 0);
    // textureColor.a *= lerp(1, 1 - (textureColor.a - gradientColor.a), (gradientMode & GradientMode_TextureAlpha) != 0);
    // mainColor.rgb = lerp(mainColor.rgb, gradientColor.rgb, (gradientMode & GradientMode_Color) != 0);
    // mainColor.a = lerp(mainColor.a, gradientColor.a, (gradientMode & GradientMode_ColorAlpha) != 0);
    // return textureColor;

    // #ifdef UIFORIA_GRADIENT
    // if gradient mode = background

    // if gradientMode == GradientMode_TextureColor
    // if gradientMode == GradientMode_TextureAlpha
    // if gradientMode == GradientMode_Opacity
    // if gradientMode == GradientMode_TextureTint
    // if gradientMode == GradientMode_BackgroundColor
    // if gradientMode == GradientMode_BackgroundTint
    // if gradientMode == GradientMode_BackgroundOpacity

    // #endif

    textureColor = lerp(textureColor, textureColor * tintColor, tintTexture);
    // todo -- these lines could implement a cover effect a-la css background cover 
    float2 s = step(uvBounds.xw, texCoord) - step(uvBounds.zy, texCoord);
    textureColor = lerp(textureColor, mainColor, s.x * s.y == 0);
    if (useTexture && useColor)
    {
        return lerp(textureColor, mainColor, 1 - textureColor.a);
    }
    return lerp(mainColor, textureColor, useTexture);
}

float sdPie(in float2 p, in float2 c, in float r)
{
    p.x = abs(p.x);
    float l = length(p) - r;
    float m = length(p - c * clamp(dot(p, c), 0.0, r));
    return max(l, m * sign(c.y * p.x - c.x * p.y));
}

float sdRoundBox(in float2 p, in float2 b, in half r)
{
    float2 q = abs(p) - b + r;
    return min(max(q.x, q.y), 0) + length(max(q, 0)) - r;
}

float sdRect(in float2 p, in float2 b)
{
    float2 q = abs(p) - b;
    return min(max(q.x, q.y), 0) + length(max(q, 0));
}

float sdBox(in float2 p, in float2 b)
{
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

half UnpackRadius(float2 p, uint packedRadii, float minSize)
{
    half4 radius = 0.002 * half4(
        (packedRadii >> 24) & 0xff,
        (packedRadii >> 16) & 0xff,
        (packedRadii >> 8) & 0xff,
        (packedRadii >> 0) & 0xff
    );
    radius *= minSize;
    radius.xy = (p.x < 0) ? radius.xw : radius.yz;
    return (p.y > 0) ? radius.x : radius.y;
}

float2 RotateUV(float2 uv, float rotation, float2 mid)
{
    return float2(
        cos(rotation) * (uv.x - mid.x) + sin(rotation) * (uv.y - mid.y) + mid.x,
        cos(rotation) * (uv.y - mid.y) - sin(rotation) * (uv.x - mid.x) + mid.y
    );
}

float2 RotateUV(float2 uv, float rotation)
{
    return float2(
        cos(rotation) * uv.x + sin(rotation) * uv.y,
        cos(rotation) * uv.y - sin(rotation) * uv.x
    );
}

float2 TransformUV(float2 uv, float2 offset, float2 scale, float rotation, half4 uvBounds)
{
    uv += offset;
    uv *= scale;
    uv = RotateUV(uv, rotation, float2(0.5, 0.5));
    // todo -- verify pivot point is correct -- (uvBounds.z - uvBounds.x) * 0.5, uvBounds.y + (uvBounds.w - uvBounds.y) * 0.5));
    // uv.x = lerp(uvBounds.x, uvBounds.z, frac(uv.x));
    // uv.y = lerp(uvBounds.y, uvBounds.w, frac(uv.y));

    // uv *= float2(uvBounds.z - uvBounds.x, uvBounds.w - uvBounds.y);
    //(frac(uv.x) % (uvBounds.z)) + uvBounds.x;
    // uv.y = (frac(uv.y) % (uvBounds.w - uvBounds.y)) + uvBounds.y;

    return uv;
}

inline half DistToLine(half2 pt1, half2 pt2, half2 testPt)
{
    half2 lineDir = pt2 - pt1;
    half2 perpDir = half2(lineDir.y, -lineDir.x);
    half2 dirToPt1 = pt1 - testPt;
    return abs(dot(normalize(perpDir), dirToPt1));
}

inline float UnpackCornerBevel(uint bevelTop, uint bevelBottom, float2 texCoord)
{
    float left = step(texCoord.x, 0.5); // 1 if left
    float bottom = step(1 - texCoord.y, 0.5); // 1 if bottom

    #define top (1 - bottom)
    #define right (1 - left)

    uint bevelAmount = 0;
    bevelAmount += (top * left) * UnpackHighBytes(bevelTop);
    bevelAmount += (top * right) * UnpackLowBytes(bevelTop);
    bevelAmount += (bottom * right) * UnpackLowBytes(bevelBottom);
    bevelAmount += (bottom * left) * UnpackHighBytes(bevelBottom);

    return (float)bevelAmount;

    #undef top
    #undef right
}

// ---------------------- Gradient Functions --------------------------------------------------------

float RadialGradient(float2 gradientTexCoord)
{
    return length(gradientTexCoord - 0.5);
}

float ConicalGradient(float2 gradientTexCoord)
{
    gradientTexCoord -= 0.5;
    return (atan2(gradientTexCoord.y, gradientTexCoord.x) + PI) / (2 * PI);
}

float LinearGradient(float2 gradientTexCoord, float gradRotation = 0)
{
    half gradientTime = cos(gradRotation) * (gradientTexCoord.x - 0.5) +
        sin(gradRotation) * (gradientTexCoord.y - 0.5) + 0.5;
    return gradientTime;
}

fixed4 SampleCornerGradient(float2 gradientTexCoord, in fixed4 colors[8], in fixed2 alphas[8])
{
    fixed4 topCol = lerp(fixed4(colors[0].rgb, alphas[0].x), fixed4(colors[1].rgb, alphas[1].x), gradientTexCoord.x);
    fixed4 bottomCol = lerp(fixed4(colors[2].rgb, alphas[2].x), fixed4(colors[3].rgb, alphas[3].x), gradientTexCoord.x);
    return lerp(topCol, bottomCol, gradientTexCoord.y);
}

fixed4 SampleGradient(float sampleValue, fixed4 colors[8], fixed2 alphas[8], int colorCount, int alphaCount,
                      int fixedOrBlend)
{
    fixed4 color = colors[0];
    fixed alpha = alphas[0].x;
    [unroll]
    for (int idx = 1; idx < 8; idx ++)
    {
        fixed prevTimeKey = colors[idx - 1].w;

        fixed colorPos = saturate((sampleValue - prevTimeKey) / (colors[idx].w - prevTimeKey)) * step(
            idx, colorCount - 1);
        color = lerp(color, colors[idx], lerp(colorPos, step(0.5, colorPos), fixedOrBlend));

        fixed alphaPos = (saturate((sampleValue - alphas[idx - 1].y) / (alphas[idx].y - alphas[idx - 1].y))) * step(
            idx, alphaCount - 1);
        alpha = lerp(alpha, alphas[idx].x, lerp(alphaPos, step(0.5, alphaPos), fixedOrBlend));
    }

    return fixed4(color.rgb, alpha);
}

// ---------------------- Text Structs and Functions ------------------------------------------------

// note -- must match non shader version 
struct GPUGlyphInfo
{
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
struct GPUFontInfo
{
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

struct TextMaterialInfoDecompressed
{
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

// layout must EXACTLY match TextMaterialInfo in C#
struct TextMaterialInfo
{
    // todo -- move text to different buffer from elements

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
    uint unused3;
    uint unused4;
    uint unused5;
    uint unused6;
    uint unused7;
    uint unused8;
    uint unused9;
    uint unused10;

    float maskSoftness;
    uint maskTopLeftUV;
    uint maskBottomRightUV;
    uint maskFlags;
};

TextMaterialInfoDecompressed DecompressTextMaterialInfo(in TextMaterialInfo textMaterialInfo)
{
    TextMaterialInfoDecompressed retn;

    retn.faceDilate = Remap(textMaterialInfo.faceUnderlayDilate & 0xffff, 0, (float)0xffff, -1, 1);
    retn.outlineWidth = GetByteN(textMaterialInfo.glowOffsetOutlineWS, 2) / (float)0xff;
    retn.outlineSoftness = GetByteN(textMaterialInfo.glowOffsetOutlineWS, 3) / (float)0xff;
    retn.glowPower = GetByteN(textMaterialInfo.glowPIOUnderlayS, 0) / (float)(0xff);
    retn.glowInner = max(0.1, GetByteN(textMaterialInfo.glowPIOUnderlayS, 1) / (float)(0xff));
    retn.glowOuter = max(0.1, GetByteN(textMaterialInfo.glowPIOUnderlayS, 2) / (float)(0xff));
    retn.glowOffset = Remap(textMaterialInfo.glowOffsetOutlineWS & 0xffff, 0, (float)0xffff, -1, 1);
    retn.underlaySoftness = GetByteN(textMaterialInfo.glowPIOUnderlayS, 3) / (float)0xff;
    retn.underlayDilate = Remap((textMaterialInfo.faceUnderlayDilate >> 16) & (1 << 16) - 1, 0, (float)0xffff, -1, 1);
    retn.underlayX = textMaterialInfo.underlayX;
    retn.underlayY = textMaterialInfo.underlayY;

    return retn;
}

float3 ComputeSDFTextScaleRatios(in GPUFontInfo fontInfo, in TextMaterialInfoDecompressed textStyle)
{
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

    float ratioCRange = (weight + faceDilate) * (gradientScale - 1);
    float ratioC_t = max(
        1, max(abs(textStyle.underlayX), abs(textStyle.underlayY)) + textStyle.underlayDilate + textStyle.
        underlaySoftness);

    float ratioC = max(0, gradientScale - 1.0 - ratioCRange) / (gradientScale * ratioC_t);

    return float3(ratioA, ratioB, ratioC);
}

float GetTextSDFPadding(float gradientScale, in TextMaterialInfoDecompressed textStyle, in float3 ratios)
{
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
    float dilate = textStyle.underlayDilate * scaleRatio_C;
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


// sample usage of sdf glow                
// float sx = 0.25;                      
// float altSDF = sdRoundBox(i.texCoord0.xy - 0.5, float2(sx, sx), sx * float4(0.0, 0.0, 0.0, 0.0));
// float glow = 1.0 / (abs(altSDF));
// glow *= 0.065; 
// glow = pow(glow, 3); //lerp(1, 3, frac(_Time.y)));
// color.rgb = glow * color.rgb; //float3(1.0, 0.5, 0.25);
// color.rgb = 1.0 - exp(-color.rgb);
// color.a *= 1.0 - smoothstep(0, sx, altSDF);
// return color;

float getGlow(float dist, float radius, float intensity)
{
    return pow(radius / dist, intensity);
}

// sample usage of rect shadow functions below                
// float sigma = lerp(1, 20, frac(_Time.y));
// float z = sigma / 2;
// float4 rect = float4(20, 20, i.size.x - 40, i.size.y - 40);
// float4 shadowRect = float4(float2(rect.x + sqrt(z), rect.y - sqrt(z)), rect.zw);
// float4 shadowCol = drawRectShadow(i.texCoord0.xy * i.size, shadowRect, RED, sigma);


// approximation to the gaussian integral [x, infty)
float gi(float x)
{
    float i6 = 1.0 / 6.0;
    float i4 = 1.0 / 4.0;
    float i3 = 1.0 / 3.0;

    if (x > 1.5) return 0.0;
    if (x < -1.5) return 1.0;

    float x2 = x * x;
    float x3 = x2 * x;

    if (x > 0.5) return .5625 - (x3 * i6 - 3. * x2 * i4 + 1.125 * x);
    if (x > -0.5) return 0.5 - (0.75 * x - x3 * i3);
    return 0.4375 + (-x3 * i6 - 3. * x2 * i4 - 1.125 * x);
}

// create a line shadow mask
float lineShadow(float2 border, float pos, float sigma)
{
    float pos1 = ((border.x - pos) / sigma) * 1.5;
    float pos2 = ((pos - border.y) / sigma) * 1.5;

    return 1.0 - abs(gi(pos1) - gi(pos2));
}

//
//// create a rect shadow by two line shadow
float rectShadow(float4 rect, float2 pt, float sigma)
{
    float lineV = lineShadow(float2(rect.x, rect.x + rect.z), pt.x, sigma);
    float lineH = lineShadow(float2(rect.y, rect.y + rect.w), pt.y, sigma);

    return lineV * lineH;
}

//
//// draw shadow
float4 drawRectShadow(float2 pos, float4 rect, float4 color, float sigma)
{
    float4 result = color;

    float shadowMask = rectShadow(rect, pos, sigma);

    result.a *= shadowMask;

    return result;
}

float4 blend(float4 src, float4 append)
{
    return float4(src.rgb * (1.0 - append.a) + append.rgb * append.a, 1.0 - (1.0 - src.a) * (1.0 - append.a));
}

float gauss(float x, float sigma)
{
    float sigmaPow2 = sigma * sigma;
    return 1.0 / sqrt(6.283185307179586 * sigmaPow2) * exp(-(x * x) / (2.0 * sigmaPow2));
}

float erf(float x)
{
    bool negative = x < 0.0;
    if (negative)
        x = -x;
    float x2 = x * x;
    float x3 = x2 * x;
    float x4 = x2 * x2;
    float denom = 1.0 + 0.278393 * x + 0.230389 * x2 + 0.000972 * x3 + 0.078108 * x4;
    float result = 1.0 - 1.0 / (denom * denom * denom * denom);
    return negative ? -result : result;
}

float erfSigma(float x, float sigma)
{
    return erf(x / (sigma * 1.4142135623730951));
}

float colorFromRect(float2 p0, float2 p1, float sigma)
{
    return ((erfSigma(p1.x, sigma) - erfSigma(p0.x, sigma)) * (erfSigma(p1.y, sigma) - erfSigma(p0.y, sigma))) / 4.0;
}

float ellipsePoint(float y, float y0, float2 radii)
{
    float bStep = (y - y0) / radii.y;
    return radii.x * sqrt(1.0 - bStep * bStep);
}

float colorCutoutGeneral(float x0l,
                         float x0r,
                         float y0,
                         float yMin,
                         float yMax,
                         float2 radii,
                         float sigma)
{
    float sum = 0.0;
    for (float y = yMin; y <= yMax; y += 1.0)
    {
        float xEllipsePoint = ellipsePoint(y, y0, radii);
        sum += gauss(y, sigma) *
        (erfSigma(x0r + radii.x, sigma) - erfSigma(x0r + xEllipsePoint, sigma) +
            erfSigma(x0l - xEllipsePoint, sigma) - erfSigma(x0l - radii.x, sigma));
    }
    return sum / 2.0;
}

float colorCutoutTop(float x0l, float x0r, float y0, float2 radii, float sigma)
{
    return colorCutoutGeneral(x0l, x0r, y0, y0, y0 + radii.y, radii, sigma);
}

// The value that needs to be subtracted to accommodate the bottom border corners.
float colorCutoutBottom(float x0l, float x0r, float y0, float2 radii, float sigma)
{
    return colorCutoutGeneral(x0l, x0r, y0, y0 - radii.y, y0, radii, sigma);
}

float color(float2 pos, float2 p0Rect, float2 p1Rect, float2 radii, float sigma)
{
    // Compute the vector distances `p_0` and `p_1`.
    float2 p0 = p0Rect - pos, p1 = p1Rect - pos;

    // Compute the basic color `"colorFromRect"_sigma(p_0, p_1)`. This is all we have to do if
    // the box is unrounded.
    float cRect = colorFromRect(p0, p1, sigma);
    // if (radii.x == 0.0 || radii.y == 0.0)
    // return cRect;
    //
    // // Compute the inner corners of the box, taking border radii into account: `x_{0_l}`,
    // // `y_{0_t}`, `x_{0_r}`, and `y_{0_b}`.
    float x0l = p0.x + radii.x;
    float y0t = p1.y - radii.y;
    float x0r = p1.x - radii.x;
    float y0b = p0.y + radii.y;
    //
    // // Compute the final color:
    // //
    // //     "colorFromRect"_sigma(p_0, p_1) -
    // //          ("colorCutoutTop"_sigma(x_{0_l}, x_{0_r}, y_{0_t}, a, b) +
    // //           "colorCutoutBottom"_sigma(x_{0_l}, x_{0_r}, y_{0_b}, a, b))
    float cCutoutTop = colorCutoutTop(x0l, x0r, y0t, radii, sigma);
    float cCutoutBottom = colorCutoutBottom(x0l, x0r, y0b, radii, sigma);
    return cRect - (cCutoutTop + cCutoutBottom);
}

#endif
