// Upgrade NOTE: upgraded instancing buffer 'Props' to new syntax.

Shader "UIForia/Instanced"
{
    Properties
    {
//        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255

        _ColorMask ("Color Mask", Float) = 15

    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            Name "Default"
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
            #include "UIForia.cginc"
            
            sampler2D _MainTex;
//            fixed4 _Color;
            fixed4 _TextureSampleAdd; // unity sets this if texture is format alpha8
//            float4 _ClipRect;         // rect used to clip
            float4 _MainTex_ST;       // used for Stencil
            
            UNITY_INSTANCING_BUFFER_START(Props)
            UNITY_DEFINE_INSTANCED_PROP(float4, _Color) 
            UNITY_DEFINE_INSTANCED_PROP(fixed4, _BorderColor) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Width) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Height) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _Roundness) 
            UNITY_DEFINE_INSTANCED_PROP(float4, _ClipRect) 
            UNITY_INSTANCING_BUFFER_END(Props)
            
            struct appdata_t {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f {
                float4 vertex   : SV_POSITION;
                float2 texcoord  : TEXCOORD0;
                float2 texcoordMinusHalf  : TEXCOORD1;
                float4 fillColor : COLOR;
            	UNITY_VERTEX_INPUT_INSTANCE_ID
            };
            
            #define m_Width UNITY_ACCESS_INSTANCED_PROP(Props, _Width)
            #define m_Height UNITY_ACCESS_INSTANCED_PROP(Props, _Height)
            #define m_FillColor UNITY_ACCESS_INSTANCED_PROP(Props, _Color)
//            #define m_BorderColor UNITY_ACCESS_INSTANCED_PROP(Props, _BorderColor)
            #define m_BorderColor fixed4(255, 0, 255, 255)
            #define m_Roundness UNITY_ACCESS_INSTANCED_PROP(Props, _Roundness)
            #define m_ClipRect UNITY_ACCESS_INSTANCED_PROP(Props, _ClipRect)
            #define m_BorderSize 0
            #define m_InnerBlur 0
            #define m_OuterBlur 0
            
            v2f vert(appdata_t v)  {
                v2f OUT;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, OUT);
                v.vertex.x += v.normal.x * m_Width;
                v.vertex.y -= v.normal.y * m_Height;
                OUT.vertex = UnityObjectToClipPos(v.vertex);
                OUT.texcoord = v.texcoord;
                OUT.texcoordMinusHalf = v.texcoord.xy - 0.5;
                OUT.fillColor = m_FillColor;
                return OUT;
            }
            
            fixed4 frag(v2f IN) : SV_Target {
                UNITY_SETUP_INSTANCE_ID(IN);
 
                float width = m_Width;
                float height = m_Height;
                
                float2 pos = float2(IN.texcoordMinusHalf.x * width, IN.texcoordMinusHalf.y * height);
                
                // clip rect is in texture space with origin top left
                // texcoord origin is bottom left, invert to match
                float2 p = float2(IN.texcoord.x, 1 - IN.texcoord.y);
                // step is the function a < b ? 0 : 1;
                // if either coord of inside is 0, pixel is not contained in clip rect
                float4 clipRect = m_ClipRect;
                float2 inside = step(clipRect.xy, p) * step(p, clipRect.zw);    

                //color.a *= inside.x * inside.y;
                clip(inside.x * inside.y);
                
                FillSettings fillSettings;
                fillSettings.fillColor1 = IN.fillColor;
                    
                float pixelSize = (_ScreenParams.z - 1) * OrthoCameraWidth * 2;
                float blur = sqrt(pixelSize * pixelSize * 2);
                
                int indexer = 0;
                int left = step(0.5, IN.texcoord.x);
                int top = step(0.5, IN.texcoord.y);
                
                indexer += and(top, left) * 1;
                indexer += and(1 - top, 1 - left) * 2;
                indexer += and(1 - top, left) * 3;
                
                float4 border = float4(20, 10, 0, 10);
                float outlineSize = border[indexer];
                int hasOutline = step(1, outlineSize);

                // subtract the AA blur from the outline so sizes stay mostly 
                // correct and outlines don't look too thick when scaled down.
                // clamp size to zero, we should use originalOutlineSize for comparison checks
//                outlineSize = lerp(max(0, outlineSize - blur), outlineSize, hasOutline); 
                if(outlineSize > 0) {
                    outlineSize = outlineSize - blur;
                }
                // todo blur calcs can be done in vertex shader except for outline size subtract
                float2 halfSize = float2(width * 0.5, height * 0.5);
                float halfMinDimension = min(width, height) * 0.5;
                float outerBlur = max(min(blur, halfMinDimension - outlineSize), 0);
                float innerBlur = max(min(outerBlur, halfMinDimension - outerBlur - outlineSize), 0);
                    
                BlurSettings blurSettings;
                blurSettings.outerBlur = outerBlur;
                blurSettings.innerBlur = innerBlur;
                blurSettings.outlineSize = outlineSize;
                blurSettings.hasOutline = hasOutline;
                        
                float4 cachedRoundness = float4(400, 400, 400, 400);//m_Roundness;
                float totalRoundness = cachedRoundness.x + cachedRoundness.y + cachedRoundness.z + cachedRoundness.w;
                fixed4 fillColor = fixed4(255, 0, 0, 255);
                fixed4 borderColor = fixed4(255, 255, 0, 255);
          
              //  if(totalRoundness > 0) {
                    float tl = and(when_le(pos.x, 0), when_ge(pos.y, 0)) * cachedRoundness.x;
                    float tr = and(when_ge(pos.x, 0), when_ge(pos.y, 0)) * cachedRoundness.y;
                    float bl = and(when_le(pos.x, 0), when_le(pos.y, 0)) * cachedRoundness.z;
                    float br = and(when_ge(pos.x, 0), when_le(pos.y, 0)) * cachedRoundness.w;
                    float roundness = (tl + tr + bl + br);// * halfMinDimension;
                    
                    float radius = min(min(width / 2, roundness), height / 2);//roundness, halfMinDimension);
                    
                    float2 extents = float2(width, height) / 2 - radius;
                    float2 delta = abs(pos) - extents;
                  
                    // first component is distance to closest side when not in a corner circle,
                    // second is distance to the rounded part when in a corner circle
                    float dist = radius - (min(max(delta.x, delta.y), 0) + length(max(delta, 0)));
//                    if(dist <= 1) return fixed4(0, 0, 0, 255);
                    fixed4 color = color_from_distance(dist, fillColor, borderColor, blurSettings);

                    clip(color.a - 0.001);

                    return color;
              //  }
                
            }
            
        ENDCG
        }
    }
}
