#ifndef UIFORIA_SDF_INCLUDE
#define UIFORIA_SDF_INCLUDE

#include "./UIForiaStructs.cginc"

const float SQRT_2 = 1.4142135623730951;
            
                           
#define PaintMode_Color 1 << 0
#define PaintMode_Texture 1 << 1
#define PaintMode_TextureTint 1 << 2
#define PaintMode_LetterBoxTexture 1 << 3

// remap input from one range to an other            
float Map(float s, float a1, float a2, float b1, float b2) {
    return b1 + (s-a1) * (b2-b1) / (a2-a1);
}           
           
// same as UnityPixelSnap except taht we add 0.5 to pixelPos after rounding
inline float4 SDFPixelSnap (float4 pos) {
     float2 hpc = _ScreenParams.xy * 0.5f;
     float2 pixelPos = round ((pos.xy / pos.w) * hpc) + 0.5;
     pos.xy = pixelPos / hpc * pos.w;
     return pos;
 }

inline float RectSDF(float2 p, float2 size, float r) {
   float2 d = abs(p) - size + float2(r, r);
   return min(max(d.x, d.y), 0.0) + length(max(d, 0.0)) - r;   
}

float EllipseSDF(float2 p, float2 r) {
    float k0 = length(p/r);
    float k1 = length(p/(r*r));
    return k0*(k0-1.0)/k1;
}

float RhombusSDF(float2 p, float2 size) {
    #define ndot(a, b) (a.x * b.x - a.y * b.y)
    
    float2 q = abs(p);
    float h = clamp( (-2.0*ndot(q,size) + ndot(size,size) )/dot(size,size), -1.0, 1.0 );
    float d = length( q - 0.5*size*float2(1.0-h,1.0+h) );
    return d * sign( q.x*size.y + q.y*size.x - size.x*size.y );
}

float DiamondSDF(float2 P, float size) {
    const float SQRT_2 = 1.4142135623730951;

    float x = SQRT_2/2.0 * (P.x - P.y);
    float y = SQRT_2/2.0 * (P.x + P.y);
    return max(abs(x), abs(y)) - size/(2.0*SQRT_2);
}

float TriangleSDF(float2 p, float2 p0, float2 p1, float2 p2) {
    float2 e0 = p1-p0, e1 = p2-p1, e2 = p0-p2;
    float2 v0 = p -p0, v1 = p -p1, v2 = p -p2;

    float2 pq0 = v0 - e0*clamp( dot(v0,e0)/dot(e0,e0), 0.0, 1.0 );
    float2 pq1 = v1 - e1*clamp( dot(v1,e1)/dot(e1,e1), 0.0, 1.0 );
    float2 pq2 = v2 - e2*clamp( dot(v2,e2)/dot(e2,e2), 0.0, 1.0 );
    
    float s = sign( e0.x*e2.y - e0.y*e2.x );
    float2 d = min(min(float2(dot(pq0,pq0), s*(v0.x*e0.y-v0.y*e0.x)),
                     float2(dot(pq1,pq1), s*(v1.x*e1.y-v1.y*e1.x))),
                     float2(dot(pq2,pq2), s*(v2.x*e2.y-v2.y*e2.x)));

    return -sqrt(d.x) * sign(d.y);
}

// https://www.shadertoy.com/view/MtScRG
// todo figure out parameters and coloring, this probably is returning alpha value that needs to be smoothsteped
float PolygonSDF(float2 p, int vertexCount, float radius) {
    // two pi
    float segmentAngle = 6.28318530718 / (float)vertexCount;
    float halfSegmentAngle = segmentAngle*0.5;

    float angleRadians = atan2(p.y, p.x);
    float repeat = (angleRadians % segmentAngle) - halfSegmentAngle;
    float inradius = radius*cos(halfSegmentAngle);
    float circle = length(p);
    float x = sin(repeat)*circle;
    float y = cos(repeat)*circle - inradius;

    float inside = min(y, 0.0);
    float corner = radius*sin(halfSegmentAngle);
    float outside = length(float2(max(abs(x) - corner, 0.0), y))*step(0.0, y);
    return inside + outside;
}

half2 UnpackToHalf2(float value) {
	const int PACKER_STEP = 4096;
	const int PRECISION = PACKER_STEP - 1;
	half2 unpacked;

	unpacked.x = (value % (PACKER_STEP)) / (PACKER_STEP - 1);
	value = floor(value / (PACKER_STEP));

	unpacked.y = (value % PACKER_STEP) / (PACKER_STEP - 1);
	return unpacked;
}

