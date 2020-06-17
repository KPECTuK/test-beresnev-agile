Shader "test-beresnev/color_select_ui"
{
	Properties
	{
		_MainTex ("Dummy", 2D) = "white" {}
		_bright ("_bright", Range(0.0, 1.0)) = 0.0
		
		_StencilComp ("Stencil Comparison", Float) = 8
		_Stencil ("Stencil ID", Float) = 0
		_StencilOp ("Stencil Operation", Float) = 0
		_StencilWriteMask ("Stencil Write Mask", Float) = 255
		_StencilReadMask ("Stencil Read Mask", Float) = 255

		_ColorMask ("Color Mask", Float) = 15

		[Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip ("Use Alpha Clip", Float) = 0
	}

	SubShader
	{
		Tags
		{
			"Queue"="Transparent"
			"IgnoreProjector"="True"
			"RenderType"="Transparent"
			"PreviewType"="Plane"
			"CanUseSpriteAtlas"="True"
		}

		Stencil
		{
				Ref [_Stencil]
				Comp [_StencilComp]
				Pass [_StencilOp]
				ReadMask [_StencilReadMask]
				WriteMask [_StencilWriteMask]
		}

		Cull Off
		Lighting Off
		ZWrite Off
		ZTest [unity_GUIZTestMode]
		Blend SrcAlpha OneMinusSrcAlpha
		ColorMask [_ColorMask]

		Pass
		{
			Name "ColorSelectUI"

			CGPROGRAM

			#pragma vertex vert
			#pragma fragment frag
			#pragma target 2.0

			#include "UnityCG.cginc"
			#include "UnityUI.cginc"

			#pragma multi_compile_local _ UNITY_UI_CLIP_RECT
			#pragma multi_compile_local _ UNITY_UI_ALPHACLIP

			float _bright;

			struct appdata_t
			{
				float4 vertex   : POSITION;
				float4 color    : COLOR;
				float2 texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
				float4 color    : COLOR0;
				float2 texcoord  : TEXCOORD0;
				float4 worldPosition : TEXCOORD1;
				UNITY_VERTEX_OUTPUT_STEREO
			};

			fixed4 _TextureSampleAdd;
			float4 _ClipRect;

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

			v2f vert(appdata_t v)
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

				o.worldPosition = v.vertex;
				o.vertex = UnityObjectToClipPos(o.worldPosition);
				o.texcoord = v.texcoord;
				o.color = v.color;
				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				float2 fromCenter = i.texcoord - float2(0.5, 0.5);
				float angle = atan2(fromCenter.y, fromCenter.x);
				float radius = length(fromCenter) * 2.0;
				float3 c = hue2hsb(float2(0.5 + angle / UNITY_TWO_PI, radius)) * _bright;
				float op = (1.0 - floor(radius)) * i.color.a;
				half4 color = float4(c, op);

				#ifdef UNITY_UI_CLIP_RECT
				color.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
				#endif

				#ifdef UNITY_UI_ALPHACLIP
				clip (color.a - 0.001);
				#endif

				return color;
			}

			ENDCG
		}
	}
}