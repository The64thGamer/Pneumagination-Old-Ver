using Godot;
using System;

public partial class SaveFileManipulator : Node
{
	[Export] Label title;
	[Export] Label description;
	string saveFolder;
	
	public void SetTitle(string name)
	{
		title.Text = name;
	}

	public void SetDescription(string desc)
	{
		description.Text = desc;
	}

	public void StartWorld()
	{
		if(GetNode<FileSaver>("/root/FileSaver").LoadNewSaveFile(saveFolder))
		{
			GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
		}
	}


	public void SetSaveFolder(string folder)
	{
		saveFolder = folder;
	}
}