float4 UnpackColor(uint input) {
    return fixed4(
        uint((input >> 0) & 0xff) / float(0xff),
        uint((input >> 8) & 0xff) / float(0xff),
        uint((input >> 16) & 0xff) / float(0xff),
        uint((input >> 24) & 0xff) / float(0xff)
    );
}

inline int and(int a, int b) {
    return a * b;
}

float4 UnpackSDFParameters(float packed) {
    uint packedInt = asuint(packed);
    int shapeType = packedInt & 0xff;
    return float4(shapeType, 0, 0, 0);
}

inline float4 UnpackSDFRadii(float packed) {
    uint packedRadii = asuint(packed);
    return float4(
        uint((packedRadii >>  0) & 0xff),
        uint((packedRadii >>  8) & 0xff),
        uint((packedRadii >> 16) & 0xff),
        uint((packedRadii >> 24) & 0xff)
    );
}

inline float2 UnpackSize(float packedSize) {
    uint input = asuint(packedSize);
    uint high = (input >> 16) & (1 << 16) - 1;
    uint low =  input & 0xffff;
    return float2(high / 10, low / 10);
}

float3x3 TRS2D(float2 position, float2 scale, float rotation) {
    const float a = 1;
    const float b = 0;
    const float c = 0;
    const float d = 1;
    const float e = 0;
    const float f = 0;
    float ca = 0;
    float sa = 0;
    
    sincos(rotation, ca, sa);  
    
    return transpose( float3x3(
        (a * ca + c * sa) * scale.x,
        (b * ca + d * sa) * scale.x, 0, 
        (c * ca - a * sa) * scale.y,
        (d * ca - b * sa) * scale.y, 0,
        a * position.x + c * position.y + e,
        b * position.x + d * position.y + f, 
        1
    ));
}
            
#define ShapeType_Rect (1 << 0)
#define ShapeType_RoundedRect (1 << 1)
#define ShapeType_Circle (1 << 2)
#define ShapeType_Ellipse (1 << 3)
#define ShapeType_Rhombus (1 << 4)
#define ShapeType_Triangle (1 << 5)
#define ShapeType_RegularPolygon (1 << 6)
#define ShapeType_Text (1 << 7)

#define ShapeType_RectLike (ShapeType_Rect | ShapeType_RoundedRect | ShapeType_Circle)

struct BorderData {
    fixed4 color;
    float size;
    float radius;
};

BorderData GetBorderData(float2 coords, float2 size, float4 packedBorderColors, float2 packedBorderSizes, float packedRadii) {
    float left = step(coords.x, 0.5); // 1 if left
    float bottom = step(coords.y, 0.5); // 1 if bottom
    
    #define top (1 - bottom)
    #define right (1 - left)  
    
    fixed4 borderColorTop = UnpackColor(asuint(packedBorderColors.x));
    fixed4 borderColorRight = UnpackColor(asuint(packedBorderColors.y));
    fixed4 borderColorBottom = UnpackColor(asuint(packedBorderColors.w));
    fixed4 borderColorLeft = UnpackColor(asuint(packedBorderColors.z));

    BorderData retn;

    uint packedRadiiUInt = asuint(packedRadii);
    
    float4 radii = float4(
        uint((packedRadiiUInt >>  0) & 0xff),
        uint((packedRadiiUInt >>  8) & 0xff),
        uint((packedRadiiUInt >> 16) & 0xff),
        uint((packedRadiiUInt >> 24) & 0xff)
    );
        
    float r = 0;
    r += (top * left) * radii.x;
    r += (top * right) * radii.y;
    r += (bottom * left) * radii.z;
    r += (bottom * right) * radii.w;
    retn.radius = (r * 2) / 1000;
    
    half2 topLeftBorderSize = UnpackSize(packedBorderSizes.x);
    half2 bottomRightBorderSize = UnpackSize(packedBorderSizes.y);
    
    #define borderTop topLeftBorderSize.y
    #define borderLeft topLeftBorderSize.x
    #define borderBottom bottomRightBorderSize.y
    #define borderRight bottomRightBorderSize.x
    
    // todo -- border size when only on 1 edge eg top but not left. don't do the triangle cut out for that case
    
    if((top * left) != 0) {
        float x = coords.x * size.x;
        float y = (1 - coords.y) * size.y;
        retn.color = lerp(borderColorLeft, borderColorTop, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderTop, borderLeft, x < y);
        return retn;  
    }
    
    if((top * right) != 0) {
        float x = (1 - coords.x) * size.x;
        float y = (1 - coords.y) * size.y;
        retn.color = lerp(borderColorRight, borderColorTop, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderTop, borderRight, x < y);
        return retn;  
    }
    
    if((bottom * left) != 0) {
        float x = (coords.x) * size.x;
        float y = (coords.y) * size.y;
        retn.color = lerp(borderColorLeft, borderColorBottom, smoothstep(-0.01, 0.01, x - y));
        retn.size = lerp(borderBottom, borderLeft, x < y);
        return retn;  
    }
    
    // bottom right case
    float x = (1 - coords.x) * size.x;
    float y = (coords.y) * size.y;
    retn.color = lerp(borderColorRight, borderColorBottom, smoothstep(-0.01, 0.01, x - y));
    retn.size = lerp(borderBottom, borderRight, x < y);
    return retn;      
}

