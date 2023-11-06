Shader "Unlit/TDAFluidColorShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _OrigTex ("Orig Texture", 2D) = "white" {}
        _CapTex ("Captured Texture", 2D) = "white" {}
        _VelocityField ("Velocity Field", 2D) = "" {}
    }

    CGINCLUDE
    #include "UnityCG.cginc"

    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float2 uv : TEXCOORD0;
        UNITY_FOG_COORDS(1)
        float4 vertex : SV_POSITION;
    };

    sampler2D _MainTex;
    float4 _MainTex_ST;
    float4 _MainTex_TexelSize;
    sampler2D _OrigTex;
    sampler2D _CapTex;
    float4 _CapTex_ST;
    float4 _CapTex_TexelSize;

    sampler2D _VelocityField;

    float DIFF_A;
    float DIFF_B;

    v2f vert (appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = TRANSFORM_TEX(v.uv, _MainTex);
        UNITY_TRANSFER_FOG(o,o.vertex);
        return o;
    }

    fixed4 frag_diffuse (v2f i) : SV_Target
    {
        float2 sizeAdjust = _MainTex_TexelSize.zw;
        
        fixed4 col = (
            (tex2D(_MainTex, i.uv - sizeAdjust * float2(1, 0)) + 
            tex2D(_MainTex, i.uv + sizeAdjust * float2(1, 0)) +
            tex2D(_MainTex, i.uv - sizeAdjust * float2(0, 1)) + 
            tex2D(_MainTex, i.uv + sizeAdjust * float2(0, 1))) +
            tex2D(_OrigTex, i.uv) * 1000) / 1004;
            //tex2D(_OrigTex, i.uv) * DIFF_A) / DIFF_B;
        

        return col;
    }

    fixed4 frag_advect (v2f i) : SV_Target
    {
        float DELTA_T = unity_DeltaTime.x;

        float2 normToAspect = float2(_MainTex_TexelSize.x/ _MainTex_TexelSize.y , 1);

        float2 disp = tex2D(_VelocityField, i.uv).xy * normToAspect * DELTA_T;

        fixed4 col = tex2D(_MainTex, (i.uv - disp));

        fixed4 capturedColor = tex2D(_CapTex, i.uv);
        return lerp(col, capturedColor, capturedColor.a);
        
        //return fixed4(tex2D(_VelocityField, i.uv).xy * normToAspect, 0, 1);
    }

    ENDCG

    SubShader
    {
        Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        Cull back
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_diffuse
            ENDCG
        }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag_advect
            ENDCG
        }
    }
}
