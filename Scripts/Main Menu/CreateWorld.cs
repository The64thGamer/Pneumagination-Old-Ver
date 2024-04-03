using Godot;
using System;

public partial class CreateWorld : Button
{
	[Export] bool joinServer;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += PressedCreateWorld;
    }

	void PressedCreateWorld()
	{
		PlayerPrefs.SetBool("Joining",joinServer);
		GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
	}
}
