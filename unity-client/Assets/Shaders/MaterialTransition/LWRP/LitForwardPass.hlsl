#ifndef LIGHTWEIGHT_FORWARD_LIT_PASS_INCLUDED
#define LIGHTWEIGHT_FORWARD_LIT_PASS_INCLUDED

#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
#include "FadeDithering.hlsl"

struct Attributes
{
    float4 positionOS   : POSITION;
    float3 normalOS     : NORMAL;
    float4 tangentOS    : TANGENT;
    float2 texcoord     : TEXCOORD0;
    float2 texcoord1     : TEXCOORD1;
    UNITY_VERTEX_INPUT_INSTANCE_ID
};

struct Varyings
{
    float4 uvAlbedoNormal           : TEXCOORD0; //Albedo, Normal UVs
    float4 uvMetallicEmissive       : TEXCOORD1; //Metallic, Emissive UVs

    float3 positionWS               : TEXCOORD2;

#ifdef _NORMALMAP
    half4 normalWS                  : TEXCOORD3;    // xyz: normal, w: viewDir.x
    half4 tangentWS                 : TEXCOORD4;    // xyz: tangent, w: viewDir.y
    half4 bitangentWS                : TEXCOORD5;    // xyz: bitangent, w: viewDir.z
#else
    half3 normalWS                  : TEXCOORD3;
    half3 viewDirWS                 : TEXCOORD4;
#endif

    half4 fogFactorAndVertexLight   : TEXCOORD6; // x: fogFactor, yzw: vertex light

#ifdef _MAIN_LIGHT_SHADOWS
    float4 shadowCoord              : TEXCOORD7;
#endif
	
	//NOTE(Brian): needed for FadeDithering
	float4 positionSS               : TEXCOORD8;

	float4 positionCS               : SV_POSITION;

    UNITY_VERTEX_INPUT_INSTANCE_ID
    UNITY_VERTEX_OUTPUT_STEREO
};

void InitializeInputData(Varyings input, half3 normalTS, out InputData inputData)
{
    inputData = (InputData)0;

    inputData.positionWS = input.positionWS;

#ifdef _NORMALMAP
    half3 viewDirWS = half3(input.normalWS.w, input.tangentWS.w, input.bitangentWS.w);
    inputData.normalWS = TransformTangentToWorld(normalTS,
        half3x3(input.tangentWS.xyz, input.bitangentWS.xyz, input.normalWS.xyz));
#else
    half3 viewDirWS = input.viewDirWS;
    inputData.normalWS = input.normalWS;
#endif

    inputData.normalWS = NormalizeNormalPerPixel(inputData.normalWS);
    viewDirWS = SafeNormalize(viewDirWS);

    inputData.viewDirectionWS = viewDirWS;
#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    inputData.shadowCoord = input.shadowCoord;
#else
    inputData.shadowCoord = float4(0, 0, 0, 0);
#endif
    inputData.fogCoord = input.fogFactorAndVertexLight.x;
    inputData.vertexLighting = input.fogFactorAndVertexLight.yzw;
    inputData.bakedGI = SAMPLE_GI(input.lightmapUV, half3(0,0,0), inputData.normalWS);
}

///////////////////////////////////////////////////////////////////////////////
//                  Vertex and Fragment functions                            //
///////////////////////////////////////////////////////////////////////////////

// Used in Standard (Physically Based) shader
Varyings LitPassVertex(Attributes input)
{
    Varyings output = (Varyings)0;

    UNITY_SETUP_INSTANCE_ID(input);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

    VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
    VertexNormalInputs normalInput = GetVertexNormalInputs(input.normalOS, input.tangentOS);
    half3 viewDirWS = GetCameraPositionWS() - vertexInput.positionWS;
    half3 vertexLight = VertexLighting(vertexInput.positionWS, normalInput.normalWS);
    half fogFactor = ComputeFogFactor(vertexInput.positionCS.z);

    float2 uvs[] = { TRANSFORM_TEX(input.texcoord, _BaseMap), TRANSFORM_TEX(input.texcoord1, _BaseMap)};
    output.uvAlbedoNormal.xy = uvs[clamp(_BaseMapUVs, 0, 1)];
    output.uvAlbedoNormal.zw = uvs[clamp(_NormalMapUVs, 0, 1)];
    output.uvMetallicEmissive.xy = uvs[clamp(_MetallicMapUVs, 0, 1)];
    output.uvMetallicEmissive.zw = uvs[clamp(_EmissiveMapUVs, 0, 1)];

#ifdef _NORMALMAP
    output.normalWS = half4(normalInput.normalWS, viewDirWS.x);
    output.tangentWS = half4(normalInput.tangentWS, viewDirWS.y);
    output.bitangentWS = half4(normalInput.bitangentWS, viewDirWS.z);
#else
    output.normalWS = NormalizeNormalPerVertex(normalInput.normalWS);
    output.viewDirWS = viewDirWS;
#endif
    output.fogFactorAndVertexLight = half4(fogFactor, vertexLight);

    output.positionWS = vertexInput.positionWS;

#if defined(_MAIN_LIGHT_SHADOWS) && !defined(_RECEIVE_SHADOWS_OFF)
    output.shadowCoord = GetShadowCoord(vertexInput);
#endif

    output.positionCS = vertexInput.positionCS;
	
	//NOTE(Brian): needed for FadeDithering
	output.positionSS = ComputeScreenPos(vertexInput.positionCS);

    return output;
}

// Used in Standard (Physically Based) shader
half4 LitPassFragment(Varyings input) : SV_Target
{
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

    SurfaceData surfaceData;
    InitializeStandardLitSurfaceData(input.uvAlbedoNormal.xy, input.uvAlbedoNormal.zw, input.uvMetallicEmissive.xy, input.uvMetallicEmissive.zw, surfaceData);

    InputData inputData;
    InitializeInputData(input, surfaceData.normalTS, inputData);

    half4 color = LightweightFragmentPBR(inputData, surfaceData.albedo, surfaceData.metallic, surfaceData.specular, surfaceData.smoothness, surfaceData.occlusion, surfaceData.emission, surfaceData.alpha);

    color.rgb = MixFog(color.rgb, inputData.fogCoord);

	color = fadeDithering(color, input.positionWS, input.positionSS);

    return color;
}

#endif
