Shader "Custom/VolumeShader"
{
    Properties
    {
        [MainTexture] _BaseMap("Base Map", 3D) = "white" {}
        _Alpha ("Alpha", Range(0, 1)) = 1.0
        _AlphaThreshold ("Alpha Threshold", Range(0, 1)) = 0.1
        _StepSize ("Step Size", Range(0.005, 0.01)) = 0.01
    }

    SubShader
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "RenderPipeline" = "UniversalPipeline" }
        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

        Pass
        {
            HLSLPROGRAM
            
            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionOS : TEXCOORD0;
            };

            TEXTURE3D(_BaseMap);
            SAMPLER(sampler_BaseMap);

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float _Alpha;
                float _AlphaThreshold;
                float _StepSize;
            CBUFFER_END

            Varyings vert(Attributes input)
            {
                Varyings output;
                output.positionHCS = TransformObjectToHClip(input.positionOS.xyz);
                output.positionOS = input.positionOS.xyz;
                return output;
            }
            
            bool IntersectAABB(float3 rayOrigin, float3 rayDirection, out float tNear, out float tFar)
            {
                float3 invR = rcp(rayDirection);
                float3 t1 = (-0.5 - rayOrigin) * invR;
                float3 t2 = (0.5 - rayOrigin) * invR;
                
                float3 tmin = min(t1, t2);
                float3 tmax = max(t1, t2);
                
                tNear = max(max(tmin.x, tmin.y), tmin.z);
                tFar  = min(min(tmax.x, tmax.y), tmax.z);

                return tNear <= tFar;
            }

            half4 frag(Varyings input) : SV_Target
            {
                float3 camPosOS = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos, 1.0)).xyz;
                float3 rayDirOS = normalize(input.positionOS - camPosOS);
                float tNear, tFar;
                if (!IntersectAABB(camPosOS, rayDirOS, tNear, tFar)) discard;

                float4 accumulatedColor = 0;
                float t = max(0, tNear);
                const int maxSteps = 128;

                for (int i = 1; i < maxSteps; i++)
                {
                    float3 uv = camPosOS + t * rayDirOS + 0.5;
                    float4 sample = SAMPLE_TEXTURE3D(_BaseMap, sampler_BaseMap, uv);

                    float mask = step(_AlphaThreshold, sample.a);
                    sample.a *= mask;
                    sample.rgb = sample.a;
                    sample.a *= _Alpha;

                    accumulatedColor.rgb += (1.0 - accumulatedColor.a) * sample.rgb * sample.a;
                    accumulatedColor.a += (1.0 - accumulatedColor.a) * sample.a;
                    
                    t += _StepSize;
                    if (t > tFar) break;
                }
                
                return accumulatedColor;
            }
            ENDHLSL
        }
    }
}