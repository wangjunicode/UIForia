// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "Internal-GUIRoundedRect"
{
    Properties {
        _MainTex ("Texture", any) = "white" {}
        _SrcBlend("SrcBlend", Int) = 5 // SrcAlpha
        _DstBlend("DstBlend", Int) = 10 // OneMinusSrcAlpha
        _Color("Color", Color) = (1,1,1,1)
        [Toggle]
        _ShouldMask("ShouldMask", Int) = 1
        _Rect("ScreenSpaceRect", Vector) = (0, 0, 0, 0)
        _BorderWidths("BorderWidths", Vector) = (0, 0, 0, 0)
        _BorderRadii("BorderRadii", Vector) = (0, 0, 0, 0)
    }

    CGINCLUDE
    #pragma vertex vert
    #pragma fragment frag
    #pragma target 2.5

    #include "UnityCG.cginc"

    struct appdata_t {
        float4 vertex : POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
    };

    struct v2f {
        float4 vertex : SV_POSITION;
        fixed4 color : COLOR;
        float2 texcoord : TEXCOORD0;
        float2 clipUV : TEXCOORD1;
        float4 pos : TEXCOORD2;
    };

    sampler2D _MainTex;
    sampler2D _GUIClipTexture;
    uniform bool _ManualTex2SRGB;
    uniform int _SrcBlend;

    uniform float4 _MainTex_ST;
    uniform float4x4 unity_GUIClipTextureMatrix;

    uniform float _CornerRadiuses[4];
    uniform float _BorderWidths[4];
    uniform float _Rect[4];
    uniform int _ShouldMask;

    half GetCornerAlpha(float2 p, float2 center, float borderWidth1, float borderWidth2, float radius, float pixelScale, bool shouldMask)
    {
        bool hasBorder = borderWidth1 > 0.0f || borderWidth2 > 0.0f;

        float2 v = p - center;
        float pixelCenterDist = length(v);

        float outRad = radius;
        float outerDist = (pixelCenterDist - outRad) * pixelScale;
        half outerDistAlpha = hasBorder ? saturate(0.5f + outerDist) : 0.0f;

        float a = radius - borderWidth1;
        float b = radius - borderWidth2;

        v.y *= a/b;
        half rawDist = (length(v) - a) * pixelScale;
        half alpha = saturate(rawDist + 0.5f);
        half innerDistAlpha = hasBorder ? ((a > 0 && b > 0) ? alpha : 1.0f) : 0.0f;
        
        // this turns mask on / off       
        return shouldMask && (outerDistAlpha == 0.0f) ? innerDistAlpha : (1.0f - outerDistAlpha);
    }
    
    bool GetCornerAlpha2(float2 p, float2 center, float borderWidth1, float borderWidth2, float radius, float pixelScale, bool shouldMask)
    {
        bool hasBorder = borderWidth1 > 0.0f || borderWidth2 > 0.0f;

        float2 v = p - center;
        float pixelCenterDist = length(v);

        float outRad = radius;
        float outerDist = (pixelCenterDist - outRad) * pixelScale;
        half outerDistAlpha = hasBorder ? saturate(0.5f + outerDist) : 0.0f;

        float a = radius - borderWidth1;
        float b = radius - borderWidth2;

        v.y *= a/b;
        half rawDist = (length(v) - a) * pixelScale;
        half alpha = saturate(rawDist + 0.5f);
        half innerDistAlpha = hasBorder ? 
            ((a > 0 && b > 0) ? alpha : 1.0f) 
            : 0.0f;
            
        // if outerDistAlpha is one then the point is on the ring.
        return outerDistAlpha != 1;    
        // this turns mask on / off       
//        return shouldMask && (outerDistAlpha == 0.0f) ? innerDistAlpha : (1.0f - outerDistAlpha);
    }

    bool IsPointInside(float2 p, float4 rect)
    {
        return p.x >= rect.x && p.x <= (rect.x+rect.z) && p.y >= rect.y && p.y <= (rect.y+rect.w);
    }

    v2f vert (appdata_t v)
    {
        float3 eyePos = UnityObjectToViewPos(v.vertex);
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.color = v.color;
        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
        o.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
        o.pos = v.vertex;
        return o;
    }

    fixed4 frag (v2f i) : SV_Target
    {
        // input values are in screen space
        _Rect[0] = 300;
        _Rect[1] = 300;
        _Rect[2] = 300;
        _Rect[3] = 300;
        _BorderWidths[0] = 10;
        _BorderWidths[1] = 10;
        _BorderWidths[2] = 10;
        _BorderWidths[3] = 10;
        _CornerRadiuses[0] = 150;
        _CornerRadiuses[1] = 150;
        _CornerRadiuses[2] = 150;
        _CornerRadiuses[3] = 150;
        float pixelScale = 1.0f/abs(ddx(i.pos.x));

        half4 borderColor = half4(1, 1, 1, 1);
        half4 contentColor = half4(1, 0, 0, 1);
        
        half4 col = contentColor;//half4(0, 0, 1, 1);//tex2D(_MainTex, i.texcoord);
        
        if (_ManualTex2SRGB)
            col.rgb = LinearToGammaSpace(col.rgb);
        col *= i.color;

        float2 p = i.pos.xy;

        float cornerRadius2 = _CornerRadiuses[0] * 2.0f;
        float middleWidth = _Rect[2] - cornerRadius2;
        float middleHeight = _Rect[3] - cornerRadius2;

        bool xIsLeft = (p.x - _Rect[0] - _Rect[2]/2.0f) <= 0.0f;
        bool yIsTop = (p.y - _Rect[1] - _Rect[3]/2.0f) <= 0.0f;

        float bw1 = _BorderWidths[0];
        float bw2 = _BorderWidths[1];

        int radiusIndex = 0;
        if (xIsLeft)
            radiusIndex = yIsTop ? 0 : 3;
        else
            radiusIndex = yIsTop ? 1 : 2;

        float activeRadius = _CornerRadiuses[radiusIndex];
        float2 center = float2(_Rect[0]+activeRadius, _Rect[1]+activeRadius);

        if (!xIsLeft)
        {
            center.x = (_Rect[0]+_Rect[2]-activeRadius);
            bw1 = _BorderWidths[2];
        }
        if (!yIsTop)
        {
            center.y = (_Rect[1]+_Rect[3]-activeRadius);
            bw2 = _BorderWidths[3];
        }

        bool isOnRing = GetCornerAlpha(p, center, bw1, bw2, activeRadius, pixelScale, _ShouldMask);
        
//        return lerp(half4(1, 1, 1, 1), half4(0, 0, 0, 1), isOnRing);
        
        bool isInCorner = (xIsLeft ? p.x <= center.x : p.x >= center.x) && (yIsTop ? p.y <= center.y : p.y >= center.y);
        float cornerAlpha = isInCorner ? GetCornerAlpha(p, center, bw1, bw2, activeRadius, pixelScale, _ShouldMask) : 1.0f;
        // corner alpha will be 1 if it is on the ring
        col.a *= cornerAlpha;
        
        if(cornerAlpha == 1) {
            return half4(1,1,1,1);
        }
        
        float4 centerRect = float4(_Rect[0]+_BorderWidths[0], _Rect[1]+_BorderWidths[1], _Rect[2]-(_BorderWidths[0]+_BorderWidths[2]), _Rect[3]-(_BorderWidths[1]+_BorderWidths[3]));
        bool isPointInCenter = IsPointInside(p, centerRect);

        // this turns mask on / off only for the spots in the 'corners' of the rect
        half middleMask = isPointInCenter ? 0.0f : 1.0f;
        
        bool hasBorder = _BorderWidths[0] > 0 ||  _BorderWidths[1] > 0 ||  _BorderWidths[2] > 0 ||  _BorderWidths[3] > 0;
        float borderAlpha = hasBorder ? (isInCorner ? 1.0f : middleMask) : 1.0f;
        float2 v = p - center;
        float pixelCenterDist = length(v);
        
        // on ring and border
        if(isOnRing && !isPointInCenter) {
            return half4(0, 1, 1, 1);
        }
        else if(isOnRing) { // on ring but not on border (red)
            return half4(contentColor.rgb, cornerAlpha);
        }
        
        if(isPointInCenter) {
            return half4(0, 0, 0,1);    
        }
        
        // corners outside borders
        if(borderAlpha == 1 && !isPointInCenter && isInCorner) {
            return half4(0, 1, 1, col.a * cornerAlpha * borderAlpha);
        }
        
        return half4(0.5, 0.5, 0.5, 1);
        
        col.a *= borderAlpha;

        float clipAlpha = tex2D(_GUIClipTexture, i.clipUV).a;
        col.a *= clipAlpha;

        // If the source blend is not SrcAlpha (default) we need to multiply the color by the rounded corner
        // alpha factors for clipping, since it will not be done at the blending stage.
        if (_SrcBlend != 5) // 5 SrcAlpha
        {
            col.rgb *= cornerAlpha * borderAlpha * clipAlpha;
        }
        return col;
    }
    ENDCG

    SubShader {
        Blend [_SrcBlend] [_DstBlend], One OneMinusSrcAlpha
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

    SubShader {
        Blend [_SrcBlend] [_DstBlend]
        Cull Off
        ZWrite Off
        ZTest Always

        Pass {
            CGPROGRAM
            ENDCG
        }
    }

FallBack "Hidden/Internal-GUITextureClip"
}


//// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)
//
//Shader "InternalGUIRoundedRect"
//{
//    Properties {
//        _MainTex ("Texture", any) = "white" {}
//        _SrcBlend("SrcBlend", Int) = 5 // SrcAlpha
//        _DstBlend("DstBlend", Int) = 10 // OneMinusSrcAlpha
//        _Width("Width", float) = 300
//        _Height("Height", float) = 300
//        
//        //_CornerRadiuses("Corners", Vector) = (0, 0, 0, 0)
//    }
//
//    CGINCLUDE
//    #pragma vertex vert
//    #pragma fragment frag
//    #pragma target 2.5
//
//    #include "UnityCG.cginc"
//
//    struct appdata_t {
//        float4 vertex : POSITION;
//        fixed4 color : COLOR;
//        float2 texcoord : TEXCOORD0;
//    };
//
//    struct v2f {
//        float4 vertex : SV_POSITION;
//        fixed4 color : COLOR;
//        float2 texcoord : TEXCOORD0;
//        float2 clipUV : TEXCOORD1;
//        float4 pos : TEXCOORD2;
//    };
//
//    sampler2D _MainTex;
//    sampler2D _GUIClipTexture;
//    uniform bool _ManualTex2SRGB;
//    uniform int _SrcBlend;
//
//    uniform float4 _MainTex_ST;
//    uniform float4x4 unity_GUIClipTextureMatrix;
////
//    uniform float _CornerRadiuses[4];
//   uniform float _BorderWidths[4];
////    uniform float _Rect[4];
//    float4 _Rect;
//    float _Width;
//    float _Height;
//    //float4 _CornerRadiuses;
//
//    half GetCornerAlpha(float2 p, float2 center, float borderWidth1, float borderWidth2, float radius, float pixelScale)
//    {
//        bool hasBorder = borderWidth1 > 0.0f || borderWidth2 > 0.0f;
//
//        float2 v = p - center;
//        float pixelCenterDist = length(v);
//
//        float outRad = radius;
//        float outerDist = (pixelCenterDist - outRad) * pixelScale;
//        half outerDistAlpha = hasBorder ? saturate(0.5f + outerDist) : 0.0f;
//
//        float a = radius - borderWidth1;
//        float b = radius - borderWidth2;
//
//        v.y *= a/b;
//        half rawDist = (length(v) - a) * pixelScale;
//        half alpha = saturate(rawDist+0.5f);
//        half innerDistAlpha = hasBorder ? ((a > 0 && b > 0) ? alpha : 1.0f) : 0.0f;
//
//        return (outerDistAlpha == 0.0f) ? innerDistAlpha : (1.0f - outerDistAlpha);
//    }
//
//    bool IsPointInside(float2 p, float4 rect)
//    {
//        return p.x >= rect.x && p.x <= (rect.x+rect.z) && p.y >= rect.y && p.y <= (rect.y+rect.w);
//    }
//
//    v2f vert (appdata_t v)
//    {
//        float3 eyePos = UnityObjectToViewPos(v.vertex);
//        v2f o;
//        o.vertex = UnityObjectToClipPos(v.vertex);
//        o.color = v.color;
//        o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex) - 0.5;
//        o.clipUV = mul(unity_GUIClipTextureMatrix, float4(eyePos.xy, 0, 1.0));
//        o.pos = v.vertex;
//        return o;
//    }
//
//    fixed4 frag (v2f i) : SV_Target
//    {
//        fixed4 white = fixed4(1, 1, 1, 1);
//        fixed4 black = fixed4(0, 0, 0, 1);
//        float pixelScale = 1.0f/abs(ddx(i.pos.x));
// 
//        half4 col = tex2D(_MainTex, i.texcoord);
//        if (_ManualTex2SRGB)
//            col.rgb = LinearToGammaSpace(col.rgb);
//        col *= i.color;
//
//        float2 p = i.pos.xy;
//
//
//        bool xIsLeft = i.texcoord.x < 0.5;//(p.x - _Rect.x - _Rect.z / 2.0f) <= 0.0f;
//        bool yIsTop =  i.texcoord.y > 0.5f;//(p.y - _Rect.y - _Rect.w / 2.0f) <= 0.0f;
//        
//        float rectSpaceX = _Width * i.texcoord.x;
//        float rectSpaceY = _Height * i.texcoord.y;
//        
//      
//        
//        float bw1 = _BorderWidths[0];
//        float bw2 = _BorderWidths[1];
//
//        int radiusIndex = 0;
//        if (xIsLeft)
//            radiusIndex = yIsTop ? 0 : 3;
//        else
//            radiusIndex = yIsTop ? 1 : 2;
//
//        float activeRadius = 0.2;//20;//_CornerRadiuses[radiusIndex];
//        float2 center = float2(0.2, 0.2);//activeRadius, activeRadius);// + activeRadius, rectSpaceY + activeRadius);
//        fixed4 color = (1, 0, 0, 1);
//        
////        if(distance(center, fixed2(rectSpaceX, rectSpaceY)) <= activeRadius) {
////           color = (1, 0, 0, 1);
////        }
//        
//        float dist = distance(center, i.texcoord);//length(i.texcoord);
//        float pwidth = length(float2(ddx(dist), ddy(dist)));
//        float alpha = smoothstep(0.5, 0.5 - pwidth * 1.5, dist);
//        if(alpha > 0.01) {
//            return fixed4(color.rgb, color.a * alpha);
//        }
//        return black;
////        return black;
//        
//        // try to find the center of one of the 4 corner points
//        if (!xIsLeft)
//        {
//            //return fixed4(1, 0, 0, 1);
//            center.x = (_Rect.x + _Rect.z - activeRadius);
//            bw1 = _BorderWidths[2];
//        }
//        if (!yIsTop)
//        {
//            center.y = (_Rect.y+_Rect.w-activeRadius);
//            bw2 = _BorderWidths[3];
//        }
//
//     
//        bool isInCorner = (xIsLeft ? p.x <= center.x : p.x >= center.x) && (yIsTop ? p.y <= center.y : p.y >= center.y);
//        
//        if(isInCorner) {
//            return fixed4(1,1,1,1);
//        }
//        return fixed4(0,0,0,1);
//        float cornerAlpha = isInCorner ? GetCornerAlpha(p, center, bw1, bw2, activeRadius, pixelScale) : 1.0f;
//        col.a *= cornerAlpha;
//
//        float4 centerRect = float4(_Rect.x+_BorderWidths[0], _Rect.y+_BorderWidths[1], _Rect.z-(_BorderWidths[0]+_BorderWidths[2]), _Rect.w-(_BorderWidths[1]+_BorderWidths[3]));
//        bool isPointInCenter = IsPointInside(p, centerRect);
//
//        half middleMask = isPointInCenter ? 0.0f : 1.0f;
//        bool hasBorder = _BorderWidths[0] > 0 || _BorderWidths[1] > 0 || _BorderWidths[2] > 0 || _BorderWidths[3] > 0;
//        float borderAlpha = hasBorder ? (isInCorner ? 1.0f : middleMask) : 1.0f;
//        col.a *= borderAlpha;
//
//        float clipAlpha = tex2D(_GUIClipTexture, i.clipUV).a;
//        col.a *= clipAlpha;
//
//        // If the source blend is not SrcAlpha (default) we need to multiply the color by the rounded corner
//        // alpha factors for clipping, since it will not be done at the blending stage.
//        if (_SrcBlend != 5) // 5 SrcAlpha
//        {
//            col.rgb *= cornerAlpha * borderAlpha * clipAlpha;
//        }
//        return col;
//    }
//    ENDCG
//
//    SubShader {
//
//        Tags { "ForceSupported" = "True" "RenderType"="Overlay" }
//
//        Lighting Off
//        Blend SrcAlpha OneMinusSrcAlpha, One One
//        Cull Off
//        ZWrite Off
//        ZTest Always
//
//        Pass {
//            CGPROGRAM
//            ENDCG
//        }
//    }
//
//    SubShader {
//        Blend [_SrcBlend] [_DstBlend]
//        Cull Off
//        ZWrite Off
//        ZTest Always
//
//        Pass {
//            CGPROGRAM
//            ENDCG
//        }
//    }
//
//FallBack "Hidden/Internal-GUITextureClip"
//}
