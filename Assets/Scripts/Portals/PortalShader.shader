Shader "Custom/Portal"
{
    Properties
    {
        _InactiveColor ("Inactive Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
        }
        LOD 100
        Cull Off

        Pass
        {
            CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag

                #include "UnityCG.cginc"

                struct appdata
                {
                    float4 vertex : POSITION;
                };

                struct v2f
                {
                    float4 vertex : SV_POSITION;
                    float4 screenPos : TEXCOORD0;
                };
                
                uniform sampler2D _MainTex;
                float4 _InactiveColor;
                int displayMask;

                v2f vert (appdata v)
                {
                    v2f o;
                    o.vertex = UnityObjectToClipPos(v.vertex);
                    o.screenPos = ComputeScreenPos(o.vertex);
                    return o;
                }

                fixed4 frag(v2f i) : SV_Target
                {
                    float2 uv = i.screenPos.xy / i.screenPos.w;
                    fixed4 col = tex2D(_MainTex, uv);
                    return col * displayMask + _InactiveColor * (1 - displayMask);
                }
            ENDCG
        }
    }
}