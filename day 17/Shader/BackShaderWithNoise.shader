Shader "Custom/BackShaderWithNoise"
{
    Properties
    {
        _Scale ("Noise Scale", Float) = 5.0
        _CameraPosition ("Camera Position", Vector) = (0,0,0,0)
        _GrassColor ("Grass Color", Color) = (0.2, 0.5, 0.2, 1)
        _DirtColor ("Dirt Color", Color) = (0.5, 0.3, 0.1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            float _Scale;
            float4 _CameraPosition;
            float4 _GrassColor;
            float4 _DirtColor;

            float random(float2 p)
            {
                return frac(sin(dot(p, float2(12.9898,78.233))) * 43758.5453);
            }

            float noise(float2 uv)
            {
                float2 p = floor(uv);
                float2 f = frac(uv);
                f = f * f * (3.0 - 2.0 * f);

                float n00 = random(p);
                float n10 = random(p + float2(1.0, 0.0));
                float n01 = random(p + float2(0.0, 1.0));
                float n11 = random(p + float2(1.0, 1.0));

                float mixX1 = lerp(n00, n10, f.x);
                float mixX2 = lerp(n01, n11, f.x);
                return lerp(mixX1, mixX2, f.y);
            }

            v2f vert (appdata_t v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv * _Scale - _CameraPosition.xy; // Fixed direction
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float n = noise(i.uv);
                return lerp(_DirtColor, _GrassColor, smoothstep(0.3, 0.7, n));
            }
            ENDCG
        }
    }
}
