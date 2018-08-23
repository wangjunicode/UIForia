// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

Shader "UIElement/Standard"
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
    uniform int _SrcBlend;

    uniform float4 _MainTex_ST;
    uniform float4x4 unity_GUIClipTextureMatrix;

    uniform Vector _BorderColor;
    uniform Vector _ContentColor;
    uniform Vector _RectVector;
    uniform Vector _BorderWidthVector;
    uniform Vector _BorderRadiiVector;
    uniform int _ShouldMask;

    /*
        Takes a corner of the rect and calculates the alpha for the pixels in it, assuming we have a border radius;  
    */
    half GetCornerAlpha(float2 p, float2 center, float borderWidth1, float borderWidth2, float radius, float pixelScale, bool shouldMask) {
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

    bool IsPointInside(float2 p, float4 rect) {
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
        _RectVector.x = 300;
        _RectVector.y = 300;
        _RectVector.z = 300;
        _RectVector.w = 300;
        
        _BorderWidthVector.x = 10;
        _BorderWidthVector.y = 10;
        _BorderWidthVector.z = 10;
        _BorderWidthVector.w = 10;
        
        _BorderRadiiVector.x = 10;
        _BorderRadiiVector.y = 10;
        _BorderRadiiVector.z = 10;
        _BorderRadiiVector.w = 10;
        _ShouldMask = false;
        
        _BorderColor = half4(1, 1, 1, 1);
        _ContentColor = half4(1, 0, 0, 1);
        
        float pixelScale = 1.0f / abs(ddx(i.pos.x));
        
        half4 col = tex2D(_MainTex, i.texcoord);
            
        col *= i.color;

        float2 p = i.pos.xy;

        bool xIsLeft = (p.x - _RectVector.x - _RectVector.z / 2.0f) <= 0.0f;
        bool yIsTop  = (p.y - _RectVector.y - _RectVector.w / 2.0f) <= 0.0f;

        float bw1 = _BorderWidthVector.x;
        float bw2 = _BorderWidthVector.y;

        int radiusIndex = 0;
        float activeRadius = 0;
        half2 center = half2(0, 0);
        
        if (xIsLeft) {
            activeRadius = lerp(_BorderRadiiVector.w, _BorderRadiiVector.x, yIsTop);//yIsTop ? 0 : 3;
            center.x = _RectVector.x + activeRadius;
            center.y = _RectVector.y + activeRadius;
        } else {
            activeRadius = lerp(_BorderRadiiVector.z, _BorderRadiiVector.y, yIsTop);//yIsTop ? 1 : 2;
            center.x = _RectVector.x + _RectVector.z - activeRadius;
            center.y = _RectVector.y + activeRadius;
            bw1 = _BorderWidthVector.z;
        }
                
        center.y = lerp(_RectVector.y + _RectVector.w - activeRadius, center.y, yIsTop);
        bw2 = lerp(_BorderWidthVector.w, bw2, yIsTop);
                
        bool isInCorner = 
            (xIsLeft
                ? p.x <= center.x 
                : p.x >= center.x)
          &&
            (yIsTop
                ? p.y <= center.y 
                : p.y >= center.y);
           
        
        float cornerAlpha = isInCorner ? GetCornerAlpha(p, center, bw1, bw2, activeRadius, pixelScale, _ShouldMask) : 1.0f;
        
        float4 centerRect = float4(
            _RectVector.x + _BorderWidthVector.x,
            _RectVector.y + _BorderWidthVector.y,
            _RectVector.z - (_BorderWidthVector.x + _BorderWidthVector.z),
            _RectVector.w - (_BorderWidthVector.y + _BorderWidthVector.w)
        );
        
        bool isPointInCenter = IsPointInside(p, centerRect);

        // this turns mask on / off only for the spots in the 'corners' of the rect
        half middleMask = _ShouldMask && isPointInCenter ? 0.0f : 1.0f;
        
        bool hasBorder = _BorderWidthVector.x > 0 
            || _BorderWidthVector.y > 0 
            || _BorderWidthVector.z > 0
            || _BorderWidthVector.w > 0;
        
        float borderAlpha = hasBorder ? (isInCorner ? 1.0f : middleMask) : 1.0f;
        
        // on ring and border: cornerAlpha == 1 && !pointInCenter
        // on ring but not on border: (cornerAlpha == 1)
        // corner space outside border: borderAlpha == 1 && !pointInCenter && isInCorner;
        
        col.rgb = lerp(_ContentColor.rgb, _BorderColor.rgb, (cornerAlpha == 1 && !isPointInCenter));
        float clipAlpha = tex2D(_GUIClipTexture, i.clipUV).a;
        col.a *= cornerAlpha;
        col.a *= borderAlpha;
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
