Shader "Vertigo/VertigoSDF"
{
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _MaskTexture ("Mask", 2D) = "white" {}
        _MaskSoftness ("Mask",  Range (0.001, 1)) = 0
        _Radius ("Radius",  Range (1, 200)) = 0
        [Toggle] _InvertMask ("Invert Mask",  Int) = 0
    }
    SubShader {
        Tags {
         "RenderType"="Transparent"
         "Queue" = "Transparent"
        }
        LOD 100
        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha
        
        // this stencil setting solves self-blending
        // does mean we have to issue the draw call twice probably
        // if we want to reset the stencil
//        Stencil {
//            Ref 0
//            Comp Equal
//            Pass IncrSat 
//            Fail IncrSat
//        }
        Pass {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #pragma multi_compile __ SDF_RECTLIKE SDF_TRIANGLE SDF_TEXT SDF_RHOMBUS
            
            #include "UnityCG.cginc"
            #include "./VertigoSDFUtil.cginc"
            
            sampler2D _MainTex;
            sampler2D _MaskTexture;
            float4 _MainTex_ST;
            float4 _Color;
            float _Radius;
            float _MaskSoftness;
            float _InvertMask;
            
            v2f vert (appdata v) {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertex = UnityPixelSnap(o.vertex);
                o.color = v.color;
                o.texCoord0 = v.texCoord0;
                o.texCoord1 = v.texCoord1;
                o.sdfCoord = UnpackSDFCoordinates(v.texCoord1.z, v.texCoord1.w);
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target {
                SDFData sdfData = UnpackSDFData(i.texCoord1, i.sdfCoord);
                fixed4 mainColor = SDFColor(sdfData, i.color);
                
              
                
                //fixed maskAlpha = saturate(tex2D(_MaskTexture, i.texCoord0.xy).a / _MaskSoftness);
                //maskAlpha = lerp(1 - maskAlpha, maskAlpha, _InvertMask);
               // mainColor.a *= maskAlpha;
              
                return mainColor;
            }

            ENDCG
        }
    }
}
