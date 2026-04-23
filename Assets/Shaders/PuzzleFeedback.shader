Shader "Custom/PuzzleSpriteFeedback"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)
        _OutlineColor ("Outline Color", Color) = (1,1,1,1)
        _OutlineIntensity ("Outline Intensity", Range(0, 1)) = 0
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
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex   : POSITION;
                float4 color    : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex   : SV_POSITION;
                fixed4 color    : COLOR;
                float2 texcoord  : TEXCOORD0;
            };

            sampler2D _MainTex;
            fixed4 _Color;
            fixed4 _OutlineColor;
            float _OutlineIntensity;
            float4 _MainTex_TexelSize;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.texcoord = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                fixed4 c = tex2D(_MainTex, IN.texcoord) * IN.color;

                // Simple Outline logic by sampling neighbors
                if (_OutlineIntensity > 0)
                {
                    float alphaUp = tex2D(_MainTex, IN.texcoord + float2(0, _MainTex_TexelSize.y)).a;
                    float alphaDown = tex2D(_MainTex, IN.texcoord - float2(0, _MainTex_TexelSize.y)).a;
                    float alphaRight = tex2D(_MainTex, IN.texcoord + float2(_MainTex_TexelSize.x, 0)).a;
                    float alphaLeft = tex2D(_MainTex, IN.texcoord - float2(_MainTex_TexelSize.x, 0)).a;

                    float outlineAlpha = max(max(alphaUp, alphaDown), max(alphaRight, alphaLeft));
                    
                    // Solo aplicar outline si el pixel original es transparente
                    if (c.a < 0.1 && outlineAlpha > 0.1)
                    {
                        c = _OutlineColor;
                        c.a *= _OutlineIntensity;
                    }
                }
                
                c.rgb *= c.a; // Premultiplied alpha
                return c;
            }
            ENDCG
        }
    }
}