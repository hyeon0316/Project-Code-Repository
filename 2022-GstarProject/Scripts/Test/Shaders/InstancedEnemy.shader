// DrawMeshInstancedProcedural 전용. 인스턴스 행렬은 _Instances(StructuredBuffer)에서 읽음 (애니메이션 없음)
Shader "Test/InstancedEnemy"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Color ("Color", Color) = (1, 1, 1, 1)
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

            #ifdef UNITY_PROCEDURAL_INSTANCING_ENABLED
            // C# EnemyInstanceData와 같은 레이아웃 (80B)
            struct InstanceData
            {
                float4x4 objectToWorld;
                float4 anim;
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

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
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

                v2f o;
                float3 posWS = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
                o.pos = mul(UNITY_MATRIX_VP, float4(posWS, 1));
                // 균등 스케일만 쓰므로 역전치 대신 ObjectToWorld로 노멀 변환
                o.normalWS = normalize(mul((float3x3)unity_ObjectToWorld, v.normal));
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
