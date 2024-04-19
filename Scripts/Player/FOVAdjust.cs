using Godot;
using System;

public partial class FOVAdjust : Camera3D
{
	public override void _Ready()
	{
		Fov = PlayerPrefs.GetFloat("FOV");
	}

	public override void _Notification(int what)
	{	
		//Application Quit
		if (what == 64646464)
		{
			Fov = PlayerPrefs.GetFloat("FOV");
		}
	}
}
