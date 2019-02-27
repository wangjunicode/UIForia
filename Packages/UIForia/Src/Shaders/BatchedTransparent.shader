Shader "UIForia/BatchedTransparent" {
    
    Properties {  
      _StencilRef ("Stencil ID", Float) = 1
    }
    
    SubShader {

        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
        Cull Back 
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite On
        ColorMask RGBA
        
//        Stencil {
//            Ref [_StencilRef]
//            Comp Equal
//        }
//         
        Pass {
           CGPROGRAM
           #pragma vertex vert
           #pragma fragment frag
           #pragma target 3.0
           #include "UnityCG.cginc"
           #include "UIForiaInc.cginc"
           
           #define antialias 2
           
           uniform sampler2D _MainTex;
           uniform sampler2D _globalGradientAtlas;
           uniform sampler2D _globalFontTexture;
           
           uniform float _globalGradientAtlasSize;
           
           uniform float4 _globalFontData1; // WeightNormal, WeightBold, TextureSizeX, TextureSizeY
           uniform float4 _globalFontData2; // GradientScale, ScaleRatioA, ScaleRatioB, ScaleRatioC
                    
           // todo -- with some bit packing we can remove uv3 and uv4 from the app data input
                               
           struct appdata {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD0;
                float4 uv1 : TEXCOORD1;
                float4 uv2 : TEXCOORD2;
                float4 uv3 : TEXCOORD3;
                float4 uv4 : TEXCOORD4;
                fixed4 color : COLOR;
           };

           struct v2f {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float4 flags : TEXCOORD0;
                float4 uv : TEXCOORD1;
                float4 fragData1 : TEXCOORD2;
                float4 fragData2 : TEXCOORD3;
                float4 fragData3 : TEXCOORD4;
                fixed4 secondaryColor : COLOR1; // todo -- remove
           };

           #include "BatchedTransparentText.cginc"
           #include "BatchedTransparentLine.cginc"

           // todo -- use multi-compile flags to only compile the SDF functions a given draw actually needs
           fixed4 SDFShape(v2f i, int shapeType, int strokeShape) {
               float2 drawSurfaceSize = i.uv.zw;
               float strokeWidth = lerp(0, i.fragData3.x, strokeShape);
               
               fixed4 fromColor = i.color;
               fixed4 toColor = Clear;
               
               float left = step(i.uv.x, 0.5); // 1 if left
               float bottom = step(i.uv.y, 0.5); // 1 if bottom
               float top = 1 - bottom;
               float right = 1 - left;
               
               float4 radii = i.fragData2;
               
               int radiusIndex = 0;
               radiusIndex += and(top, right) * 1;
               radiusIndex += and(bottom, left) * 2;
               radiusIndex += and(bottom, 1 - left) * 3;
               
               float2 pixelCoord = float2(i.uv.x, 1 - i.uv.y) * drawSurfaceSize;
               
               float radius = clamp(radii[radiusIndex], 0, min(drawSurfaceSize.x * 0.5, drawSurfaceSize.y * 0.5));
               
               float halfStrokeWidth = strokeWidth * 0.5;
               float2 halfShapeSize = (drawSurfaceSize * 0.5) - halfStrokeWidth;
               float2 center = i.uv.xy - 0.5;
               
               float fDist = 0;
               float4 widths = float4(20, 8, 8, 20);
               if (shapeType == ShapeType_Rect || shapeType == ShapeType_RoundedRect) {
                   fDist = RectSDF(center * drawSurfaceSize, halfShapeSize, radius - halfStrokeWidth);
                   
               }
               else if(shapeType == ShapeType_Circle) {
                   fDist = length(center * drawSurfaceSize) - halfShapeSize;
               }
               else if(shapeType == ShapeType_Ellipse) {
                   fDist = EllipseSDF(center * drawSurfaceSize, halfShapeSize);
               }
               
               // todo -- support squircles
               
               if(halfStrokeWidth > 0) {
                   if(fDist <= 0) {
                       toColor = Clear;
                   }
                   fDist = abs(fDist) - halfStrokeWidth;
               }
               else {
                   toColor = Clear;
               }
               
               float fBlendAmount = smoothstep(-1, 1, fDist);
               return lerp(fromColor, toColor, fBlendAmount);

           }
           
           v2f FillVertex(appdata v, uint shapeType, uint renderType) {
                
                uint fillFlags = (uint)v.uv1.y;
                
                uint texFlag = (fillFlags & FillMode_Texture) != 0;
                uint gradientFlag = (fillFlags & FillMode_Gradient) != 0;
                uint tintFlag = (fillFlags & FillMode_Tint) != 0;
                uint gradientTintFlag = (fillFlags & FillMode_GradientTint) != 0;
                
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color;
                o.flags = float4(renderType, shapeType, v.uv1.yz);
                o.secondaryColor = fixed4(0, 0, 0, 0);
                o.fragData1 = float4(texFlag, gradientFlag, tintFlag, gradientTintFlag);
                o.fragData2 = v.uv2;
                o.fragData3 = v.uv3;
                
                return o;
           }

           fixed4 FillFragment(v2f i, int strokeShape) {

               #define ShapeType i.flags.y
               #define GradientId i.flags.z
               #define GradientDirection i.flags.w
               
               fixed4 color = SDFShape(i, ShapeType, strokeShape);
               
               // todo -- see if we can drop the discard statement
               if(color.a - 0.001 <= 0) {
                   discard;
               }
               
               float t = lerp(i.uv.x, 1 - i.uv.y, GradientDirection);
               float y = GetPixelInRowUV(GradientId, _globalGradientAtlasSize);
               fixed4 textureColor = tex2Dlod(_MainTex, float4(i.uv.xy, 0, 0));
               fixed4 gradientColor = tex2Dlod(_globalGradientAtlas, float4(t, y, 0, 0));
               fixed4 tintColor = lerp(White, color, i.fragData1.z);                
               
               textureColor = lerp(color, textureColor, i.fragData1.x);
               gradientColor = lerp(White, gradientColor, i.fragData1.y);
               tintColor = lerp(tintColor, gradientColor, i.fragData1.w);
               
               fixed4 tintedTextureColor = lerp(textureColor, textureColor * tintColor, tintColor.a);
           
               color = lerp(tintedTextureColor, gradientColor, 0);
        
               // todo -- see if we can drop the discard statement
               if(color.a - 0.001 <= 0) {
                   discard;
               }
                  
               return color;
          }
          
          fixed4 ShadowFragment(v2f i) {

               float shadowSoftnessX = i.fragData2.x;
               float shadowSoftnessY = i.fragData2.y;
               float shadowAlpha = i.fragData2.z;
               fixed4 shadowTint = fixed4(i.fragData3.r, i.fragData3.g, i.fragData3.b, 0);
               float2 shadowPosition =  float2(0.1, 0.1);
               float2 shadowSize = float2(0.8, 0.8);
               
               float shadowRect = shadowAlpha * SmoothRect(i.uv.xy, shadowPosition, shadowSize, shadowSoftnessX, shadowSoftnessY);
               
               float a = shadowRect;
               fixed4 shadowColor = i.color;
               fixed4 color = lerp(shadowTint, fixed4(shadowColor.rgb, shadowColor.a * a), a);
               return color;
               
          }
           
          v2f vert (appdata input) {
               int renderData = (int)input.uv1.x;
               int renderType = (renderData & 0xffff);
               uint shapeType = (renderData >> 16) & (1 << 16) - 1;

               if(renderType == RenderType_Fill || renderType == RenderType_Shadow) {
                   return FillVertex(input, shapeType, renderType);
               }
               else if(renderType == RenderType_Text) {
                   return TextVertex(input);
               }
               else if(renderType == RenderType_StrokeShape) {
                   return FillVertex(input, shapeType, renderType);
               }
               else {
                   return LineVertex(input);
               }
           }

            fixed4 frag (v2f i) : SV_Target {

               if(i.flags.x == RenderType_Fill) {
                   return FillFragment(i, 0);
               }
               else if(i.flags.x == RenderType_Text) {
                   return TextFragment(i);
               }
               else if(i.flags.x == RenderType_StrokeShape) {
                   return FillFragment(i, 1);
               }
               else if(i.flags.x == RenderType_Shadow) {
                   return ShadowFragment(i);
               }
               else {
                   return LineFragment(i);
               }
           }
           
            ENDCG
        }
    }
}
