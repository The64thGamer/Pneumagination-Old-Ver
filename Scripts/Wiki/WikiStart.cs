using Godot;
using System;

public partial class WikiStart : Container
{
	bool canInput = true;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Modulate = new Color(1,1,1,0);
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("Toggle Wiki") && canInput)
		{
			if(Modulate.A == 0)
			{
				Modulate = new Color(1,1,1,1);
			}
			else
			{
				Modulate = new Color(1,1,1,0);
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
