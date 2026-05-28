Shader "Custom/StencilOnly"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry-1" }

        // No escribe color en ningún canal (R G B A)
        ColorMask 0

        // No escribe profundidad
        ZWrite Off

        // Solo escribe en el stencil buffer
        Stencil
        {
            Ref 1          // <- el mismo valor que usa tu shader de visión
            Comp Always
            Pass Replace
        }

        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes { float4 positionOS : POSITION; };
            struct Varyings   { float4 positionHCS : SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                return half4(0,0,0,0); // no importa, ColorMask 0 lo descarta
            }
            ENDHLSL
        }
    }
}