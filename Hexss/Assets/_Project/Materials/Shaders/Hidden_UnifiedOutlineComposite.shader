Shader "Hidden/UnifiedOutlineComposite"
{
    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
        }

        Pass
        {
            Name "Unified Outline Composite"

            ZWrite Off
            ZTest Always
            Cull Off

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            TEXTURE2D(_SourceTex);
            SAMPLER(sampler_SourceTex);

            TEXTURE2D(_MaskTex);
            SAMPLER(sampler_MaskTex);

            float4 _OutlineColor;
            int _Thickness;
            float4 _MaskTex_TexelSize_Custom;

            struct Attributes
            {
                uint vertexID : SV_VertexID;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings Vert(Attributes input)
            {
                Varyings output;
                output.positionCS = GetFullScreenTriangleVertexPosition(input.vertexID);
                output.uv = GetFullScreenTriangleTexCoord(input.vertexID);
                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                float2 uv = input.uv;

                half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_SourceTex, uv);

                float center = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, uv).r;

                // Do not draw outline inside the selected objects.
                if (center > 0.5)
                    return sceneColor;

                float2 texel = _MaskTex_TexelSize_Custom.xy;

                float foundMaskNearby = 0;

                // Max loop size must be constant for shader compilation.
                // Runtime thickness is clamped by the feature inspector.
                [loop]
                for (int x = -12; x <= 12; x++)
                {
                    [loop]
                    for (int y = -12; y <= 12; y++)
                    {
                        if (x == 0 && y == 0)
                            continue;

                        float dist = length(float2(x, y));

                        if (dist > _Thickness)
                            continue;

                        float2 offsetUv = uv + float2(x, y) * texel;
                        float sampleMask = SAMPLE_TEXTURE2D(_MaskTex, sampler_MaskTex, offsetUv).r;

                        foundMaskNearby = max(foundMaskNearby, sampleMask);
                    }
                }

                if (foundMaskNearby > 0.5)
                {
                    return lerp(sceneColor, _OutlineColor, _OutlineColor.a);
                }

                return sceneColor;
            }

            ENDHLSL
        }
    }
}