Shader "UIForia/SimpleStrokeOpaque"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" } // revert to opaque
        LOD 100
        Cull Off // todo set this to Back
                Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag

           #include "UnityCG.cginc"
           
            #define prev v.prevNext.xy
            #define curr v.vertex.xy
            #define next v.prevNext.zw
            #define extrude v.flags.x
            #define leftRight v.flags.y

           struct appdata {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 prevNext : TEXCOORD1;
                float4 flags : TEXCOORD2;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
           };

            // idx 0 == top left
            // idx 1 == top right
            // idx 2 == bottom left
            // idx 3 == bottom right
            // idx 4 == join point top left
            // idx 5 == join point top right
            // idx 6 == join point bottom center
            
            v2f vert (appdata v)
           {
                v2f o;
                float strokeWidth = 25;
                
                #define cap 1
                #define join 2
                
                int flag = (int)v.flags.z;
                int idx = (int)v.flags.y;
                
                float2 toCurrent = normalize(curr - prev);
                float2 toNext = normalize(next - curr);
                float2 tangent = normalize(toNext + toCurrent);
                float2 miter = float2(-tangent.y, tangent.x);
                float2 normal = float2(-toCurrent.y, toCurrent.x) * extrude;
                
                float miterLength = strokeWidth / dot(miter, normal);
                
                // todo -- encode this differently to avoid conditional
                
                int leftSide = idx == 0 || idx == 2;
                int rightSide = 1 - leftSide;
                int topSide = idx < 2;
                int bottomSide = idx == 2 || idx == 3;
                
                float2 toNextPerp = float2(-toNext.y, toNext.x);
                float2 toCurrentPerp = float2(-toCurrent.y, toCurrent.x);
                float segmentLength = distance(curr, prev);
                
                float dir = dot(toCurrent, toNextPerp) <= 0 ? -1 : 1;
                float2 miterVec = miter * miterLength;
                float2 vertWithOffset = v.vertex + miterVec;

                if(flag == join && idx < 4) {
                    // todo -- figure out how to remove this if
                    if(dir > 0) {
                        float2 leftRightVec = lerp(normal, toNextPerp, leftSide) * strokeWidth;
                        float2 topBottomVec = lerp(leftRightVec, miterVec, bottomSide);
                        vertWithOffset = v.vertex + topBottomVec;
                    }
                    else {
                        float2 topBottomVec = lerp(-toNextPerp, normal, rightSide) * strokeWidth;
                        float2 leftRightVec = lerp(topBottomVec, miterVec, topSide);
                        vertWithOffset = v.vertex + leftRightVec;
                    }
                    
                    // if distance from join point to offset point is greater than segment length, limit it to segment length

                    float2 topBottomInterp = lerp(toNextPerp, -toNextPerp, 1 - topSide);
                    topBottomInterp = lerp(topBottomInterp, normal, rightSide);
                    float2 originalPosition = v.vertex + (topBottomInterp * strokeWidth);
                    
                    if(distance(originalPosition, vertWithOffset) > segmentLength) {
                        vertWithOffset = originalPosition;
                    }
                }
                
                if(idx > 3) {
                    float2 joinPoint = lerp(toCurrentPerp, toNextPerp, idx == 4);
                    vertWithOffset = v.vertex + joinPoint * strokeWidth * dir;
                }
                
                if(idx == 6) {
                    vertWithOffset = v.vertex + miterVec * dir;
                    if(distance(v.vertex, vertWithOffset) > segmentLength) {
                        vertWithOffset = v.vertex;
                    }
                }
                
                //#if USE_ROUND_OR_MITER_CLIP_JOIN
                    // round join / clipped miter
                    if(idx > 6) {
                        vertWithOffset = -miterVec;
                        if(idx == 7) {
                            vertWithOffset = normal * strokeWidth;
                        }
                        else if(idx == 8) {
                            vertWithOffset = toNextPerp * strokeWidth;
                        }                      

                        vertWithOffset = v.vertex + vertWithOffset * dir;
                     
                    }
              //  #endif
                
                if(flag == cap) {
                    vertWithOffset = v.vertex + normal * strokeWidth;   
                }
                
                o.color = v.color;
                o.vertex = UnityObjectToClipPos(float3(vertWithOffset.xy, v.vertex.z)); 
                return o;
                
           }

           fixed4 frag (v2f i) : SV_Target
           {
               return i.color;
           }
           ENDCG
        }
    }
}
