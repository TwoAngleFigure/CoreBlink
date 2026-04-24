

Shader "Hidden/Pixelize"
{
    SubShader
    {
        Tags
        {
            "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"
        }

        ZWrite Off
        Cull Off

        HLSLINCLUDE
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

        SamplerState sampler_point_clamp;

        uniform float2 _BlockCount;
        uniform float2 _BlockSize;
        uniform float2 _HalfBlockSize;
        ENDHLSL

        Pass
        {
            Name "Pixelation"

            HLSLPROGRAM
            #pragma vertex Vert
            #pragma fragment frag

            half4 frag(Varyings IN) : SV_TARGET
            {
                float2 blockPos = floor(IN.texcoord * _BlockCount);
                float2 blockCenter = blockPos * _BlockSize + _HalfBlockSize;

                float4 tex = SAMPLE_TEXTURE2D(_BlitTexture, sampler_point_clamp, blockCenter);

                return tex;
            }
            ENDHLSL
        }
    }
}