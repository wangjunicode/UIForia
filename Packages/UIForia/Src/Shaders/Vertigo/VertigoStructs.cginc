#ifndef VERTIGO_STRUCT_INCLUDE
#define VERTIGO_STRUCT_INCLUDE

struct appdata {
    float4 vertex : POSITION;
    float4 texCoord0 : TEXCOORD0;
    float4 texCoord1 : TEXCOORD1;
    fixed4 color : COLOR;
};

struct v2f {
    float4 vertex : SV_POSITION;
    float4 texCoord0 : TEXCOORD0;
    nointerpolation 
    float4 texCoord1 : TEXCOORD1;
    float4 sdfCoord  : TEXCOORD2;
    fixed4 color : COLOR0;
};

struct SDFData {
    float2 uv;
    float2 size;
    float radius;
    float strokeWidth;
    int shapeType;
};


#endif 