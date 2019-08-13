Shader "UIForia/UIForiaPathSDF"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}       
        _ShadowIntensity ("_ShadowIntensity", float) = 0
        
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
            
            float _ShadowIntensity;

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
            #define Frag_ShapeType i.texCoord2.z
            #define Frag_PaintMode i.texCoord2.w
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
                uint packedFlags = objectInfo.x;
                uint shapeType = (packedFlags >> 16) & (1 << 16) - 1;
                uint colorMode = packedFlags & 0xffff;
                
                UIForiaPathFragData  o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                // if pixel snapping is on some shapes get cut off. find a way to account for this
                //  o.vertex = UIForiaPixelSnap(o.vertex);
                o.texCoord0 = v.texCoord0;
                o.texCoord1 = v.texCoord1;
                o.texCoord2 = float4(size.x, size.y, shapeType, colorMode);
                return o;
            }
            
            float rectangle(float2 samplePosition, float2 halfSize){
                float2 componentWiseEdgeDistance = abs(samplePosition) - halfSize;
                float outsideDistance = length(max(componentWiseEdgeDistance, 0));
                float insideDistance = min(max(componentWiseEdgeDistance.x, componentWiseEdgeDistance.y), 0);
                return outsideDistance + insideDistance;
            }

            float SmoothRect(float2 uv, float2 pos, float2 size, float sX, float sY) {
               float2 end = pos + size;
               return smoothstep(pos.x - sX, pos.x + sX, uv.x) 
                   * (1.0 - smoothstep(end.x - sX, end.x + sX, uv.x))
                   * smoothstep(pos.y - sY, pos.y + sY, uv.y)
                   * (1.0 - smoothstep(end.y - sY, end.y + sY, uv.y));
            }
            
            fixed4 ShadowFragment(float2 pos) {

               float shadowSoftnessX = 0.25;// i.fragData2.x;
               float shadowSoftnessY = 0.25;//1 - 0.5; //i.fragData2.y;
               
               float shadowAlpha = 1; //i.fragData2.z;
               fixed4 shadowTint = fixed4(0, 1, 0, 0);//i.fragData3.r, i.fragData3.g, i.fragData3.b, 0);
               float2 shadowSize = float2(1, 1);//float2(0.8, 0.8);
               float2 shadowPosition =  float2((1 - shadowSize.x) * 0.5, (1 - shadowSize.y) * 0.5);
               
               float shadowRect = shadowAlpha * SmoothRect(pos, shadowPosition, shadowSize, shadowSoftnessX, shadowSoftnessY);
               
               float a = smoothstep(0.5, 0.8, shadowRect);
               fixed4 shadowColor = fixed4(1, 1,1, 1);
               fixed4 color = lerp(fixed4(shadowTint.rgb, 0), fixed4(shadowColor.rgb, shadowColor.a * a), a);
               color = lerp(fixed4(1, 1, 1, 0), color, a);
               color.rgb *= color.a;
               return color;
               
            }
          
            fixed4 frag (UIForiaPathFragData i) : SV_Target {              
                _ShadowIntensity = 50;
                float2 size = i.texCoord2.xy;
                float minSize = min(size.x, size.y);
            
                float4 objectInfo = _ObjectData[(int)Frag_ObjectIndex];
                float4 colorInfo = _ColorData[(int)Frag_ObjectIndex];
                uint packedFlags = (objectInfo.x);
                uint paintMode = packedFlags;//(packedFlags & 0xffff);
                
                fixed4 mainColor = ComputeColor(colorInfo.r, colorInfo.g, colorInfo.a, i.texCoord0.xy, _MainTex);
                mainColor.a *= colorInfo.b;
                
                float halfStrokeWidth = 0;// 0.5 * Frag_StrokeWidth;
                
                int isStroke = 0;
                int isShadow = paintMode == (1 << 4);
                int shapeType = ShapeType_RectLike;//Frag_ShapeType;
                fixed4 inner = mainColor;
                fixed4 outer = fixed4(mainColor.rgb, 0);
                
                inner.rgb *= inner.a;
                outer.rgb *= outer.a;
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
                
                if(isShadow) {
                    float n = smoothstep(-_ShadowIntensity, 0, sdf);        
                    fixed4 shadowColor = fixed4(1, 1, 1, 1);  
                    fixed4 shadowTint = fixed4(0, 1, 0, 0);
            
                    fixed4 color = lerp(fixed4(shadowColor.rgb, 0), shadowColor, 1 - n);                
                    color = lerp(fixed4(shadowTint.rgb, 0), fixed4(shadowColor.rgb, shadowColor.a * (1 - n)),  1 - n);
                    color.rgb *= color.a;
                    return color;       
                }
      
                float distanceChange = fwidth(sdf);
                float aa = smoothstep(distanceChange, -distanceChange, sdf);
                return lerp(inner, outer, 1 - aa);
                
            }

            ENDCG
        }
    }
}
