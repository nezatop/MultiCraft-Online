Shader "Custom/WaterShader"
{
    Properties
    {
        _Color ("Water Color", Color) = (0, 0.5, 1, 0.5)
        _MainTex ("Water Texture", 2D) = "white" {}
        _WaveStrength ("Wave Strength", Range(0, 1)) = 0.1
        _WaveSpeed ("Wave Speed", Range(0, 10)) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" }
        LOD 200
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "UnityCG.cginc"
            
            struct appdata_t
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float4 worldPos : TEXCOORD1;
            };
            
            sampler2D _MainTex;
            float4 _Color;
            float _WaveStrength;
            float _WaveSpeed;

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

                float waveOffset = sin(v.vertex.x * 10.0 + _Time.y * _WaveSpeed) * _WaveStrength;
                o.vertex.y += waveOffset;

                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.uv = v.uv;
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // Получаем текстуру
                float4 texColor = tex2D(_MainTex, uv);
                
                float4 finalColor = texColor * _Color;

                return finalColor;
            }
            ENDCG
        }
    }
}