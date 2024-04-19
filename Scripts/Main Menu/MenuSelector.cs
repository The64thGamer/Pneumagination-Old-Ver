using Godot;
using System;

public partial class MenuSelector : Node
{
	public override void _Ready()
	{
		SetVisible("Main Menu");
	}

	public void SetVisible(string name)
	{
		foreach(Control menu in GetChildren())
		{
			if(menu.Name == name)
			{
				menu.Visible = true;
			}
			else
			{
				menu.Visible = false;
			}
		}
	}
}
