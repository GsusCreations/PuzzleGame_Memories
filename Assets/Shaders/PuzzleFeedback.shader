Shader "Custom/PuzzleSpriteFeedback"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineIntensity ("Outline Intensity", Range(0, 1)) = 0
        _OutlineThickness ("Outline Thickness", Range(1, 100)) = 1 // <-- NUEVA PROPIEDAD
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
                float4 _Color;
                float4 _OutlineColor;
                float _OutlineIntensity;
                float _OutlineThickness; // <-- NUEVA VARIABLE
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.uv = IN.uv;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                half4 c = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv) * IN.color;

                if (_OutlineIntensity > 0)
                {
                    // Multiplicamos el tamaño del pixel por el grosor deseado
                    float2 thickness = _MainTex_TexelSize.xy * _OutlineThickness;

                    // Muestreamos en las 4 direcciones usando el nuevo grosor
                    float alphaUp    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(0, thickness.y)).a;
                    float alphaDown  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(0, thickness.y)).a;
                    float alphaRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, 0)).a;
                    float alphaLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv - float2(thickness.x, 0)).a;
                    
                    // También muestreamos las diagonales para que el borde no se vea cortado en las esquinas al hacerlo más grueso
                    float alphaUpRight   = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, thickness.y)).a;
                    float alphaUpLeft    = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-thickness.x, thickness.y)).a;
                    float alphaDownRight = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(thickness.x, -thickness.y)).a;
                    float alphaDownLeft  = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, IN.uv + float2(-thickness.x, -thickness.y)).a;

                    float outlineAlpha = max(max(max(alphaUp, alphaDown), max(alphaRight, alphaLeft)),
                                             max(max(alphaUpRight, alphaUpLeft), max(alphaDownRight, alphaDownLeft)));
                    
                    if (c.a < 0.1 && outlineAlpha > 0.1)
                    {
                        c = _OutlineColor;
                        c.a *= _OutlineIntensity;
                    }
                }
                
                c.rgb *= c.a; 
                return c;
            }
            ENDHLSL
        }
    }
}