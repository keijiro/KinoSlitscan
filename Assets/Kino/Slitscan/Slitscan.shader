Shader "Hidden/Kino/Slitscan"
{
    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _CurrentFrame;
    sampler2D _SourceTexture;

    float _BitIndex;

    float CheckBit(float x, float index)
    {
        x *= 255;
        return frac(x / pow(2, index + 1)) >= 0.5;
    }

    fixed4 frag_encode(v2f_img i) : SV_Target
    {
        float bw = tex2D(_CurrentFrame, i.uv).r > 0.5;

        float p = tex2D(_SourceTexture, i.uv).r;

        float cur = CheckBit(p.r, _BitIndex);

        p += (bw - cur) * pow(2, _BitIndex) / 255;

        return p;
    }

    sampler2D _Texture0;
    sampler2D _Texture1;
    sampler2D _Texture2;
    sampler2D _Texture3;

    fixed4 frag_decode(v2f_img i) : SV_Target
    {
        float index = fmod(trunc((i.uv.y + 1) * 32 - _BitIndex + 7), 32);

        float shift = 7 - fmod(index, 8);

        fixed p0 = CheckBit(tex2D(_Texture0, i.uv).r, shift);
        fixed p1 = CheckBit(tex2D(_Texture1, i.uv).r, shift);
        fixed p2 = CheckBit(tex2D(_Texture2, i.uv).r, shift);
        fixed p3 = CheckBit(tex2D(_Texture3, i.uv).r, shift);

        float bl = index / 8;
        fixed p = p0;
        p = lerp(p, p1, bl >= 1);
        p = lerp(p, p2, bl >= 2);
        p = lerp(p, p3, bl >= 3);
        return p;
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_encode
            #pragma target 3.0
            ENDCG
        }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag_decode
            #pragma target 3.0
            ENDCG
        }
    }
}
