Shader "test-beresnev/skybox"
{
	Properties
	{
		_colorTop ("_colorTop", Color) = (1, 1, 1, 0)
		_colorMid ("_colorMid", Color) = (1, 1, 1, 0)
		_colorBottom ("_colorBottom", Color) = (1, 1, 1, 0)
		_upVector ("_upVector", Vector) = (0, 1, 0, 0)
		_intensity ("_intensity", Float) = 1.0
		_exponentTop ("_exponentTop", Float) = 1.0
		_exponentBottom ("_exponentBottom", Float) = 1.0
	}

	CGINCLUDE

	#include "UnityCG.cginc"

	struct appdata_t
	{
		float4 position : POSITION;
		float3 texcoord : TEXCOORD0;
	};
		
	struct v2f
	{
		float4 position : SV_POSITION;
		float3 texcoord : TEXCOORD0;
	};
		
	half4 _colorTop;
	half4 _colorMid;
	half4 _colorBottom;
	half4 _upVector;
	half _intensity;
	half _exponentTop;
	half _exponentBottom;
		
	v2f vert(appdata_t v)
	{
			v2f o;
			o.position = UnityObjectToClipPos(v.position);
			o.texcoord = v.texcoord;
			return o;
	}
		
	fixed4 frag(v2f i) : COLOR
	{
		//half d = dot(normalize(i.texcoord), _upVector) * 0.5f + 0.5f;
		//return lerp(_colorBottom, _colorTop, pow(d, _exponent)) * _intensity;

		float p = normalize (i.texcoord).y;
		float p1 = 1.0f - pow(min(1.0f, 1.0f - p), _exponentTop);
		float p3 = 1.0f - pow(min(1.0f, 1.0f + p), _exponentBottom);
		float p2 = 1.0f - p1 - p3;
		return (_colorTop * p1 + _colorMid * p2 + _colorBottom * p3) * _intensity;
	}

	ENDCG

	SubShader
	{
		Tags
		{ 
			"RenderType"="Background" 
			"Queue"="Background"
		}
		Pass
		{
			ZWrite Off
			Cull Off
			Fog { Mode Off }

			CGPROGRAM
			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma vertex vert
			#pragma fragment frag
			ENDCG
		}
	}
}