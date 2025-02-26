Shader "Custom/BackShaderWithTexture"
{
    Properties
    {
        _MainTex ("Grass Texture", 2D) = "white" {}
        _NoiseScale ("Noise Scale", Float) = 10.0
        _Color ("Color", Color) = (1,1,1,1)
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _NoiseScale;
            fixed4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Calculate noise based on world position
                float2 noiseUV = i.worldPos.xy / _NoiseScale;
                fixed4 noise = tex2D(_MainTex, noiseUV);

                // Apply color and noise
                fixed4 col = noise * _Color;
                return col;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}