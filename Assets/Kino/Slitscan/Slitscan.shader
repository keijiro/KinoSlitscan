Shader "Hidden/Kino/Slitscan"
{
    Properties
    {
        _CurrentFrame("", 2D) = "" {}
        _SourceTexture("", 2D) = "" {}
        _Texture0("", 2D) = "" {}
        _Texture1("", 2D) = "" {}
        _Texture2("", 2D) = "" {}
        _Texture3("", 2D) = "" {}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _CurrentFrame;
    sampler2D _SourceTexture;
    sampler2D _Texture0;
    sampler2D _Texture1;
    sampler2D _Texture2;
    sampler2D _Texture3;
    float _BitOffset;

    fixed4 ComponentSelector(float index)
    {
        index = round(index);
        fixed x = index < 8;
        fixed y = index < 16;
        fixed z = index < 24;
        return round(fixed4(x, y - x, z - y, 1 - z));
    }

    fixed TestBit(fixed4 data, float index, fixed4 selector)
    {
        float temp = dot(data, selector);
        temp = round(temp * 255);
        temp /= round(pow(2, fmod(index, 8) + 1));
        return frac(temp) >= 0.5;
    }

    fixed4 frag_encode(v2f_img i) : SV_Target
    {
        fixed4 source = tex2D(_SourceTexture, i.uv);
        fixed input = tex2D(_CurrentFrame, i.uv).r > 0.5;

        fixed4 selector = ComponentSelector(round(_BitOffset));
        float current = TestBit(source, _BitOffset, selector);
        float modifier = round((input - current) * round(pow(2, round(fmod(_BitOffset, 8))))) / 255;

        return source + selector * modifier;
    }

    fixed4 frag_decode(v2f_img i) : SV_Target
    {
        float shift = 12.0f;//floor(i.uv.y * 32);
        fixed4 selector = ComponentSelector(shift);
        return TestBit(tex2D(_Texture1, i.uv), shift, selector);


/*
        float index = round(fmod(round((i.uv.y + 1) * 128 - _BitOffset + 31), 128));
        float shift = 31 - round(fmod(index, 32));

        fixed4 selector = ComponentSelector(shift);
        fixed p0 = TestBit(tex2D(_Texture0, i.uv), shift, selector);
        fixed p1 = TestBit(tex2D(_Texture1, i.uv), shift, selector);
        fixed p2 = TestBit(tex2D(_Texture2, i.uv), shift, selector);
        fixed p3 = TestBit(tex2D(_Texture3, i.uv), shift, selector);

        fixed p = p0;
        p = lerp(p, p1, index >= 32);
        p = lerp(p, p2, index >= 64);
        p = lerp(p, p3, index >= 96);
        return p;
        */
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
