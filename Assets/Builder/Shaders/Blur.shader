Shader "Builder/Blur" 
{
    Properties
    {
        _MainTex("Base (RGB)", 2D) = "white" { }
    }

    SubShader
    {
        ZTest Always Cull Off ZWrite Off Fog{ Mode Off }

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;
            float step_w;
            float step_h;

            struct v2f 
            {
                float4  pos : SV_POSITION;
                float2  uv : TEXCOORD0;
            };

            float4 _MainTex_ST;
            float4 _MainTex_ST_TexelSize;

            v2f vert(appdata_base v) 
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
                return o;
            }

            fixed4 frag(v2f i) : COLOR
            {
                step_w = _MainTex_TexelSize.x;
                step_h = _MainTex_TexelSize.y;

                float2 offset[25] = 
                {
                    float2(-step_w*2.0, -step_h*2.0), float2(-step_w, -step_h*2.0),  float2(0.0, -step_h*2.0), float2(step_w, -step_h*2.0), float2(step_w*2.0, -step_h*2.0),
                    float2(-step_w*2.0, -step_h),     float2(-step_w, -step_h),      float2(0.0, -step_h),     float2(step_w, -step_h),     float2(step_w*2.0, -step_h),
                    float2(-step_w*2.0, 0.0),         float2(-step_w, 0.0),          float2(0.0, 0.0),         float2(step_w, 0.0),         float2(step_w*2.0, 0.0),
                    float2(-step_w*2.0, step_h),      float2(-step_w, step_h),       float2(0.0, step_h),      float2(step_w, step_h),      float2(step_w*2.0, step_h),
                    float2(-step_w*2.0, step_h*2.0),  float2(-step_w, step_h*2.0),   float2(0.0, step_h*2.0),  float2(step_w, step_h * 20),   float2(step_w*2.0, step_h*2.0)
                };

                float kernel[25] = 
                {

                    0.003765,    0.015019,    0.023792,    0.015019,    0.003765,
                    0.015019,    0.059912,    0.094907,    0.059912,    0.015019,
                    0.023792,    0.094907,    0.150342,    0.094907,    0.023792,
                    0.015019,    0.059912,    0.094907,    0.059912,    0.015019,
                    0.003765,    0.015019,    0.023792,    0.015019,    0.003765
                };

                float4 sum = float4(0.0, 0.0, 0.0, 0.0);

                for (int j = 0; j < 25; j++) 
                {
                    float4 tmp = tex2D(_MainTex, i.uv + offset[j]);
                    sum += tmp * kernel[j];
                }

                return sum;
            }

        ENDCG //Shader End
        }

    }

}