using Godot;
using System;

public partial class Inventory : Control
{	
    public static int[] inventory;

	bool canInput = true;
	public static bool inventoryEnabled;
	Input.MouseModeEnum oldMouse;

	public override void _Ready()
	{
		Visible = false;
	}

	public override void _Process(double delta)
	{
		if(Input.IsActionJustPressed("Inventory") && canInput)
		{
			ToggleWiki();
		}
		if (Input.IsActionJustPressed("Pause") && Visible)
		{
			ToggleWiki();
		}
	}

	public void ToggleWiki()
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
		inventoryEnabled = Visible;
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

