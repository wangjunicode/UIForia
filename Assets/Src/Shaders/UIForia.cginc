#define PI 3.141592653
#define PI2 6.283185307

#define OrthoCameraWidth unity_OrthoParams.x
#define OrthoCameraHeight unity_OrthoParams.y

struct BlurSettings {
    float innerBlur;
    float outerBlur;
    float outlineSize;
    int hasOutline;
};

struct FillSettings {
    fixed4 fillColor1;
    fixed4 fillColor2;
    fixed4 outlineColor;
    float fillRotation;
    float fillOffsetX;
    float fillOffsetY;
    float fillScaleX;
    float fillScaleY;
    int gradientType;
    int gradientAxis;
    float gradientStart;
    float gridSize;
    float lineSize;
};

// ***** 
// begin http://theorangeduck.com/page/avoiding-shader-conditionals
// *****

float4 when_eq(float4 x, float4 y) {
  return 1.0 - abs(sign(x - y));
}

float4 when_neq(float4 x, float4 y) {
  return abs(sign(x - y));
}

float4 when_gt(float4 x, float4 y) {
  return max(sign(x - y), 0.0);
}

float4 when_lt(float4 x, float4 y) {
  return max(sign(y - x), 0.0);
}

float4 when_ge(float4 x, float4 y) {
  return 1.0 - when_lt(x, y);
}

float4 when_le(float4 x, float4 y) {
  return 1.0 - when_gt(x, y);
}

float4 and(float4 a, float4 b) {
  return a * b;
}

float4 or(float4 a, float4 b) {
  return min(a + b, 1.0);
}

float4 xor(float4 a, float4 b) {
  return (a + b) % 2.0;
}

float4 not(float4 a) {
  return 1.0 - a;
}

// *****
// end http://theorangeduck.com/page/avoiding-shader-conditionals
// *****

// this seems a little nicer than smoothstep for antialiasing, though
// not necessarily for large amounts of blurring...there are tradeoffs,
// might want to separate blur from antialias
float blur(float edge1, float edge2, float amount) {
    return clamp(lerp(0, 1, (amount - edge1) / (edge2 - edge1)), 0, 1);
    // return clamp(pow(max(0, amount - edge1), 2) / pow(edge2 - edge1, 2), 0, 1);
    // return clamp((amount - edge1) / (edge2 - edge1), 0, 1);
    // amount = clamp(amount, edge1, edge2);
    // return pow((amount - edge1) / max(edge2 - edge1, 0.0001), 0.9);
    // amount = clamp((amount - edge1) / (edge2 - edge1), 0.0, 1.0);
    // return amount*amount*amount*(amount*(amount*6 - 15) + 10);
}

fixed4 fill_blend(float dist, fixed4 fillColor, float outerBlur) {
    float alpha = blur(0, outerBlur, dist);
    fixed4 color = fillColor;
    color.a *= alpha;
    return color;
}

fixed4 outline_fill_blend(float dist, fixed4 fillColor, fixed4 outlineColor, float outerBlur, float outline, float innerBlur) {
    float mix = blur(outline, innerBlur, dist);
    fixed4 color = lerp(outlineColor, fillColor, mix);
    float alpha = blur(0, 1, dist / outerBlur);
    color.a *= alpha;
    return color;
}

float2 rotate_fill(float2 fpos, float rotation) {
    float2 old_fpos = fpos;
    fpos.x = old_fpos.x * cos(rotation) - old_fpos.y * sin(rotation);
    fpos.y = old_fpos.x * sin(rotation) + old_fpos.y * cos(rotation);
    return fpos;
}

fixed4 color_from_distance(float dist, fixed4 fillColor, fixed4 outlineColor, BlurSettings settings) {
        return fill_blend(dist, fillColor, settings.blur);
    if (settings.hasOutline == 0) {
    } else {
        float outline = settings.outerBlur + settings.outlineSize;
        float innerBlur = outline + settings.innerBlur;
        return outline_fill_blend(dist, fillColor, outlineColor, settings.outerBlur, outline, innerBlur);
    }
}


