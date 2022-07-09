Shader "Unlit/WaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Water ("Water", 3D) = "red" {}
        _WaterSamplePoints ("Sample Points", int) = 10
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
                float4 vertex : SV_POSITION;
                float3 vertexPos : TEXCORD0;
                float3 rayToSurface : TEXCORD1;
                // float3 pos : TEXCORD2;
            };

            sampler2D _MainTex;
            sampler3D _Water;
            float4 _MainTex_ST;
            float3 _Size;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.vertexPos = v.vertex;

                float3 worldPosVertex = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.rayToSurface = worldPosVertex - _WorldSpaceCameraPos;
                // o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                // UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex3D(_Water, i.vertexPos);
                // apply fog
                // UNITY_APPLY_FOG(i.fogCoord, col);
                float density = 0;

                float3 scanPos = i.vertexPos;

                return col;
            }
            ENDCG
        }
    }
}
