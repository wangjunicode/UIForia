               
#define FillMode_Color 0
#define FillMode_Texture (1 << 0)
#define FillMode_Gradient (1 << 1)
#define FillMode_Tint (1 << 2)
#define FillMode_GradientTint (FillMode_Tint | FillMode_Gradient)

#define ShapeType_Rect 1
#define ShapeType_RoundedRect 2
#define ShapeType_Path 3
#define ShapeType_Circle 4
#define ShapeType_Ellipse 5

#define StrokePlacement_Center 0
#define StrokePlacement_Inside 1
#define StrokePlacement_Center 2

#define RenderType_Fill 0
#define RenderType_Text 1
#define RenderType_Stroke 2

#define Red fixed4(1, 0, 0, 1)
#define Green fixed4(0, 1, 0, 1)
#define Blue fixed4(0, 0, 1, 1)
#define White fixed4(1, 1, 1, 1)
#define Black fixed4(0, 0, 0, 1)

 // 0.5 is to target center of texel, otherwise we get bad neighbor blending
inline float GetPixelInRowUV(int targetY, float textureHeight) {
    return (targetY + 0.5) / textureHeight;
}

inline uint GetByte0(uint value) {
    return value & 0xff;
}

inline uint GetByte1(uint value) {
    return (value >> 8) & 0xff;
}

inline uint GetByte2(uint value) {
    return (value >> 16) & 0xff;
}

inline uint GetByte3(uint value) {
    return (value >> 24) & 0xff;
}