// this will give great AA on rotated edges but for cases where the sides are vertical or horizontal 
// it will cause a ~1 pixel blend that looks terrible when placed side by side with another object
// size is size of the quad, distFromCenter is 0 - 1 where 0 is an edge          
inline fixed4 MeshBorderAA(fixed4 mainColor, float2 size, float distFromCenter) {
     // this tries to find a 1 or 2 pixel border from edges
     float borderSize = 1 / (min(size.x, size.y)) * 2;// could also be 1.41 as sqrt2 for pixel size
     
     if(mainColor.a > 0 && distFromCenter < borderSize && distFromCenter == 1) {
        fixed4 retn = fixed4(mainColor.rgb, 0);
        retn = lerp(retn, mainColor, distFromCenter / borderSize);
        retn.rgb *= distFromCenter / (borderSize);
        return retn;
     }
     
     return mainColor;
}
       
            
inline fixed4 ComputeColor(float packedBg, float packedTint, int colorMode, float2 texCoord, sampler2D _MainTexture) {

    int useColor = (colorMode & PaintMode_Color) != 0;
    int useTexture = (colorMode & PaintMode_Texture) != 0;
    int tintTexture = (colorMode & PaintMode_TextureTint) != 0;
    int letterBoxTexture = (colorMode & PaintMode_LetterBoxTexture) != 0;
    
    fixed4 bgColor = UnpackColor(asuint(packedBg));
    fixed4 tintColor = UnpackColor(asuint(packedTint));
    fixed4 textureColor = tex2D(_MainTexture, texCoord);

    bgColor.rgb *= bgColor.a;
    tintColor.rgb *= tintColor.a;
    textureColor.rgb *= textureColor.a;
    
    textureColor = lerp(textureColor, textureColor + tintColor, tintTexture);
    
    if (useTexture && letterBoxTexture && (texCoord.x < 0 || texCoord.x > 1) || (texCoord.y < 0 || texCoord.y > 1)) {
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


fixed4 SDFColor(SDFData sdfData, fixed4 borderColor, fixed4 contentColor, float distFromCenter) {
    float halfStrokeWidth = sdfData.strokeWidth * 0.5;
    float2 size = sdfData.size;
    float minSize = min(size.x, size.y);
    float radius = clamp(minSize * sdfData.radius, 0, minSize);
  
    float2 center = ((sdfData.uv.xy - 0.5) * size); 
    float fBlendAmount = 0;
    
    float shape1 = RectSDF(center, (size * 0.5) - halfStrokeWidth, radius - halfStrokeWidth);
    float retn = abs(shape1) - halfStrokeWidth;
    
    borderColor = lerp(contentColor, borderColor, halfStrokeWidth != 0);
    
    if(contentColor.a == 0) {
        contentColor = fixed4(borderColor.rgb, 0);
    }
    
    if(shape1 >= 0) {
       contentColor = lerp(fixed4(contentColor.rgb, 0), fixed4(borderColor.rgb, 0), halfStrokeWidth > 0);
    }
    
    float borderSize = (1 / minSize) *  1.4;

    // with a border -1, 1 looks the best, without use 0, 1
    fBlendAmount = smoothstep(lerp(-1, 0, halfStrokeWidth == 0), 1, retn);
       
   
    return lerp(contentColor, borderColor, 1 - fBlendAmount); // do not pre-multiply alpha here!
    
    // using -1 to 1 gives slightly better aa but all edges have alpha which is bad when two shapes share an edge
    // use larger negative to get nice blur effect
    // smoothstep(-1, 0, fDist) gives the best aa but there is a gap between shapes that should touch
    // smoothstep(0, 1, fDist) fixes the gap perfectly but causes rounded shapes to be slightly cut off at the bottom and right edges
}
         
#endif // UIFORIA_SDF_INCLUDE