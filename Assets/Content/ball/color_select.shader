Shader "test-beresnev/color_select"
{
	Properties
	{
		_bright ("_bright", Range(0.0, 1.0)) = 0.0
	}
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha OneMinusSrcAlpha
		LOD 100

		Pass
		{
			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"

			float _bright;

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

			float3 hue2hsb(float2 hue)
			{
				hue = frac(hue);
				float r = abs(hue.x * 6.0 - 3.0) - 1.0;
				float g = 2.0 - abs(hue.x * 6.0 - 2.0);
				float b = 2.0 - abs(hue.x * 6.0 - 4.0);
				float3 c = float3(r, g, b);
				c = saturate(c);
				return lerp(1.0, c, hue.y);
			}

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv;
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				float2 fromCenter = i.uv - float2(0.5, 0.5);
				float angle = atan2(fromCenter.y, fromCenter.x);
				float radius = length(fromCenter) * 2.0;
				float3 c = hue2hsb(float2(angle / UNITY_TWO_PI, radius)) * _bright;
				float op = 1.0 - floor(radius);
				return float4(c, op);
			}

			ENDCG
		}
	}
}
