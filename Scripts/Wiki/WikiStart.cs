using Godot;
using System;

public partial class WikiStart : CanvasLayer
{
	bool canInput = true;
	Input.MouseModeEnum oldMouse;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Visible = false;
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("Toggle Wiki") && canInput)
		{
			Visible = !Visible;
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

	public void StopInputs()
	{
		canInput = false;
	}

	public void StartInputs()
	{		
		canInput = true;
	}
}
