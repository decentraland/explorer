﻿Shader "DCL/Unlit Cutout Tinted" {
Properties {
    _BaseMap ("Base (RGB) Trans (A)", 2D) = "white" {}
    _TintMask ("Mask for tint (Monochannel) (1 == tint, 0 == no tint)", 2D) = "white" {}
    _Cutoff ("Alpha cutoff", Range(0,1)) = 0.5
    _BaseColor ("Color", Color) = (0,0,0,0)
}
SubShader {
    Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
    LOD 100
	
    Lighting Off
  
    Pass {
        CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 2.0
            #pragma multi_compile_fog
 
            #include "UnityCG.cginc"
 
            struct appdata_t {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };
 
            struct v2f {
                float4 vertex : SV_POSITION;
                float2 texcoord : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                UNITY_VERTEX_OUTPUT_STEREO
            };
 
            sampler2D _BaseMap;
            float4 _BaseMap_ST;
            sampler2D _TintMask;
            fixed _Cutoff;
            fixed4 _BaseColor;
 
            v2f vert (appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.texcoord = TRANSFORM_TEX(v.texcoord, _BaseMap);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }
 
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_BaseMap, i.texcoord);
                fixed4 tintMask = tex2D(_TintMask, i.texcoord);

                col *= lerp(float4(1,1,1,1), _BaseColor, tintMask.r);

                clip(col.a - _Cutoff);
                UNITY_APPLY_FOG(i.fogCoord, col);
                return col;
            }
        ENDCG
    }
}
 
}