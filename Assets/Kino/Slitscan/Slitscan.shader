Shader "Hidden/Kino/Slitscan"
{
    Properties
    {
        _MainTex("", 2D) = "" {}

        _YTexture0("", 2D) = "" {}
        _YTexture1("", 2D) = "" {}
        _YTexture2("", 2D) = "" {}
        _YTexture3("", 2D) = "" {}

        _CgTexture0("", 2D) = "" {}
        _CgTexture1("", 2D) = "" {}
        _CgTexture2("", 2D) = "" {}
        _CgTexture3("", 2D) = "" {}

        _CoTexture0("", 2D) = "" {}
        _CoTexture1("", 2D) = "" {}
        _CoTexture2("", 2D) = "" {}
        _CoTexture3("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    sampler2D _YTexture0;
    sampler2D _YTexture1;
    sampler2D _YTexture2;
    sampler2D _YTexture3;

    sampler2D _CgTexture0;
    sampler2D _CgTexture1;
    sampler2D _CgTexture2;
    sampler2D _CgTexture3;

    sampler2D _CoTexture0;
    sampler2D _CoTexture1;
    sampler2D _CoTexture2;
    sampler2D _CoTexture3;

    // Color space conversion shader (RGB => Luminance)
    fixed frag_encode_y(v2f_img i) : SV_Target
    {
        fixed4 src = tex2D(_MainTex, i.uv);
        return dot(src.rgb, fixed3(0.25, 0.5, 0.25));
    }

    // Color space conversion shader (RGB => chrominance)
    struct ChrominanceOutput
    {
        fixed cg : SV_Target0;
        fixed co : SV_Target1;
    };

    ChrominanceOutput frag_encode_cgco(v2f_img i)
    {
        fixed4 src = tex2D(_MainTex, i.uv);
        ChrominanceOutput o;
        o.cg = dot(src.rgb, fixed3(-0.25, 0.5, -0.25)) + 0.5;
        o.co = (src.r - src.b) * 0.5 + 0.5;
        return o;
    }

    // Slice rendering shader
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float3 texcoord : TEXCOORD0;
    };

    float _SliceScale;
    float _SliceOffset;

    fixed3 SampleYCgCo(float2 uv, sampler2D yTex, sampler2D cgTex, sampler2D coTex)
    {
        fixed y = tex2D(yTex, uv).r;
        fixed cg = tex2D(cgTex, uv).r - 0.5;
        fixed co = tex2D(coTex, uv).r - 0.5;
        fixed temp = y - cg;
        return fixed3(temp + co, y + cg, temp - co);
    }

    v2f vert_decode(appdata v)
    {
        float x = v.vertex.x * 2;
        float y = v.vertex.y * 2 * _SliceScale + _SliceOffset;

        v2f o;
        o.vertex = float4(x, y, 1, 1);
        o.texcoord = float3(v.uv.xy, y * 0.5 + 0.5);
        return o;
    }

    half4 frag_decode(v2f i) : SV_Target
    {
        float2 uv = i.texcoord.xz;
        float selector = i.texcoord.y * 4;

        fixed3 c0 = SampleYCgCo(uv, _YTexture0, _CgTexture0, _CoTexture0);
        fixed3 c1 = SampleYCgCo(uv, _YTexture1, _CgTexture1, _CoTexture1);
        fixed3 c2 = SampleYCgCo(uv, _YTexture2, _CgTexture2, _CoTexture2);
        fixed3 c3 = SampleYCgCo(uv, _YTexture3, _CgTexture3, _CoTexture3);

        fixed3 color = lerp(c0, c1, selector > 1);
        color = lerp(color, c2, selector > 2);
        color = lerp(color, c3, selector > 3);
        return half4(color, 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_encode_y
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_encode_cgco
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_decode
            #pragma fragment frag_decode
            #pragma target 3.0
            ENDCG
        }
    }
}
