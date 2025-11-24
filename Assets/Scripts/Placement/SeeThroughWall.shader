Shader "Custom/SeeThroughWall"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _CutoutRadius ("Cutout Radius (Screen Space)", Range(0, 1)) = 0.5
        _CutoutSoftness ("Cutout Softness", Range(0, 0.5)) = 0.1
        _CutoutCenter ("Cutout Center (Viewport)", Vector) = (0.5, 0.5, 0, 0)
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" }
        LOD 100
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 normalWS : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _CutoutRadius;
                float _CutoutSoftness;
                float4 _CutoutCenter;
            CBUFFER_END

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.uv = TRANSFORM_TEX(input.uv, _BaseMap);
                output.screenPos = ComputeScreenPos(output.positionCS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float2 screenUV = input.screenPos.xy / input.screenPos.w;
                
                // Correct for aspect ratio to ensure the cutout is circular
                float aspect = _ScreenParams.x / _ScreenParams.y;
                float2 uvCorrected = screenUV;
                uvCorrected.x *= aspect;
                
                float2 centerCorrected = _CutoutCenter.xy;
                centerCorrected.x *= aspect;

                float dist = distance(uvCorrected, centerCorrected);
                
                // Calculate alpha factor: 0 inside radius, 1 outside (with softness)
                float alphaFactor = smoothstep(_CutoutRadius, _CutoutRadius + _CutoutSoftness, dist);

                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, input.uv) * _BaseColor;
                
                // Simple Fake Lighting (to avoid dependency issues with Lighting.hlsl)
                // Fixed light direction (from top-left-back)
                float3 lightDir = normalize(float3(-0.5, 1.0, -0.5));
                float3 normalWS = normalize(input.normalWS);
                float NdotL = max(0, dot(normalWS, lightDir));
                
                // Ambient
                float3 ambient = float3(0.3, 0.3, 0.3); 
                
                float3 lighting = NdotL + ambient;
                
                half4 finalColor = half4(albedo.rgb * lighting, albedo.a * alphaFactor);

                return finalColor;
            }
            ENDHLSL
        }
    }
}