// returns the fill color for the given uv point in the quad.
// @param uv the uv coords from -0.5 to 0.5
fixed4 fill(float2 uv, FillSettings settings) {
    float2 fpos = float2(uv.x, uv.y);
    
    #if FILL_NONE
        return fixed4(0, 0, 0, 0);
        
    #elif FILL_OUTLINE_COLOR
        return settings.outlineColor;
        
    #elif FILL_SOLID_COLOR
        return settings.fillColor1;
    #else
        return settings.fillColor1;
        
        /*
    #elif FILL_GRADIENT
        // gradient
        // todo - simplify and try to remove conditionals
        fpos = rotate_fill(fpos);
        fpos += float2(_FillOffsetX, _FillOffsetY);
        float gmin = 0, gmax = 0, current = 0;
        if (_GradientType == 0) {
            // linear gradient
            if (_GradientAxis == 0) {
                gmin = -_XScale / 2 + _GradientStart * _XScale;
                gmax = _XScale / 2;
                current = fpos.x;
            } else {
                gmin = -_YScale / 2 + _GradientStart * _YScale;
                gmax = _YScale / 2;
                current = fpos.y;
            }
        } else if (_GradientType == 1) {
            // cylindrical gradient
            if (_GradientAxis == 0) {
                gmin = _GradientStart / 2 * _XScale;
                gmax = _XScale / 2;
                current = abs(fpos.x);
            } else {
                gmin = _GradientStart / 2 * _YScale;
                gmax = _YScale / 2;
                current = abs(fpos.y);
            }
        } else {
            // radial gradient
            gmax = length(float2(0.5, 0.5));
            gmin = gmax * _GradientStart;
            current = length(fpos);
        }
        
        if (current < gmin) {
            return _FillColor;
        }
        
        if (gmax == gmin) {
            return _FillColor2;
        }
        
        return lerp(_FillColor, _FillColor2, (current - gmin) / (gmax - gmin));
    #elif FILL_GRID
        // grid - background is _FillColor, lines are _FillColor2
        fpos = rotate_fill(fpos);
        fpos += float2(_FillOffsetX, _FillOffsetY);
        // float edge = min(fwidth(fpos) * 2, _GridSize);
        float edge = min(_PixelSize * 2, _GridSize);
        // webgl breaks with component-wise ops for some reason and only shows vertical
        // grid lines...sigh
        // float2 p = abs(frac(fpos / _GridSize) * _GridSize * 2 - _GridSize);
        // float2 mix = smoothstep(_GridSize - _LineSize - edge, _GridSize - _LineSize, p);
        // return lerp(_FillColor, _FillColor2, max(mix.x, mix.y));
        float px = abs(frac(fpos.x / _GridSize) * _GridSize * 2 - _GridSize);
        float py = abs(frac(fpos.y / _GridSize) * _GridSize * 2 - _GridSize);
        float mixx = smoothstep(_GridSize - _LineSize - edge, _GridSize - _LineSize, px);
        float mixy = smoothstep(_GridSize - _LineSize - edge, _GridSize - _LineSize, py);
        return lerp(_FillColor, _FillColor2, max(mixx, mixy));
    #elif FILL_CHECKERBOARD
        // checkerboard
        fpos = rotate_fill(fpos);
        fpos += float2(_FillOffsetX, _FillOffsetY);
        // float edge = min(fwidth(fpos), _GridSize);
        float edge = min(_PixelSize, _GridSize);
        float2 p = frac(fpos / _GridSize);
        float2 mix = smoothstep(0, edge / _GridSize, p);
        float tile = abs(floor(fpos.y / _GridSize) + floor(fpos.x / _GridSize)) % 2;
        fixed4 color1 = tile * _FillColor + (1 - tile) * _FillColor2;
        fixed4 color2 = tile * _FillColor2 + (1 - tile) * _FillColor;
        return lerp(color1, color2, min(mix.x, mix.y));
    #elif FILL_STRIPES
        // stripes
        fpos = rotate_fill(fpos);
        fpos += float2(_FillOffsetX, _FillOffsetY);
        // float edge = min(fwidth(fpos) * 2, _GridSize);
        float edge = min(_PixelSize * 2, _GridSize);
        float p = abs(frac(fpos.x / _GridSize) * _GridSize * 2 - _GridSize);
        float mix = smoothstep(_GridSize - _LineSize - edge, _GridSize - _LineSize, p);
        return lerp(_FillColor, _FillColor2, mix);
    #elif FILL_TEXTURE
        // texture
        fpos = rotate_fill(fpos);
        fpos /= float2(_XScale, _YScale);
        fpos += float2(0.5, 0.5);
        fpos += float2(_FillOffsetX, _FillOffsetY);
        fpos /= float2(_FillScaleX, _FillScaleY);
        return tex2D(_FillTexture, fpos);
        */
    #endif
}

