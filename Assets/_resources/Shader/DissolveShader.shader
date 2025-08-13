Shader "Custom/URP_Dissolve_Unlit"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Tint", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags {
            "RenderPipeline"="UniversalRenderPipeline"
            "Queue"="Transparent"
            "RenderType"="Transparent"
        }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            // URP headers
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // material inputs
            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);
            float4 _BaseColor;

            // global mask (set by SphereMaskController)
            float3 _GLOBALMaskPosition;
            float  _GLOBALMaskRadius;
            float  _GLOBALMaskSoftness;

            struct Attributes {
                float4 positionOS : POSITION;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
            };

            Varyings vert (Attributes v)
            {
                Varyings o;
                o.positionWS  = TransformObjectToWorld(v.positionOS.xyz);
                o.positionHCS = TransformWorldToHClip(o.positionWS);
                o.uv          = v.uv;
                return o;
            }

            half4 frag (Varyings i) : SV_Target
            {
                half4 col = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, i.uv) * _BaseColor;

                // sphere dissolve
                float d = distance(i.positionWS, _GLOBALMaskPosition);

                // alpha goes to 0 outside radius, soft edge using smoothstep
                float a = smoothstep(_GLOBALMaskRadius, _GLOBALMaskRadius - _GLOBALMaskSoftness, d);

                // kill pixels completely outside (prevents sorting halos)
                clip(a - 0.001);

                col.a *= a;
                return col;
            }
            ENDHLSL
        }
    }
}
