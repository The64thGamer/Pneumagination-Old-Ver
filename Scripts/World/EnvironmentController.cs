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
    ShaderMaterial fogMat;
    
    ServerClient server;

    //Statics
    public static float timeOfDay;
    public static float lengthOfDay = 1500;

    //Locals
    float exactTimeOfDay = 400;
    float maxFogRange = (WorldGen.chunkLoadingDistance * WorldGen.chunkSize) - WorldGen.chunkSize;

    //Consts
    const float sdfgiMaxDistance = 1600;
    const float sdfgiMaxDistancePhotoMode = 2400;

    public override void _Ready()
    {
		server = GetTree().Root.GetNode("World/Server") as ServerClient;
    }
    public override void _Process(double delta)
    {
        float range = GetClampedRange(-200, 0, server.GetMainPlayer().GlobalPosition.Y);

        if(fogMat == null)
        {
            if(server.GetMainPlayer() == null)
            {
                return;
            }
            fogMat = (ShaderMaterial)(server.GetMainPlayer().GetNode("Player Head/Player Camera/Fog Mesh") as MeshInstance3D).MaterialOverride;
        }

        exactTimeOfDay = (exactTimeOfDay + (float)delta) % lengthOfDay;
        timeOfDay = exactTimeOfDay / lengthOfDay;
        sun.LightColor = sunColor.Sample(timeOfDay);
        Environment.FogLightColor = 
        Environment.BackgroundColor = skyColor.Sample(timeOfDay) * skyDepthColor.Sample(range);
        fogMat.SetShaderParameter("fog_color", fogColor.Sample(timeOfDay) * fogDepthColor.Sample(range));
        fogMat.SetShaderParameter("fogCenterWorldPos", server.GetMainPlayer().GlobalPosition);
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
    }

    public void DisablePhotoMode()
    {
        Environment.SdfgiMaxDistance = sdfgiMaxDistance;
    }
}
