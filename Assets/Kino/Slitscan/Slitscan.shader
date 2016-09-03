Shader "Hidden/Kino/Slitscan"
{
    Properties
    {
        _MainTex("", 2D) = "" {}
        _LumaTexture0("", 2D) = "" {}
        _LumaTexture1("", 2D) = "" {}
        _LumaTexture2("", 2D) = "" {}
        _LumaTexture3("", 2D) = "" {}
        _ChromaTexture0("", 2D) = "" {}
        _ChromaTexture1("", 2D) = "" {}
        _ChromaTexture2("", 2D) = "" {}
        _ChromaTexture3("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;
    float4 _MainTex_TexelSize;

    sampler2D _LumaTexture0;
    sampler2D _LumaTexture1;
    sampler2D _LumaTexture2;
    sampler2D _LumaTexture3;

    sampler2D _ChromaTexture0;
    sampler2D _ChromaTexture1;
    sampler2D _ChromaTexture2;
    sampler2D _ChromaTexture3;

    // MRT output struct for the compressor
    struct CompressorOutput
    {
        half4 luma : SV_Target0;
        half4 chroma : SV_Target1;
    };

    // Frame compression fragment shader
    CompressorOutput frag_encode(v2f_img i)
    {
        float sw = _ScreenParams.x;     // Screen width
        float pw = _ScreenParams.z - 1; // Pixel width

        // RGB to YCbCr convertion matrix
        const half3 kY  = half3( 0.299   ,  0.587   ,  0.114   );
        const half3 kCB = half3(-0.168736, -0.331264,  0.5     );
        const half3 kCR = half3( 0.5     , -0.418688, -0.081312);

        // 0: even column, 1: odd column
        half odd = frac(i.uv.x * sw * 0.5) > 0.5;

        // Calculate UV for chroma componetns.
        // It's between the even and odd columns.
        float2 uv_c = i.uv.xy;
        uv_c.x = (floor(uv_c.x * sw * 0.5) * 2 + 1) * pw;

        // Sample the source texture.
        half3 rgb_y = tex2D(_MainTex, i.uv).rgb;
        half3 rgb_c = tex2D(_MainTex, uv_c).rgb;

    #if !UNITY_COLORSPACE_GAMMA
        rgb_y = LinearToGammaSpace(rgb_y);
        rgb_c = LinearToGammaSpace(rgb_c);
    #endif

        // Convertion and subsampling
        CompressorOutput o;
        o.luma = dot(kY, rgb_y);
        o.chroma = dot(lerp(kCB, kCR, odd), rgb_c) + 0.5;
        return o;
    }

    // Sample luma-chroma textures and convert to RGB
    half3 DecodeHistory(float2 uvLuma, float2 uvCb, float2 uvCr, sampler2D lumaTex, sampler2D chromaTex)
    {
        half y = tex2D(lumaTex, uvLuma).r;
        half cb = tex2D(chromaTex, uvCb).r - 0.5;
        half cr = tex2D(chromaTex, uvCr).r - 0.5;
        return y + half3(1.402 * cr, -0.34414 * cb - 0.71414 * cr, 1.772 * cb);
    }

    //
    struct appdata
    {
        float4 vertex : POSITION;
        float2 uv : TEXCOORD0;
    };

    struct v2f
    {
        float4 vertex : SV_POSITION;
        float2 uv : TEXCOORD0;
        float selector : TEXCOORD1;
    };

    float _SliceScale;
    float _SliceOffset;

    v2f vert_decode(appdata v)
    {
        v2f o;

        float x = v.vertex.x * 2;
        float y = v.vertex.y * 2 * _SliceScale + _SliceOffset;
        o.vertex = float4(x, y, 1, 1);

        o.uv = float2(v.uv.x, y / 2 + 0.5);
        o.selector = v.uv.y;

        return o;
    }

    //
    half4 frag_decode(v2f i) : SV_Target
    {
        float selector = i.selector * 4;

        float sw = _MainTex_TexelSize.z; // Texture width
        float pw = _MainTex_TexelSize.x; // Texel width

        // UV for luma
        float2 uvLuma = i.uv;

        // UV for Cb (even columns)
        float2 uvCb = i.uv;
        uvCb.x = (floor(uvCb.x * sw * 0.5) * 2 + 0.5) * pw;

        // UV for Cr (even columns)
        float2 uvCr = uvCb;
        uvCr.x += pw;

        half3 acc;
        acc =           DecodeHistory(uvLuma, uvCb, uvCr, _LumaTexture0, _ChromaTexture0);
        acc = lerp(acc, DecodeHistory(uvLuma, uvCb, uvCr, _LumaTexture1, _ChromaTexture1), selector > 1);
        acc = lerp(acc, DecodeHistory(uvLuma, uvCb, uvCr, _LumaTexture2, _ChromaTexture2), selector > 2);
        acc = lerp(acc, DecodeHistory(uvLuma, uvCb, uvCr, _LumaTexture3, _ChromaTexture3), selector > 3);

#if !UNITY_COLORSPACE_GAMMA
        acc = GammaToLinearSpace(acc);
#endif

        return half4(acc, 1);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_img
            #pragma fragment frag_encode
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma multi_compile _ UNITY_COLORSPACE_GAMMA
            #pragma vertex vert_decode
            #pragma fragment frag_decode
            #pragma target 3.0
            ENDCG
        }
    }
}
