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
		if(!joinServer)
		GetNode<FileSaver>("/root/FileSaver").CreateNewSaveFile(
			new Godot.Collections.Dictionary<string, Variant>(){
				{ "World Name", PlayerPrefs.GetString("World Name")},
				{ "World Author", PlayerPrefs.GetString("Name")},
				{ "World Created Time UTC", DateTime.Now.ToUniversalTime().ToString(@"MM\/dd\/yyyy h\:mm tt")},
				{ "World Created Version", Tr("CURRENT_VERSION")},
				{ "World Seed", PlayerPrefs.GetString("Seed")},}
			, true
		);
		GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
	}
}
