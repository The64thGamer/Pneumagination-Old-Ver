Shader "Unlit/Cel Animator"
{
    Properties
    {
        _Diffuse ("Diffuse", 2D) = "white" {}
        _Mask ("Mask", 2D) = "white" {}
        _Normal ("Normal", 2D) = "bump" {}
        
        _Palette ("Palette", 2D) = "black" {}

        _Palette_0("Palette 0", Color) = (.75, .1, .2, 1)
        _Palette_1("Palette 1", Color) = (.55, .7, .3, 1)
        _Palette_2("Palette 2", Color) = (.45, .5, .4, 1)
        _Palette_3("Palette 3", Color) = (.25, .2, .5, 1)

        _Cel_Highlight("Cel Highlight", Color) = (1, 1, 1, 1)
        _Cel_Midtone("Cel Midtone", Color) = (.5, .5, .5, 1)
        _Cel_Shadow("Cel Shadow", Color) = (.25, .25, .25, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
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
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _Diffuse;
            float4 _Diffuse_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _Diffuse);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_Diffuse, i.uv);
                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
            ENDCG
        }
    }
}
