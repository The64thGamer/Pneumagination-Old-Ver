using Godot;
using System;

public partial class EnvironmentController : WorldEnvironment
{
    [Export] Gradient fogDepthColor;
    [Export] Gradient skyDepthColor;
    [Export] Gradient sunColor;
    [Export] Gradient skyColor;
    [Export] Gradient fogColor;
    [Export] Curve maxFogDistance;
    [Export] Curve minFogDistance;
    [Export] DirectionalLight3D sun;
    [Export] MeshInstance3D fogMesh;
    ShaderMaterial fogMat;

    //Statics
    public static float timeOfDay;
    public static float lengthOfDay = 1500;

    //Locals
    float exactTimeOfDay = 400;
    float maxFogRange = (WorldGen.chunkLoadingDistance * WorldGen.chunkSize) - WorldGen.chunkSize;

    //Consts
    const float sdfgiMaxDistance = 1600;
    const float sdfgiMaxDistancePhotoMode = 3000;
    const float fogDensity = 0.0007f;
    const float fogDensityPhotoMode = 0.0005f;

    public override void _Ready()
    {
        fogMat = ((ShaderMaterial)fogMesh.MaterialOverride);
    }

    public override void _Process(double delta)
    {
        float range = GetClampedRange(-200, 0, PlayerMovement.currentPosition.Y);


        exactTimeOfDay = (exactTimeOfDay + (float)delta) % lengthOfDay;
        timeOfDay = exactTimeOfDay / lengthOfDay;
        sun.LightColor = sunColor.Sample(timeOfDay);
        Environment.FogLightColor = 
        Environment.BackgroundColor = skyColor.Sample(timeOfDay) * skyDepthColor.Sample(range);
        fogMat.SetShaderParameter("fog_color", fogColor.Sample(timeOfDay) * fogDepthColor.Sample(range));
        fogMat.SetShaderParameter("fogCenterWorldPos", PlayerMovement.currentPosition);
        fogMat.SetShaderParameter("fogMaxRadius", maxFogRange * maxFogDistance.Sample(timeOfDay));
        fogMat.SetShaderParameter("fogMinRadius", maxFogRange * minFogDistance.Sample(timeOfDay));

    }

    float GetClampedRange(float lowerBound, float upperBound, float pos)
    {
        return Mathf.Clamp((pos - lowerBound) / (upperBound - lowerBound), 0, 1);
    }

    public void EnablePhotoMode()
    {
        Environment.SdfgiMaxDistance = sdfgiMaxDistancePhotoMode;
        Environment.FogDensity = fogDensityPhotoMode;
    }

    public void DisablePhotoMode()
    {
        Environment.SdfgiMaxDistance = sdfgiMaxDistance;
        Environment.FogDensity = fogDensity;
    }
}
