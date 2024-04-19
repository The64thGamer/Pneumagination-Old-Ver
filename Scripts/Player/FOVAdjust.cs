using Godot;
using System;

public partial class FOVAdjust : Camera3D
{
	public override void _Ready()
	{
		Fov = PlayerPrefs.GetFloat("FOV");
	}

}
