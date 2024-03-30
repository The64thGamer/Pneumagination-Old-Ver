using Godot;
using System;

public partial class MenuSeed : LineEdit
{
	public override void _Ready()
	{
		Random rnd  = new Random();
		Text = rnd.Next().ToString();
	}
}
