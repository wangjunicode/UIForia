Shader "Unlit/ColorToColor"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
		Tags { "Queue" = "Transparent" }
        LOD 100

		Blend SrcAlpha OneMinusSrcAlpha // Traditional transparency

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(0, 0, 0, 0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);

				uint input = asuint(v.uv.r);

				o.color = float4(
					uint((input >> 0) & 0xff) / float(0xff),
					uint((input >> 8) & 0xff) / float(0xff),
					uint((input >> 16) & 0xff) / float(0xff),
					uint((input >> 24) & 0xff) / float(0xff)
					);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
            ENDCG
        } 
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(1.5,0,1.5,0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				uint input = asuint(v.uv.g);

				o.color = float4(
					uint((input >> 0) & 0xff) / float(0xff),
					uint((input >> 8) & 0xff) / float(0xff),
					uint((input >> 16) & 0xff) / float(0xff),
					uint((input >> 24) & 0xff) / float(0xff)
					);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(3,0,3,0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				uint input = asuint(v.uv.b);

				o.color = float4(
					uint((input >> 0) & 0xff) / float(0xff),
					uint((input >> 8) & 0xff) / float(0xff),
					uint((input >> 16) & 0xff) / float(0xff),
					uint((input >> 24) & 0xff) / float(0xff)
					);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float4 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;

			v2f vert(appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex + float4(4.5,0,4.5,0));
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				uint input = asuint(v.uv.a);

				o.color = float4(
					uint((input >> 0) & 0xff) / float(0xff),
					uint((input >> 8) & 0xff) / float(0xff),
					uint((input >> 16) & 0xff) / float(0xff),
					uint((input >> 24) & 0xff) / float(0xff)
					);
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return i.color;
			}
			ENDCG
		}
	}
}
