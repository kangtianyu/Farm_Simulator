Shader "Custom/DoubleSidedClippingPlaneShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _ClipPlane ("Clip Plane", Vector) = (0, 0, 1, 0)
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" }
        
        // ** Double-sided rendering **
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
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _ClipPlane; // Plane equation (a, b, c, d)

            v2f vert(appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Calculate distance from the plane and discard pixels beyond the plane
                float distanceToPlane = dot(float4(i.worldPos, 1.0), _ClipPlane);
                if (distanceToPlane > 0) discard; // Hide pixels beyond the plane

                return tex2D(_MainTex, i.uv); // Apply texture on both sides
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}