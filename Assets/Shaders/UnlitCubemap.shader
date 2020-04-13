Shader "Adrenak/UniMap/Unlit Cubemap"{
	Properties {
		_Cubemap("Cubemap", CUBE) = "" {}
	}

	SubShader {
		Tags {
			"RenderType" = "Opaque"
		}
		Cull Front
		CGPROGRAM

		#pragma surface surf Lambert

		struct Input {
			float3 worldRefl;
		};

		samplerCUBE _Cubemap;

		void surf(Input IN, inout SurfaceOutput o) {
			o.Emission = texCUBE(_Cubemap, IN.worldRefl).rgb;
		}
		ENDCG
	}
	Fallback "Diffuse"
}
