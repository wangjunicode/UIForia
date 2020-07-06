#ifndef __UIForiaInc__
#define __UIForiaInc__

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
            
bool UIForiaOverflowClip(float2 screenPos, int overflowIdx) {
    return true;
    // float4 p0 = _OverflowClippers[overflowIdx];
    // float4 p1 = _OverflowClippers[overflowIdx + 1];
    // return PointInTriangle(screenPos.xy, p0.xy, p0.zw, p1.xy) || PointInTriangle(screenPos.xy, p0.xy, p1.xy, p1.zw);
}
            
#endif