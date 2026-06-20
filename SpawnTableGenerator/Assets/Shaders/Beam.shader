Shader "SpawnSystem/Beam"
{
    Properties
    {
        [HDR] _Color ("Edge Color (HDR)", Color) = (0, 1, 1, 1)
        [HDR] _CoreColor ("Core Color (HDR)", Color) = (1, 1, 1, 1)
        _Intensity ("Intensity", Float) = 4
        _FresnelPower ("Fresnel Power", Float) = 2
    }
    SubShader
    {
        Tags { "RenderType" = "Transparent" "Queue" = "Transparent" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            Blend One One        // additive — 발광/글로우
            ZWrite Off
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            float4 _Color;
            float4 _CoreColor;
            float _Intensity;
            float _FresnelPower;

            Varyings vert(Attributes IN)
            {
                Varyings o;
                VertexPositionInputs pos = GetVertexPositionInputs(IN.positionOS.xyz);
                o.positionHCS = pos.positionCS;
                o.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                o.viewDirWS = GetWorldSpaceViewDir(pos.positionWS);
                return o;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 N = normalize(IN.normalWS);
                float3 V = normalize(IN.viewDirWS);
                // 정면일수록 코어(흰빛), 가장자리일수록 엣지 색 → 광선검 코어+글로우 느낌.
                float fres = pow(1.0 - saturate(dot(N, V)), _FresnelPower);
                float3 col = lerp(_CoreColor.rgb, _Color.rgb, fres) * _Intensity;
                return half4(col, 1);
            }
            ENDHLSL
        }
    }
    Fallback Off
}
