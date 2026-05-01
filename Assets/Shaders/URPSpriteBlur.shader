Shader "Custom/URPSpriteBlur"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _BlurAmount ("Blur Amount", Range(0, 10)) = 0
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

        Cull Off
        Lighting Off
        ZWrite Off
        Blend One OneMinusSrcAlpha

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float4 color        : COLOR;
                float2 uv           : TEXCOORD0;
            };

            TEXTURE2D(_MainTex);
            SAMPLER(sampler_MainTex);
            float4 _MainTex_TexelSize; // Unity lo llena automáticamente con el tamaño del pixel

            CBUFFER_START(UnityPerMaterial)
                float4 _Color;
                float _BlurAmount;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            // Un desenfoque gaussiano simple de 9 muestras (muy ligero para WebGL)
            half4 frag(Varyings IN) : SV_Target
            {
                half4 sum = half4(0,0,0,0);
                float x = _MainTex_TexelSize.x * _BlurAmount;
                float y = _MainTex_TexelSize.y * _BlurAmount;

                // Centro
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * 0.25;

                // Esquinas
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-x, -y)) * 0.0625;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(x, -y)) * 0.0625;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-x, y)) * 0.0625;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(x, y)) * 0.0625;

                // Lados
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-x, 0)) * 0.125;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(x, 0)) * 0.125;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, -y)) * 0.125;
                sum += SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, y)) * 0.125;

                sum *= IN.color;
                sum.rgb *= sum.a; // Premultiplied Alpha
                
                return sum;
            }
            ENDHLSL
        }
    }
}