Shader "Inverted Flip Normal" {
	Properties{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

		SubShader{
			Tags { "RenderType" = "Opaque" }
			Cull front    //TO FLIP THE SURFACES
			LOD 100

			Pass {
				CGPROGRAM
					#pragma vertex vert
					#pragma fragment frag

					#include "UnityCG.cginc"

					struct appdata_t {
						float4 vertex : POSITION;
						float2 texcoord : TEXCOORD0;
					};

					struct v2f {
						float4 vertex : SV_POSITION;
						half2 texcoord : TEXCOORD0;
					};

					//
					sampler2D _MainTex;
					float4 _MainTex_ST;

					v2f vert(appdata_t v) {
						v2f o;
						o.vertex = UnityObjectToClipPos(v.vertex);						
						v.texcoord.x = 1 - v.texcoord.x;
						o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
						return o;
					}

					fixed4 frag(v2f i) : SV_Target {
						fixed4 col = tex2D(_MainTex, i.texcoord);
						return col;
					}

				ENDCG
				}
	}
}


//
//Shader "Custom/Flip Normals" {
//	Properties{
//		_MainTex("Base (RGB)", 2D) = "white" {}
//	}
//		SubShader{
//
//			Tags { "RenderType" = "Opaque" }
//
//			Cull Off
//
//			CGPROGRAM
//
//			#pragma surface surf Lambert vertex:vert
//			sampler2D _MainTex;
//
//			struct Input {
//				float2 uv_MainTex;
//				float4 color : COLOR;
//			};
//
//			void vert(inout appdata_full v) {
//				v.normal.xyz = v.normal * -1;
//
//
//			}
//
//			void surf(Input IN, inout SurfaceOutput o) {
//				 fixed3 result = tex2D(_MainTex, IN.uv_MainTex);
//				 o.Albedo = result.rgb;
//				 o.Alpha = 1;
//			}
//
//			ENDCG
//
//	}
//
//		Fallback "Diffuse"
//}

