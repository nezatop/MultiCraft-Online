Shader "Custom/DestroyBlock"
{
    Properties
    {
        _CrackTex0 ("Crack Texture 0", 2D) = "" {}
        _CrackTex1 ("Crack Texture 1", 2D) = "" {}
        _CrackTex2 ("Crack Texture 2", 2D) = "" {}
        _CrackTex3 ("Crack Texture 3", 2D) = "" {}
        _CrackTex4 ("Crack Texture 4", 2D) = "" {}
        _CrackTex5 ("Crack Texture 5", 2D) = "" {}
        _CrackTex6 ("Crack Texture 6", 2D) = "" {}
        _CrackTex7 ("Crack Texture 7", 2D) = "" {}
        _CrackTex8 ("Crack Texture 8", 2D) = "" {}
        _CrackTex9 ("Crack Texture 9", 2D) = "" {}
        _DamageAmount ("Damage Amount", Range(0, 1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1" }
        LOD 200
		offset -3,-3
        ZWrite On  
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            sampler2D _CrackTex0;
            sampler2D _CrackTex1;
            sampler2D _CrackTex2;
            sampler2D _CrackTex3;
            sampler2D _CrackTex4;
            sampler2D _CrackTex5;
            sampler2D _CrackTex6;
            sampler2D _CrackTex7;
            sampler2D _CrackTex8;
            sampler2D _CrackTex9;
            float _DamageAmount;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 SelectCrackTexture(float2 uv)
            {
                int crackIndex = int(_DamageAmount * 10);
                if (crackIndex == 0) return tex2D(_CrackTex0, uv);
                if (crackIndex == 1) return tex2D(_CrackTex1, uv);
                if (crackIndex == 2) return tex2D(_CrackTex2, uv);
                if (crackIndex == 3) return tex2D(_CrackTex3, uv);
                if (crackIndex == 4) return tex2D(_CrackTex4, uv);
                if (crackIndex == 5) return tex2D(_CrackTex5, uv);
                if (crackIndex == 6) return tex2D(_CrackTex6, uv);
                if (crackIndex == 7) return tex2D(_CrackTex7, uv);
                if (crackIndex == 8) return tex2D(_CrackTex8, uv);
                return tex2D(_CrackTex9, uv);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Получение текстуры трещины в зависимости от уровня урона
                fixed4 crackColor = SelectCrackTexture(i.uv);

                // Если трещина имеет альфа-канал, она будет отображаться на прозрачном фоне
                return crackColor;
            }
            ENDCG
        }
    }
}
