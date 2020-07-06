Shader "Unlit/Red"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FVal("FVal", Float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100
        
      
        Cull Off
//		Lighting Off
//		ZWrite Off
//		ZTest[unity_GUIZTestMode]
//		Blend SrcAlpha OneMinusSrcAlpha
//		ColorMask[_ColorMask]
		
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"
		
            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPosition : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
	        float4 _ClipRect;
	        
            v2f vert (appdata v)
            {
                v2f o;
                o.worldPosition = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                
                fixed4 color = tex2D(_MainTex, i.uv);
			    return color;
            }
            ENDCG
        }
    }
}
