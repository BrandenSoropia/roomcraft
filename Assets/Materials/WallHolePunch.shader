Shader "Custom/WallHolePunch"
{
    Properties
    {
        _BaseMap ("Base Map", 2D) = "white" {}
        _BaseColor ("Base Color", Color) = (1,1,1,1)
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        
        [Header(Hole Punch Settings)]
        _HoleRadius ("Hole Radius", Range(0.1, 10)) = 2.0
        _HoleFalloff ("Hole Falloff", Range(0.1, 5)) = 1.0
        _MaxHoles ("Max Holes", Range(1, 10)) = 5
    }

    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent"
            "RenderPipeline"="UniversalRenderPipeline"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float fogCoord : TEXCOORD3;
            };

            TEXTURE2D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float _Metallic;
                float _Smoothness;
                float _HoleRadius;
                float _HoleFalloff;
                float _MaxHoles;
            CBUFFER_END

            // Hole positions - using individual properties (max 5 holes for now)
            // Set via Material.SetVector in script
            float4 _HolePos0;
            float4 _HolePos1;
            float4 _HolePos2;
            float4 _HolePos3;
            float4 _HolePos4;
            float _HoleCount;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs vertexInput = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInput = GetVertexNormalInputs(IN.normalOS);
                
                OUT.positionHCS = vertexInput.positionCS;
                OUT.positionWS = vertexInput.positionWS;
                OUT.normalWS = normalInput.normalWS;
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.fogCoord = ComputeFogFactor(vertexInput.positionCS.z);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Sample base texture (will return white if texture is null)
                half4 albedo = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv) * _BaseColor;
                
                // Calculate lighting
                Light mainLight = GetMainLight();
                float3 normalWS = normalize(IN.normalWS);
                float NdotL = saturate(dot(normalWS, mainLight.direction));
                float3 lighting = mainLight.color * NdotL + SampleSH(normalWS);
                
                half3 color = albedo.rgb * lighting;
                
                // Calculate hole punch alpha
                float alpha = albedo.a;
                float minDist = 999999.0;
                
                // Find minimum distance to any hole position
                int holeCount = (int)min(_HoleCount, _MaxHoles);
                
                // Check each hole position
                if (holeCount > 0)
                {
                    float dist0 = distance(IN.positionWS, _HolePos0.xyz);
                    minDist = min(minDist, dist0);
                }
                if (holeCount > 1)
                {
                    float dist1 = distance(IN.positionWS, _HolePos1.xyz);
                    minDist = min(minDist, dist1);
                }
                if (holeCount > 2)
                {
                    float dist2 = distance(IN.positionWS, _HolePos2.xyz);
                    minDist = min(minDist, dist2);
                }
                if (holeCount > 3)
                {
                    float dist3 = distance(IN.positionWS, _HolePos3.xyz);
                    minDist = min(minDist, dist3);
                }
                if (holeCount > 4)
                {
                    float dist4 = distance(IN.positionWS, _HolePos4.xyz);
                    minDist = min(minDist, dist4);
                }
                
                // Calculate hole alpha based on distance
                if (minDist < _HoleRadius)
                {
                    // Smooth falloff from edge to center
                    float normalizedDist = minDist / _HoleRadius;
                    float holeAlpha = smoothstep(0.0, _HoleFalloff / _HoleRadius, normalizedDist);
                    alpha *= holeAlpha;
                }
                
                // Apply fog
                color = MixFog(color, IN.fogCoord);
                
                return half4(color, alpha);
            }
            ENDHLSL
        }

    }

    FallBack "Hidden/Universal Render Pipeline/FallbackError"
}

