using Godot;
using System;

public partial class SaveFileDisplay : Control
{
	[Export] PackedScene fileNode;
	[Export] VBoxContainer container;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		VisibilityChanged += SetSaveFiles;
	}

	void SetSaveFiles()
	{
		GD.Print("????");
		foreach(Node n in container.GetChildren())
		{
			n.QueueFree();
		}

		FileSaver saver = GetNode<FileSaver>("/root/FileSaver");

		string[] dirs = DirAccess.GetDirectoriesAt(FileSaver.savePath);

		foreach(string path in dirs)
		{
			Node currentNode = fileNode.Instantiate();
			container.AddChild(currentNode);
			SaveFileManipulator sfm = currentNode as SaveFileManipulator;
			sfm.SetSaveFolder(path);
			Godot.Collections.Dictionary<string, Variant> data = saver.AcquireSaveData(path);
			bool check = true;
			if(data != null)
			{
				Variant value;
				if(data.TryGetValue("World Name", out value))
				{
					sfm.SetTitle(value.ToString());
				}
				else
				{
					check = false;
				}

				if(data.TryGetValue("World Created Time UTC", out value))
				{
					sfm.SetDescription("Created " + value.ToString());
				}
				else
				{
					check = false;
				}
			}
			else
			{
				check = false;
			}

			if(!check)
			{
				sfm.SetTitle(Tr("ERROR_CORRUPTED_WORLD"));
				sfm.SetDescription(Tr("ERROR_CORRUPTED_WORLD_TIP"));
			}
		}
	}
}
