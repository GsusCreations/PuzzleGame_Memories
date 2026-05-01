Shader "Custom/PuzzleGuide"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _OutlineColor ("Outline Color", Color) = (1, 1, 1, 0.5) // Blanco semitransparente por defecto
        _OutlineThickness ("Outline Thickness", Range(1, 10)) = 2
        _FillColor ("Inner Fill Color", Color) = (0, 0, 0, 0) // Totalmente transparente por defecto adentro
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
            float4 _MainTex_TexelSize;

            CBUFFER_START(UnityPerMaterial)
                float4 _OutlineColor;
                float _OutlineThickness;
                float4 _FillColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color; // Ignoramos el _Color general para forzar los nuestros
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv);

                float2 thickness = _MainTex_TexelSize.xy * _OutlineThickness;

                // Muestreamos los pixeles vecinos para encontrar dónde acaba el alpha (el borde)
                float alphaUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, thickness.y)).a;
                float alphaDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0, thickness.y)).a;
                float alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, 0)).a;
                float alphaLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(thickness.x, 0)).a;

                float alphaUpRight   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, thickness.y)).a;
                float alphaUpLeft    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-thickness.x, thickness.y)).a;
                float alphaDownRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, -thickness.y)).a;
                float alphaDownLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-thickness.x, -thickness.y)).a;

                // Encontramos el pixel más sólido y el más transparente en esta área
                float maxAlpha1 = max(max(alphaUp, alphaDown), max(alphaRight, alphaLeft));
                float maxAlpha2 = max(max(alphaUpRight, alphaUpLeft), max(alphaDownRight, alphaDownLeft));
                float maxAlpha = max(c.a, max(maxAlpha1, maxAlpha2));

                float minAlpha1 = min(min(alphaUp, alphaDown), min(alphaRight, alphaLeft));
                float minAlpha2 = min(min(alphaUpRight, alphaUpLeft), min(alphaDownRight, alphaDownLeft));
                float minAlpha = min(c.a, min(minAlpha1, minAlpha2));

                // La diferencia nos dice si estamos justo en un borde
                float edge = maxAlpha - minAlpha;

                half4 finalColor = half4(0,0,0,0);

                if (edge > 0.1)
                {
                    // Estamos en la línea del borde
                    finalColor = _OutlineColor;
                }
                else if (c.a >= 0.1)
                {
                    // Estamos completamente adentro del sprite
                    finalColor = _FillColor;
                }

                // Aplicar alpha premultiplicado requerido por URP 2D
                finalColor.rgb *= finalColor.a;
                
                return finalColor;
            }
            ENDHLSL
        }
    }
}
