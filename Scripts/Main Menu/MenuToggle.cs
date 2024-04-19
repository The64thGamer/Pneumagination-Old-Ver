using Godot;
using System;

public partial class MenuToggle : CanvasLayer
{
	Input.MouseModeEnum oldMouse;

	public override void _Ready()
	{
		SetMenu(false);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause"))
		{
			SetMenu(!Visible);
        }
	}

	void SetMenu(bool set)
	{
		if(GetTree().CurrentScene.Name == "Menu")
		{
			return;
		}
		Visible = set;
		if(Visible)
		{
			oldMouse = Input.MouseMode;
			Input.MouseMode = Input.MouseModeEnum.Visible;
		}
		else
		{
			Input.MouseMode = oldMouse;
		}
	}
}
