// DrawMeshInstancedProcedural + VAT. 인스턴스마다 _Instances.anim 프레임의 정점 위치/노멀을 텍스처에서 읽음
Shader "Test/InstancedEnemyVat"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
        [NoScaleOffset] _VatPositionTex ("VAT Position", 2D) = "black" {}
        [NoScaleOffset] _VatNormalTex ("VAT Normal", 2D) = "black" {}
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #pragma instancing_options procedural:setup
            #pragma target 4.5

            #include "UnityCG.cginc"
            #include "UnityLightingCommon.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            fixed4 _Color;
            sampler2D _VatPositionTex;
            sampler2D _VatNormalTex;
            float4 _VatInfo; // x: 텍스처 한 행의 정점 수, y: 프레임당 행 수, z: 1/텍스처 폭, w: 1/텍스처 높이

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // C# EnemyInstanceData와 같은 레이아웃 (80B)
            struct InstanceData
            {
                float4x4 objectToWorld;
                float4 anim; // x: 시작 프레임, y: 프레임 수, z: 현재 프레임(클립 기준), w: 루프 여부
            };
            StructuredBuffer<InstanceData> _Instances;
            #endif

            // UNITY_SETUP_INSTANCE_ID에서 인스턴스마다 호출
            void setup()
            {
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                unity_ObjectToWorld = _Instances[unity_InstanceID].objectToWorld;
            #endif
            }

            // 조명 계산은 여기만 사용 (URP 이식 시 이 함수와 include만 교체)
            half3 SimpleLighting(float3 normalWS, half3 albedo)
            {
                half ndl = saturate(dot(normalWS, _WorldSpaceLightPos0.xyz));
                half3 ambient = ShadeSH9(float4(normalWS, 1));
                return albedo * (_LightColor0.rgb * ndl + ambient);
            }

            // 정점 vid의 절대 프레임 frame 텍셀 중심 UV
            float4 VatUV(uint vid, float frame)
            {
                uint width = (uint)_VatInfo.x;
                float x = vid % width;
                float y = frame * _VatInfo.y + vid / width;
                return float4((x + 0.5) * _VatInfo.z, (y + 0.5) * _VatInfo.w, 0, 0);
            }

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
                uint vid : SV_VertexID;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
            };

            v2f vert(appdata v)
            {
                UNITY_SETUP_INSTANCE_ID(v);

                float3 posOS = v.vertex.xyz;
                float3 normalOS = v.normal;
            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
                float4 anim = _Instances[unity_InstanceID].anim;
                float f0 = floor(anim.z);
                float f1 = f0 + 1;
                if (f1 >= anim.y)
                {
                    f1 = anim.w > 0.5 ? 0 : anim.y - 1; // 루프면 첫 프레임으로, 아니면 마지막 프레임 유지
                }
                float t = anim.z - f0;

                float4 uv0 = VatUV(v.vid, anim.x + f0);
                float4 uv1 = VatUV(v.vid, anim.x + f1);
                posOS = lerp(tex2Dlod(_VatPositionTex, uv0).xyz, tex2Dlod(_VatPositionTex, uv1).xyz, t);
                normalOS = lerp(tex2Dlod(_VatNormalTex, uv0).xyz, tex2Dlod(_VatNormalTex, uv1).xyz, t);
            #endif

                v2f o;
                float3 posWS = mul(unity_ObjectToWorld, float4(posOS, 1)).xyz;
                o.pos = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                // 균등 스케일만 쓰므로 역전치 대신 ObjectToWorld로 노멀 변환
                o.normalWS = normalize(mul((float3x3)unity_ObjectToWorld, normalOS));
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                half3 albedo = tex2D(_MainTex, i.uv).rgb * _Color.rgb;
                return fixed4(SimpleLighting(normalize(i.normalWS), albedo), 1);
            }
            ENDCG
        }
    }
}
