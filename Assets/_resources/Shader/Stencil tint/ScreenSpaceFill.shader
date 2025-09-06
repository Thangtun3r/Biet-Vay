Shader "BV/OverlayStripes_Outline"
{
    Properties
    {
        // overlay
        _OverlayColor ("Overlay Color", Color) = (1,1,1,1)
        _OverlayAlpha ("Overlay Alpha", Range(0,1)) = 0.6
        _Tiling       ("Stripe Tiling", Float) = 20.0
        _Offset       ("Stripe Offset", Float) = 0.0     // animate for panning

        // outline
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineWidth ("Outline Width (world)", Float) = 0.01
    }

    SubShader
    {
        // Draw after the base (opaque) material
        Tags{ "Queue" = "Transparent+10" "RenderType" = "Transparent" }
        LOD 100

        // ------------ PASS 1: OUTLINE (optional) ------------
        // Renders an expanded backface shell in a solid color.
        Pass
        {
            Name "OUTLINE"
            Cull Front
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OutlineColor;
            float _OutlineWidth;

            struct appdata {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };
            struct v2f {
                float4 pos : SV_POSITION;
            };

            v2f vert (appdata v)
            {
                // expand along normals in world space for consistent thickness
                float3 worldPos   = mul(unity_ObjectToWorld, v.vertex).xyz;
                float3 worldNorm  = UnityObjectToWorldNormal(v.normal);
                worldPos += normalize(worldNorm) * _OutlineWidth;
                v2f o;
                o.pos = UnityWorldToClipPos(float4(worldPos,1));
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                return _OutlineColor;
            }
            ENDCG
        }

        // ------------ PASS 2: OVERLAY STRIPES ------------
        // Uses screen-space UVs so the pattern is consistent across the model.
        Pass
        {
            Name "OVERLAY"
            Cull Back
            ZTest LEqual
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _OverlayColor;
            float  _OverlayAlpha;
            float  _Tiling;
            float  _Offset;

            struct appdata {
                float4 vertex : POSITION;
            };
            struct v2f {
                float4 pos        : SV_POSITION;
                float4 screenPos  : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.screenPos = ComputeScreenPos(o.pos);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // screen-space coordinates [0..1]
                float2 uv = i.screenPos.xy / i.screenPos.w;

                // diagonal stripes: use uv.x + uv.y
                float s = frac((uv.x + uv.y) * _Tiling + _Offset);

                // make hard bands: 0 or 1 (adjust 0.5 to change width)
                float mask = step(0.5, s);

                // overlay color with alpha only where stripes are
                float a = mask * _OverlayAlpha;
                return float4(_OverlayColor.rgb, a);
            }
            ENDCG
        }
    }
}
