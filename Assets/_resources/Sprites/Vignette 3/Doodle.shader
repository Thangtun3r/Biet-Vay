Shader "Sprites/DoodleWobble"
{
    Properties
    {
        [PerRendererData]_MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        // Doodle controls
        _NoiseScale ("Displacement (local units)", Range(0,0.2)) = 0.02
        _NoiseFPS   ("Jitter FPS", Range(1,30)) = 6
        _Seed       ("Seed", Float) = 0.0

        // Usual sprite/UI options
        [MaterialToggle] PixelSnap ("Pixel snap", Float) = 0
        [HideInInspector]_RendererColor ("RendererColor", Color) = (1,1,1,1)
        [HideInInspector]_Flip ("Flip", Vector) = (1,1,1,1)
        [PerRendererData]_AlphaTex ("External Alpha", 2D) = "white" {}
        [PerRendererData]_EnableExternalAlpha ("Enable External Alpha", Float) = 0
        // UI masking
        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
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
        Blend One OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile _ PIXELSNAP_ON
            #pragma multi_compile _ ETC1_EXTERNAL_ALPHA

            #include "UnityCG.cginc"
            #include "UnityUI.cginc"       // for UI masking support
            #include "UnitySprites.cginc"  // sprite helpers

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            fixed4 _Color;
            fixed4 _RendererColor;
            float4 _Flip;

            float _NoiseScale;
            float _NoiseFPS;
            float _Seed;

            // ===== Hash / pseudo-random helpers (deterministic, fast) =====
            // 3D hash returning float3 in [0,1)
            float3 hash33(float3 p)
            {
                // Nikita Miropolskiy–style hash
                const float3 dotDir = float3(127.1, 311.7, 74.7);
                const float3 dotDir2 = float3(269.5, 183.3, 246.1);
                float n = sin(dot(p, dotDir)) * 43758.5453;
                float3 q = frac(float3(n, n*1.2154, n*1.3453));
                q += sin(dot(p, dotDir2)) * 0.0001; // tiny decorrelation
                return frac(q * 1.1213);
            }

            // Snap a scalar time value to 1/FPS steps (e.g., 6 fps => 0.1667 s steps)
            float snapTime(float t, float fps)
            {
                fps = max(fps, 1.0);
                float step = 1.0 / fps;
                return round(t / step) * step;
            }

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
                fixed4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
                #ifdef UNITY_UI_CLIP_RECT
                float4 worldPosition : TEXCOORD1;
                #endif
                UNITY_VERTEX_OUTPUT_STEREO
            };

            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                // Respect sprite flipping the same way as Unity Sprites do
                v.vertex = UnityFlipSprite(v.vertex, _Flip);

                // Time snapping for that hand-drawn feel
                float t = snapTime(_Time.y, _NoiseFPS);

                // Build a per-vertex seed: object-space vertex + time + user seed
                float3 seed = float3(v.vertex.xy, 0.0) + float3(t + _Seed, t*1.3 + _Seed*2.1, t*2.0 + _Seed*3.7);

                // Random displacement in [-0.5, 0.5]
                float2 r = hash33(seed).xy - 0.5;

                // Apply displacement in local units
                v.vertex.xy += r * _NoiseScale;

                #if defined(PIXELSNAP_ON)
                    v.vertex = UnityPixelSnap(v.vertex);
                #endif

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = v.texcoord;
                o.color = v.color * _Color * _RendererColor;

                #ifdef UNITY_UI_CLIP_RECT
                o.worldPosition = v.vertex;
                #endif

                return o;
            }

            fixed4 SampleSpriteTexture (float2 uv)
            {
                fixed4 c = tex2D(_MainTex, uv);
                #if ETC1_EXTERNAL_ALPHA
                    fixed4 alpha = tex2D (_AlphaTex, uv);
                    c.a = alpha.r;
                #endif
                return c;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 c = SampleSpriteTexture(i.texcoord) * i.color;
                clip(c.a - 0.001);
                return c;
            }
            ENDCG
        }
    }

    FallBack "Sprites/Default"
}
