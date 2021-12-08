Shader "PX/UGUI_GrayImage"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _GrayAmount("GrayAmount",Range(0.0,1)) = 1.0
    }
    SubShader
    {

        Pass
        {
            Tags {"Queue"="Transparent"}
            
            ZWrite Off
            Blend SrcAlpha OneMinusSrcAlpha

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            uniform sampler2D _MainTex;
            fixed _GrayAmount;

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 renderTex = tex2D(_MainTex,i.uv);
                fixed4 finalColor = renderTex;
                if (renderTex.a > 0)
                {
                    float luminosity = 0.299 * renderTex.r + 0.587 * renderTex.g + 0.114 * renderTex.b;
                    finalColor = lerp(renderTex,luminosity,_GrayAmount);
                    // finalColor.a = renderTex.a;
                }
                
                return finalColor;
            }
            ENDCG
        }
    }
}