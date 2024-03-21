using Godot;
using System;

public partial class EnvironmentController : WorldEnvironment
{
    [Export] Gradient fogColor;
    [Export] Gradient skyColor;

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
}
