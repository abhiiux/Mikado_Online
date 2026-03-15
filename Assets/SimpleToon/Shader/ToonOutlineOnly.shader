Shader "Lpk/LightModel/ToonOutlineOnly"
{
    Properties
    {
        _OutlineWidth ("Outline Width", Range(0, 0.2)) = 0.1
        _OutlineColor ("OutlineColor", Color) = (0.0, 0.0, 0.0, 1)
    }

    SubShader
    {
        Tags
        {
            "Queue" = "Geometry+1"
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
        }

        Pass
        {
            Name "OutlineOnly"
            Cull Front
            ZTest LEqual
            ZWrite On
            Tags
            {
                "LightMode" = "SRPDefaultUnlit"
            }

            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fog
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float _OutlineWidth;
                float4 _OutlineColor;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float fogCoord : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output = (Varyings)0;

                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 outlinePositionOS = input.positionOS.xyz + input.normalOS * _OutlineWidth;

                VertexPositionInputs vertexInput = GetVertexPositionInputs(outlinePositionOS);
                output.positionCS = vertexInput.positionCS;
                output.fogCoord = ComputeFogFactor(output.positionCS.z);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half3 finalColor = MixFog(_OutlineColor.rgb, input.fogCoord);
                return half4(finalColor, _OutlineColor.a);
            }
            ENDHLSL
        }
    }
}
