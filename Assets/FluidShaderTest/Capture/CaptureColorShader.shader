Shader "Unlit/CaptureColorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NewTex ("New Color", 2D) = "white" {}
        _VelocityField ("New Color", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Transparent"}
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _NewTex;
            float4 _NewTex_ST;
            sampler2D _VelocityField;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //fixed4 col = tex2D(_MainTex, i.uv);
                //fixed4 newCol = tex2D(_NewTex, i.uv);
                return float4(tex2D(_VelocityField, i.uv).rg, 0, 1);
            }
            ENDCG
        }
    }
}
