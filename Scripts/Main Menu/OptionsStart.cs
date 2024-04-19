using Godot;
using System;

public partial class OptionsStart : CanvasLayer
{
	bool canInput = true;

	public override void _Ready()
	{
		Visible = false;
	}

	public override void _Process(double delta)
	{
		if (Input.IsActionJustPressed("Pause") && Visible)
		{
			Visible = false;
        }
	}

	public void ToggleOptions()
	{
		Visible = !Visible;
	}
}
