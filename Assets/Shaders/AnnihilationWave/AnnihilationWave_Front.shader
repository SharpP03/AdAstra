Shader "Custom/AnnihilationWave_Front"
{
    Properties
    {
        [Header(Colors)]
        [HDR] _DeepColor ("Deep Void Color", Color) = (0.08, 0.005, 0.02, 0.2)
        [HDR] _CoreColor ("Core Plasma Color", Color) = (1.8, 0.25, 0.05, 0.8)
        [HDR] _FilamentColor ("Electric Filament Web", Color) = (3.5, 1.2, 0.2, 1.0)
        [HDR] _ContactGlowColor ("Contact Intersection Glow", Color) = (4.5, 1.8, 0.4, 1.0)

        [Header(Noise and Fluid Turbulence)]
        _NoiseScale ("Noise Frequency Scale", Float) = 2.5
        _DistortionStrength ("Turbulence Distortion", Range(0.0, 2.0)) = 0.65
        _Speed1 ("Primary Drift Speed (XY)", Vector) = (0.006, 0.012, 0, 0)
        _Speed2 ("Secondary Drift Speed (XY)", Vector) = (-0.009, 0.007, 0, 0)
        _VoronoiScale ("Filament Web Scale", Float) = 5.5
        _VoronoiPower ("Filament Sharpness", Range(1.0, 8.0)) = 3.5
        _PulseSpeed ("Global Pulse Frequency", Float) = 0.6
        _PulseIntensity ("Global Pulse Intensity", Range(0.0, 1.0)) = 0.2

        [Header(Soft Intersections and Organic Edges)]
        _DepthFadeDistance ("Soft Intersection Distance", Float) = 8.0
        _ContactGlowPower ("Contact Glow Sharpness", Range(1.0, 10.0)) = 3.0
        _ContactGlowIntensity ("Contact Glow Intensity", Float) = 3.0
        _EdgeSoftness ("Edge Feathering Margin", Range(0.1, 0.9)) = 0.45
        _EdgeNoiseDistortion ("Edge Raggedness / Erosion", Range(0.0, 1.0)) = 0.45

        [Header(Vertex Undulation)]
        _WaveAmplitude ("3D Mesh Undulation Amplitude", Float) = 2.5
        _WaveFrequency ("3D Mesh Undulation Speed", Float) = 0.15

        [Header(Blending)]
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend ("Src Blend", Float) = 5 // SrcAlpha
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend ("Dst Blend", Float) = 1 // One (Additive)
        [Enum(UnityEngine.Rendering.CullMode)] _Cull ("Cull Mode", Float) = 0 // Off
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Transparent"
            "Queue" = "Transparent+100"
            "RenderPipeline" = "UniversalPipeline"
            "IgnoreProjector" = "True"
        }

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            Blend [_SrcBlend] [_DstBlend]
            ZWrite Off
            Cull [_Cull]

            HLSLPROGRAM
            #pragma target 3.5
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/DeclareDepthTexture.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS       : SV_POSITION;
                float2 uv               : TEXCOORD0;
                float4 screenPosition   : TEXCOORD1;
                float3 worldNormal      : NORMAL;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _DeepColor;
                float4 _CoreColor;
                float4 _FilamentColor;
                float4 _ContactGlowColor;
                float2 _Speed1;
                float2 _Speed2;
                float _NoiseScale;
                float _DistortionStrength;
                float _VoronoiScale;
                float _VoronoiPower;
                float _PulseSpeed;
                float _PulseIntensity;
                float _DepthFadeDistance;
                float _ContactGlowPower;
                float _ContactGlowIntensity;
                float _EdgeSoftness;
                float _EdgeNoiseDistortion;
                float _WaveAmplitude;
                float _WaveFrequency;
            CBUFFER_END

            // Pseudo-random 2D hash
            float2 hash2(float2 p)
            {
                p = float2(dot(p, float2(127.1, 311.7)),
                           dot(p, float2(269.5, 183.3)));
                return frac(sin(p) * 43758.5453123);
            }

            // 2D Gradient Noise
            float Noise2D(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float2 u = f * f * (3.0 - 2.0 * f);

                float a = frac(sin(dot(i + float2(0.0, 0.0), float2(12.9898, 78.233))) * 43758.5453);
                float b = frac(sin(dot(i + float2(1.0, 0.0), float2(12.9898, 78.233))) * 43758.5453);
                float c = frac(sin(dot(i + float2(0.0, 1.0), float2(12.9898, 78.233))) * 43758.5453);
                float d = frac(sin(dot(i + float2(1.0, 1.0), float2(12.9898, 78.233))) * 43758.5453);

                return lerp(lerp(a, b, u.x), lerp(c, d, u.x), u.y);
            }

            // Fractal Brownian Motion (FBM)
            float FBM(float2 uv)
            {
                float val = 0.0;
                float amp = 0.52;
                for (int i = 0; i < 3; i++)
                {
                    val += amp * Noise2D(uv);
                    uv *= 2.12;
                    amp *= 0.48;
                }
                return val;
            }

            // Voronoi F2 - F1 edge detection for cosmic electric filament web
            float VoronoiEdges(float2 uv)
            {
                float2 i = floor(uv);
                float2 f = frac(uv);
                float d1 = 8.0;
                float d2 = 8.0;

                for (int y = -1; y <= 1; y++)
                {
                    for (int x = -1; x <= 1; x++)
                    {
                        float2 neighbor = float2(x, y);
                        float2 p = hash2(i + neighbor);
                        p = 0.5 + 0.5 * sin(_Time.y * 0.35 + 6.2831 * p);
                        float2 diff = neighbor + p - f;
                        float d = length(diff);
                        if (d < d1)
                        {
                            d2 = d1;
                            d1 = d;
                        }
                        else if (d < d2)
                        {
                            d2 = d;
                        }
                    }
                }
                return d2 - d1;
            }

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                // Multi-frequency organic vertex undulation
                float wave1 = sin(_Time.y * _WaveFrequency + input.positionOS.x * 0.06)
                            * cos(_Time.y * (_WaveFrequency * 0.75) + input.positionOS.y * 0.06);
                float wave2 = sin(_Time.y * (_WaveFrequency * 1.4) + (input.positionOS.x + input.positionOS.y) * 0.08) * 0.5;
                float totalWave = (wave1 + wave2) * _WaveAmplitude;

                float3 displacedOS = input.positionOS.xyz + input.normalOS * totalWave;

                VertexPositionInputs vertexInputs = GetVertexPositionInputs(displacedOS);
                output.positionCS = vertexInputs.positionCS;
                output.screenPosition = ComputeScreenPos(vertexInputs.positionCS);
                output.worldNormal = TransformObjectToWorldNormal(input.normalOS);
                output.uv = input.uv;

                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                // Screen UV for depth texture sampling
                float2 screenUV = input.screenPosition.xy / input.screenPosition.w;
                float rawDepth = SampleSceneDepth(screenUV);
                float sceneEyeDepth = LinearEyeDepth(rawDepth, _ZBufferParams);
                float surfaceEyeDepth = input.screenPosition.w;
                float depthDiff = sceneEyeDepth - surfaceEyeDepth;

                // Depth Fade: softens intersection with scene objects (asteroids, ship)
                float depthFade = saturate(depthDiff / max(_DepthFadeDistance, 0.01));

                // Contact Glow: blazing fiery silhouette at intersection lines
                float contactGlow = pow(saturate(1.0 - depthFade), _ContactGlowPower) * _ContactGlowIntensity;

                // Dual counter-flowing coordinate layers for organic fluid turbulence
                float2 uv1 = input.uv + _Speed1 * _Time.y;
                float2 uv2 = input.uv + _Speed2 * _Time.y;

                // Domain warping: layer 1 warps layer 2
                float2 warp = float2(
                    FBM(uv1 * _NoiseScale),
                    FBM(uv2 * _NoiseScale + float2(3.7, 1.9))
                );

                float plasmaTurbulence = FBM(uv1 * _NoiseScale + warp * _DistortionStrength);
                float contrastPlasma = smoothstep(0.28, 0.75, plasmaTurbulence);

                // High-energy electrical filament web (F2 - F1 cell boundary detection)
                float edgeDist = VoronoiEdges(uv2 * _VoronoiScale + warp * 0.4);
                float filamentEnergy = pow(saturate(1.0 - edgeDist * _VoronoiPower), 2.5);

                // Organic ragged perimeter erosion: eliminates square quad boundaries
                float2 centeredUV = input.uv * 2.0 - 1.0;
                float radialDist = length(centeredUV);
                float raggedDist = radialDist + (plasmaTurbulence - 0.5) * _EdgeNoiseDistortion;
                float edgeFalloff = smoothstep(1.0, 1.0 - _EdgeSoftness, raggedDist);

                // Global rhythmic pulse
                float pulse = 1.0 + sin(_Time.y * _PulseSpeed) * _PulseIntensity;

                // Layered HDR color blending
                half3 rgbPlasma = lerp(_DeepColor.rgb, _CoreColor.rgb, contrastPlasma) * pulse;
                half3 rgbFilaments = _FilamentColor.rgb * filamentEnergy;
                half3 rgbContact = _ContactGlowColor.rgb * contactGlow;

                half3 finalRGB = rgbPlasma + rgbFilaments + rgbContact;

                // Alpha blending: density based on turbulence, glowing filaments and soft edges
                float baseAlpha = lerp(_DeepColor.a, _CoreColor.a, contrastPlasma);
                half finalAlpha = saturate((baseAlpha + filamentEnergy * 0.7 + contactGlow) * edgeFalloff * depthFade);

                return half4(finalRGB, finalAlpha);
            }
            ENDHLSL
        }
    }
    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}
