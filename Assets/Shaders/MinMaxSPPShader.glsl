import lib-sss.glsl
import lib-pbr.glsl
import lib-emissive.glsl
import lib-pom.glsl
import lib-utils.glsl

//: metadata {
//:   "mdl":"mdl::alg::materials::skin_metallic_roughness::skin_metallic_roughness"
//: }

//: param auto channel_basecolor
uniform SamplerSparse basecolor_tex;
//: param auto channel_roughness
uniform SamplerSparse roughness_tex;
//: param auto channel_metallic
uniform SamplerSparse metallic_tex;
//: param auto channel_specularlevel
uniform SamplerSparse specularlevel_tex;

//: param auto channel_user0
uniform SamplerSparse palleteMask1;
//: param auto channel_user1
uniform SamplerSparse palleteMask2;
//: param auto channel_user2
uniform SamplerSparse palleteMask3;
//: param auto channel_user3
uniform SamplerSparse palleteMask4;

//: param custom { "default": 0, "label": "Palette1", "widget": "color" }
uniform vec3 u_palette1_float3;
//: param custom { "default": 0, "label": "Palette2", "widget": "color" }
uniform vec3 u_palette2_float3;
//: param custom { "default": 0, "label": "Palette3", "widget": "color" }
uniform vec3 u_palette3_float3;
//: param custom { "default": 0, "label": "Palette4", "widget": "color" }
uniform vec3 u_palette4_float3;

float sampleWithDefault(SamplerSparse sampler, SparseCoord coord, float defaultValue)
{
	vec2 value = textureSparse(sampler, coord).rg;
	return value.r + defaultValue*(1.0 - value.r);
}

void shade(V2F inputs)
{
  // Apply parallax occlusion mapping if possible
  vec3 viewTS = worldSpaceToTangentSpace(getEyeVec(inputs.position), inputs);
  applyParallaxOffset(inputs, viewTS);

  // Fetch material parameters, and conversion to the specular/roughness model
  float roughness = getRoughness(roughness_tex, inputs.sparse_coord);
  vec3 baseColor = getBaseColor(basecolor_tex, inputs.sparse_coord);

  float mask1 = sampleWithDefault(palleteMask1, inputs.sparse_coord, 0.0);
  float mask2 = sampleWithDefault(palleteMask2, inputs.sparse_coord, 0.0);
  float mask3 = sampleWithDefault(palleteMask3, inputs.sparse_coord, 0.0);
  float mask4 = sampleWithDefault(palleteMask4, inputs.sparse_coord, 0.0);

  baseColor = mix(baseColor, u_palette1_float3, mask1);
  baseColor = mix(baseColor, u_palette2_float3, mask2);
  baseColor = mix(baseColor, u_palette3_float3, mask3);
  baseColor = mix(baseColor, u_palette4_float3, mask4);

  float metallic = getMetallic(metallic_tex, inputs.sparse_coord);
  float specularLevel = getSpecularLevel(specularlevel_tex, inputs.sparse_coord);
  vec3 diffColor = generateDiffuseColor(baseColor, metallic);
  vec3 specColor = generateSpecularColor(specularLevel, baseColor, metallic);
  // Get detail (ambient occlusion) and global (shadow) occlusion factors
  float occlusion = getAO(inputs.sparse_coord) * getShadowFactor();
  float specOcclusion = specularOcclusionCorrection(occlusion, metallic, roughness);



  LocalVectors vectors = computeLocalFrame(inputs);

  // Feed parameters for a physically based BRDF integration
  emissiveColorOutput(pbrComputeEmissive(emissive_tex, inputs.sparse_coord));
  albedoOutput(diffColor);
  diffuseShadingOutput(occlusion * envIrradiance(vectors.normal));
  specularShadingOutput(specOcclusion * pbrComputeSpecular(vectors, specColor, roughness));
  sssCoefficientsOutput(getSSSCoefficients(inputs.sparse_coord));
}
