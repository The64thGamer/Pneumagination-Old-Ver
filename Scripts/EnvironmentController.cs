using Godot;
using System;

public partial class EnvironmentController : WorldEnvironment
{
    [Export] Gradient fogColor;
    [Export] Gradient skyColor;

    const float sdfgiMaxDistance = 1400;
    const float sdfgiMaxDistancePhotoMode = 2500;
    const float fogDensity = 0.0007f;
    const float fogDensityPhotoMode = 0.0005f;


    public override void _Process(double delta)
    {
        float range = GetClampedRange(-200, 0, PlayerMovement.currentPosition.Y);
        Environment.FogLightColor = fogColor.Sample(range);
        Environment.BackgroundColor = skyColor.Sample(range);
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
