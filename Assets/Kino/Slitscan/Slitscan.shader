Shader "Hidden/Kino/Slitscan"
{
    Properties
    {
        _MainTex("", 2D) = ""{}
    }

    CGINCLUDE

    #include "UnityCG.cginc"

    sampler2D _MainTex;

    float _Origin;
    float _Height;

    half4 frag(v2f_img i) : SV_Target
    {
        if (i.uv.y < _Origin || i.uv.y > _Origin + _Height)
            discard;
        return tex2D(_MainTex, i.uv);
    }

    ENDCG

    SubShader
    {
        Cull Off ZWrite Off ZTest Always
        Pass
        {
            CGPROGRAM
            #pragma vertex vert_img
            #pragma fragment frag
            #pragma target 3.0
            ENDCG
        }
    }
}
