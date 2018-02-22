Shader "Unlit/Aura"
{
	Properties
	{
		_MainColor ("Main Color", Color) = (1,1,1,1)
		_DistanceTex ("Distance", 2D) = "white" {}
		_DistortTex ("Distort", 2D) = "white" {}
		_TileDistort("Distort Tiling", Float) = 2.0
		_OffsetDistort("Distort Offset", Float) = 0.0
		_DistortAmount("Distort Amount", Range(0,0.5)) = 0.12
		_GlowSize("Glow Size", Range(0.01,5.0)) = 2.0
		_GlowHardness("Glow Hardness", Range(1.0,8.0)) = 5.0
	}
	SubShader
	{
		Tags { "RenderType"="Transparent" "Queue"="Transparent"}
		ZWrite Off

		LOD 100

		Pass
		{
			Blend SrcAlpha OneMinusSrcAlpha, One One
			BlendOp Add,Min

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0
			#pragma alpha:blend
			#include "UnityCG.cginc"

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

	//		sampler2D _MainTex;
			float4 _MainColor;
			sampler2D _DistanceTex;
			sampler2D _DistortTex;
			float _TileDistort;
			float _OffsetDistort;
			float _DistortAmount;
			float _GlowSize;
			float _GlowHardness;
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv =v.uv;
				return o;
			}

			float2 convert(float2 uv, float2 origin) {
				float PI = 3.141;
			    float s = 2.0/PI;
			    float r = length(uv-origin);
			    float theta = fmod(atan2(uv.y - origin.y, uv.x - origin.x) + PI/2.0 + PI, 2.0*PI) - PI;
			    return float2(s * theta, -r*3.0) + origin;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				// sample the texture
				fixed4 col = _MainColor;

				fixed4 distance = tex2D(_DistanceTex, i.uv);

				fixed4 distort = tex2D(_DistortTex, float2(_OffsetDistort,0) + _TileDistort*convert(i.uv, float2(0.5,0.5))) ;
				fixed4 distort2 = tex2D(_DistortTex, float2(-_OffsetDistort+0.5,0) + _TileDistort*convert(i.uv, float2(0.5,0.5))) ;

				distance.a += _DistortAmount*max(distort.a,distort2.a);
			
				col.a = col.a*smoothstep(0,1,distance.a*_GlowHardness - 1./_GlowSize);


				return col;
			}
			ENDCG
		}
	}
}
