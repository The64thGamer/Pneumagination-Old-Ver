float3 viewDirWS = normalize(ViewDirWS);

float3 normalWS = normalize(NormalWS);

float3 reflDir = normalize(reflect(-viewDirWS, normalWS));



float3 factors = ((reflDir > 0 ? unity_SpecCube0_BoxMax.xyz : unity_SpecCube0_BoxMin.xyz) - PositionWS) / reflDir;

float scalar = min(min(factors.x, factors.y), factors.z);

float3 uvw = reflDir * scalar + (PositionWS - unity_SpecCube0_ProbePosition.xyz);



float4 sampleRefl = SAMPLE_TEXTURECUBE_LOD(unity_SpecCube0, samplerunity_SpecCube0, uvw, LOD);

float3 specCol = DecodeHDREnvironment(sampleRefl, unity_SpecCube0_HDR);

Color = specCol;