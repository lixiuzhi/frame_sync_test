// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Shadow/Self_Illlumin_Diffuse"
{
	Properties{
		_Color("Main Color", Color) = (1, 1, 1, 1)
		_MainTex("Base (RGB) Gloss (A)", 2D) = "white" {}
	_Illum("Illumin (A)", 2D) = "white" {}
	_EmissionLM("Emission (Lightmapper)", Float) = 0

		_H("H", Float) = 0.09
		_ShadowColor("Shadow Color", Color) = (0, 0, 0, 0.4)
		_ShadowDir("_ShadowDir", Vector) = (0, -0.5, 1, 1)
	}
		SubShader{



		Tags{ "RenderType" = "Opaque" }
		LOD 200

		CGPROGRAM
#pragma surface surf Lambert

		sampler2D _MainTex;
	sampler2D _Illum;
	fixed4 _Color;

	struct Input {
		float2 uv_MainTex;
		float2 uv_Illum;
	};

	void surf(Input IN, inout SurfaceOutput o)
	{
		fixed4 tex = tex2D(_MainTex, IN.uv_MainTex);
		fixed4 c = tex * _Color;
		o.Albedo = c.rgb;
		o.Emission = c.rgb * tex2D(_Illum, IN.uv_Illum).a;
		o.Alpha = c.a;
	}
	ENDCG


		Pass{
		Tags{ "Queue" = "Transparent" "IgnoreProjector" = "True" "RenderType" = "Transparent" }

		Stencil{
		Ref 123
		Comp notequal
		Pass replace
	}

		Cull Off
		Blend SrcAlpha OneMinusSrcAlpha
		Lighting Off
		ColorMask RGB
		//ZWrite Off
		Fog{ Mode Off }

		CGINCLUDE
#include "UnityCG.cginc"

		struct appdata {
		float4 vertex :
		POSITION;
	};

	struct v2f {
		float4 pos :
		POSITION;
		float a :
		TEXCOORD0;
	};

	float _H;
	float4 _ShadowDir;
	float4 _ShadowColor;

	v2f vert(appdata v)
	{
		v2f o;
		float4 v4 = mul(unity_ObjectToWorld, v.vertex);
		//v4.xyz = v4.w;
		v4.y -= _H;
		o.a = v4.y;

		float3 shadowDir = normalize(_ShadowDir.xyz);

		float3 v3 = v4.xyz + shadowDir * v4.y / dot(shadowDir, float3(0, -1, 0));
		v4.xyz = v3.xyz;
		v4.y = _H;
		o.pos = mul(UNITY_MATRIX_VP, v4);

		return o;
	}


	half4 frag(v2f i) : COLOR
	{
		return _ShadowColor * sign(i.a);
	}

		ENDCG

		CGPROGRAM
#pragma vertex vert
#pragma fragment frag
		ENDCG
	}


	}
		FallBack "Self-Illumin/VertexLit"
}
