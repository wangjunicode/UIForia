Shader "UIForia/UIForiaDissolve" {
    Properties {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex("Noise", 2D) = "white" {}
        _EffectFactor ("Effect Factor", Range (0.0, 1)) = 0
        _Softness ("Softness", Range (0.0, 1)) = 0
        _Width ("Width", Range (0.0, 15)) = 0
        _DissolveColor ("Dissolve Color", Color) = (0.0, 1, 0, 1)
    }
    SubShader {
        Tags {
            "RenderType"="Opaque"
        }
        LOD 100

        Pass {
            Cull Off // configurable is probably the best approach for this
            ColorMask [_UIForiaColorMask]
            Stencil {
                Ref [_UIForiaStencilRef]
                Comp [_UIForiaStencilComp]
                Pass [_UIForiaStencilOp]
            }
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
#pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            float _Width;
            float _EffectFactor;
            float _Softness;
            fixed4 _DissolveColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _NoiseTex;

            fixed4 ApplyTransitionEffect(half4 color, half2 transParam)
            {
                float alpha = tex2D(_NoiseTex, transParam).r;
                fixed width = _Width / 4; //param.y / 4;
                fixed softness = _Softness;
                fixed3 dissolveColor = _DissolveColor.rgb; //tex2D(_ParamTex, float2(0.75, transParam.z)).rgb;
                float factor = alpha - _EffectFactor * (1 + width) + width;
                fixed edgeLerp = step(factor, color.a) * saturate((width - factor) * 16 / softness);
                // color = ApplyColorEffect(color, fixed4(dissolveColor, edgeLerp));
                fixed4 factorColor = fixed4(dissolveColor, edgeLerp);
                color.rgb = lerp(color.rgb, color.rgb * factorColor.rgb, factorColor.a);
                color.a *= saturate((factor) * 32 / softness);
                return color;
            }


            v2f vert(appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half4 color = tex2D(_MainTex, i.uv);
                return ApplyTransitionEffect(color, i.uv);
            }
            ENDCG
        }
    }
}