using Godot;
using System;

public partial class EnvironmentController : WorldEnvironment
{
    [Export] Gradient fogDepthColor;
    [Export] Gradient skyDepthColor;
    [Export] Gradient sunColor;
    [Export] Gradient skyColor;
    [Export] Gradient fogColor;
    [Export] DirectionalLight3D sun;
    public static float timeOfDay;
    public static float lengthOfDay = 20;

    float exactTimeOfDay = 0;

    const float sdfgiMaxDistance = 1400;
    const float sdfgiMaxDistancePhotoMode = 3000;
    const float fogDensity = 0.0007f;
    const float fogDensityPhotoMode = 0.0005f;


    public override void _Process(double delta)
    {
        float range = GetClampedRange(-200, 0, PlayerMovement.currentPosition.Y);
        Environment.FogLightColor = fogColor.Sample(timeOfDay) * fogDepthColor.Sample(range);
        Environment.BackgroundColor = skyColor.Sample(timeOfDay) * skyDepthColor.Sample(range);

        exactTimeOfDay = (exactTimeOfDay + (float)delta) % lengthOfDay;
        timeOfDay = exactTimeOfDay / lengthOfDay;
        sun.LightColor = sunColor.Sample(timeOfDay);
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
