Shader "Custom/Portal"
{
    Properties
    {
        _MaskTexture("Mask Texture", 2D) = "" {}
        _InactiveColor ("Inactive Color", Color) = (1, 1, 1, 1)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100

        Cull Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            sampler2D _MaskTexture;
            float4 _MaskTexture_ST;
            float4 _InactiveColor;
            int displayMask; // set to 1 to display texture, otherwise will draw test colour
            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MaskTexture);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Clip texture UV for portal
                float2 uv = i.screenPos.xy / i.screenPos.w;
                // Import textures
                fixed4 portalCol = tex2D(_MainTex, uv);
                fixed4 maskCol = tex2D(_MaskTexture, i.uv);
                fixed4 borderCol = tex2D(_MaskTexture, i.uv * 1.1 - 0.05);
                fixed4 inactiveCol = _InactiveColor;
                // Apply border to portal
                borderCol.xyz = 1- borderCol.xyz;
                borderCol *= _InactiveColor;
                portalCol += borderCol;
                // Apply mask to portal and mask
                portalCol.a = maskCol.x;
                inactiveCol.a = maskCol.x;
                // Set color
                return portalCol * displayMask + inactiveCol * (1-displayMask);
            }
            ENDCG
        }
    }
    Fallback "Standard" // for shadows
}
