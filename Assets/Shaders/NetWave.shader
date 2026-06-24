Shader "Custom/NetWave"
{
    Properties
    {
        _MainTex   ("Texture",        2D)    = "white" {}
        _WaveAmp   ("Wave Amplitude", Float) = 0.018
        _WaveFreq  ("Wave Frequency", Float) = 7.0
        _WaveSpeed ("Wave Speed",     Float) = 4.0
        _Intensity ("Intensity",      Float) = 0.0
        _Color     ("Tint",           Color) = (1,1,1,1)
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            struct v2f
            {
                float4 pos    : SV_POSITION;
                float2 uv     : TEXCOORD0;
                float4 color  : COLOR;
            };

            sampler2D _MainTex;
            float4    _MainTex_ST;
            float     _WaveAmp;
            float     _WaveFreq;
            float     _WaveSpeed;
            float     _Intensity;
            float4    _Color;

            v2f vert(appdata v)
            {
                v2f o;
                o.pos   = UnityObjectToClipPos(v.vertex);
                o.uv    = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;
                // UV'yi yatayda sine dalgasıyla kaydır → ağ sallanır
                float wave = sin(uv.y * _WaveFreq + _Time.y * _WaveSpeed) * _WaveAmp * _Intensity;
                uv.x += wave;
                fixed4 col = tex2D(_MainTex, uv) * i.color;
                return col;
            }
            ENDCG
        }
    }

    Fallback "Sprites/Default"
}
