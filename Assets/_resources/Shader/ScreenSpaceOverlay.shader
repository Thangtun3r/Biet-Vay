Shader "Custom/ScreenSpaceOverlayObject"
{
    Properties
    {
        _OverlayTex ("Overlay Texture", 2D) = "white" {}
        _TintColor ("Tint Color", Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            sampler2D _OverlayTex;
            float4 _TintColor;

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                float2 screenUV : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                float4 worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.pos = UnityObjectToClipPos(v.vertex);

                // Calculate screen-space UV
                float4 screenPos = ComputeScreenPos(o.pos);
                o.screenUV = screenPos.xy / screenPos.w;

                o.worldNormal = UnityObjectToWorldNormal(v.normal);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float2 uv = i.screenUV;
                uv.y = 1.0 - uv.y;

                fixed4 overlay = tex2D(_OverlayTex, uv) * _TintColor;

                // Lighting
                float3 normal = normalize(i.worldNormal);
                float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);
                float NdotL = max(0, dot(normal, lightDir));

                fixed3 lightColor = _LightColor0.rgb;
                fixed3 litColor = overlay.rgb * lightColor * NdotL;

                return fixed4(litColor, overlay.a);
            }
            ENDCG
        }
    }
}
