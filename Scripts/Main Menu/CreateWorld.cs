using Godot;
using System;

public partial class CreateWorld : Button
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += PressedCreateWorld;

    }

	void PressedCreateWorld()
	{
		GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
	}
}
