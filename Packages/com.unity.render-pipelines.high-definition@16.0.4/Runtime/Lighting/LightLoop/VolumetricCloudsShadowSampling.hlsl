#ifndef VOLUMETRIC_CLOUDS_SHADOW_SAMPLING
#define VOLUMETRIC_CLOUDS_SHADOW_SAMPLING

float EvaluateVolumetricCloudsShadows(LightLoopContext lightLoopContext, DirectionalLightData light, float3 positionWS)
{
    // Compute the vector from the shadow origin to the point to shade
    float3 shadowOriginVec = positionWS - _VolumetricCloudsShadowOriginToggle.xyz;

    // Compute the Coordinates of the point in the local space of the shadow
    float xCoord = dot(shadowOriginVec, normalize(light.right)) / _VolumetricCloudsShadowScale.x;
    float yCoord = dot(shadowOriginVec, normalize(light.up)) / _VolumetricCloudsShadowScale.y;
    float zCoord = dot(shadowOriginVec, -light.forward);

    // We let the sampler handle clamping to border.
    float2 uv = saturate(float2(xCoord, yCoord));
    float3 cloudsShadow = SAMPLE_TEXTURE2D_LOD(_VolumetricCloudsShadowsTexture, s_linear_clamp_sampler, uv, 0).rgb;

    // Evaluate the shadow
    float shadowRange = saturate((1.0 / zCoord - cloudsShadow.x) / (cloudsShadow.z - cloudsShadow.x));
    float shadowValue = cloudsShadow.y != 1.0 ? lerp(cloudsShadow.y, 1.0, shadowRange) : 1.0;
    return (all(uv != 0.0) && all(uv != 1.0)) ? shadowValue : _VolumetricCloudsFallBackValue;
}

#endif // VOLUMETRIC_CLOUDS_SHADOW_SAMPLING
