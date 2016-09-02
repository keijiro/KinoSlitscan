Shader "Hidden/Kino/Slitscan"
{
    Properties
    {
        _Texture0("", 2D) = "" {}
        _Texture1("", 2D) = "" {}
        _Texture2("", 2D) = "" {}
        _Texture3("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _Texture0;
    sampler2D _Texture1;
    sampler2D _Texture2;
    sampler2D _Texture3;
    sampler2D _Texture4;
    sampler2D _Texture5;
    sampler2D _Texture6;
    sampler2D _Texture7;

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

    sampler2D _MainTex;

    float _SliceScale;
    float _SliceOffset;

    v2f vert_composit(appdata v)
    {
        v2f o;

        float x = v.vertex.x * 2;
        float y = v.vertex.y * 2 * _SliceScale + _SliceOffset;
        o.vertex = float4(x, y, 1, 1);

        o.uv = float2(v.uv.x, y / 2 + 0.5);
        o.selector = v.uv.y;

        return o;
    }

    fixed4 frag_composit(v2f i) : SV_Target
    {
        float s = i.selector * 8;
        fixed4 p = tex2D(_Texture0, i.uv);
        p = lerp(p, tex2D(_Texture1, i.uv), s >= 1);
        p = lerp(p, tex2D(_Texture2, i.uv), s >= 2);
        p = lerp(p, tex2D(_Texture3, i.uv), s >= 3);
        p = lerp(p, tex2D(_Texture4, i.uv), s >= 4);
        p = lerp(p, tex2D(_Texture5, i.uv), s >= 5);
        p = lerp(p, tex2D(_Texture6, i.uv), s >= 6);
        p = lerp(p, tex2D(_Texture7, i.uv), s >= 7);
        return p;
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_composit
            #pragma fragment frag_composit
            #pragma target 3.0
            ENDCG
        }
    }
}
