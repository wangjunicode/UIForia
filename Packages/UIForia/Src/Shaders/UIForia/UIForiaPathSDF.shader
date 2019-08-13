Shader "UIForia/UIForiaPathSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        LOD 100
        Blend One OneMinusSrcAlpha
        Cull Off // lines often come in reversed
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ BATCH_SIZE_SMALL BATCH_SIZE_MEDIUM BATCH_SIZE_LARGE BATCH_SIZE_HUGE BATCH_SIZE_MASSIVE

            
            #include "./BatchSize.cginc"
            #include "UnityCG.cginc"
            #include "UIForiaSDFUtil.cginc"
            

            struct PathSDFAppData {
                float4 vertex : POSITION;
                float4 texCoord0 : TEXCOORD0;
                float4 texCoord1 : TEXCOORD1;
            };

            struct UIForiaPathFragData {
                float4 vertex : SV_POSITION;
                float4 texCoord0 : TEXCOORD0;
                nointerpolation float4 texCoord1 : TEXCOORD1;
                nointerpolation float4 texCoord2 : TEXCOORD2;
            };
            
            #define Vert_ObjectIndex v.texCoord1.w
            #define Frag_ObjectIndex i.texCoord1.w
            #define Frag_StrokeWidth objectInfo.w
            #define ObjectInfo_CornerRadii objectInfo.y

            float4 _ObjectData[BATCH_SIZE];
            float4 _ColorData[BATCH_SIZE];

            // todo -- better not to use a 4x4, paths are always 2d
            float4x4 _TransformData[BATCH_SIZE];

            sampler2D _MainTex;
            float4 _MainTex_ST;

            UIForiaPathFragData vert (appdata v) {
            
                float4 objectInfo = _ObjectData[(int)Vert_ObjectIndex];
                half2 size = UnpackSize(objectInfo.z);

                UIForiaPathFragData  o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // if pixel snapping is on some shapes get cut off. find a way to account for this
                //  o.vertex = UIForiaPixelSnap(o.vertex);
                o.texCoord0 = v.texCoord0;

                o.texCoord1 = v.texCoord1;
                o.texCoord2 = float4(size.x, size.y, 0, 0);
                return o;
            }

            fixed4 frag (UIForiaPathFragData i) : SV_Target {
                
                float2 size = i.texCoord2.xy;
                float minSize = min(size.x, size.y);

                float4 objectInfo = _ObjectData[(int)Frag_ObjectIndex];
                float4 colorInfo = _ColorData[(int)Frag_ObjectIndex];
                float halfStrokeWidth = 0;// 0.5 * Frag_StrokeWidth;
                int isStroke = 0;
                int shapeType = objectInfo.x;
                                                
                fixed4 mainColor = ComputeColor(colorInfo.r, colorInfo.g, colorInfo.a, i.texCoord0.xy, _MainTex);
                mainColor.a *= colorInfo.b;
                float sdf = 0;

                if(shapeType == ShapeType_Ellipse) {
                    halfStrokeWidth = halfStrokeWidth / max(size.x, size.y);
                    sdf = EllipseSDF(i.texCoord0.xy - 0.5, float2(0.49, 0.49));
                }
                else if((shapeType & ShapeType_RectLike) != 0) {      
                    float percentRadius = UnpackCornerRadius(ObjectInfo_CornerRadii, i.texCoord0.zw);
                    float radius = clamp(minSize * percentRadius, 0, minSize);
                    float2 center = (i.texCoord0.xy - 0.5) * size;
                    sdf = RectSDF(center,  (size * 0.5) - halfStrokeWidth, radius - halfStrokeWidth);
                }
                else {
                    return mainColor;
                }
              
                //sdf = lerp(sdf, abs(sdf) - halfStrokeWidth, isStroke);
                return smoothstep(-5, 1, sdf);
                float distanceChange = fwidth(sdf);
                float aa = smoothstep(distanceChange, -distanceChange, sdf);
            
                fixed4 inner = mainColor;
                fixed4 outer = fixed4(mainColor.rgb, 0);
                
                inner.rgb *= inner.a;
                outer.rgb *= outer.a;
                
                return lerp(inner, outer, 1 - aa);
                
            }
            
            ENDCG
        }
    }
}
