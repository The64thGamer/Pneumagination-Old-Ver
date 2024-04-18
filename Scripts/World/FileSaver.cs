using Godot;
using System;
using System.Linq;
using Chunk = WorldGen.Chunk;
using Console = media.Laura.SofiaConsole.Console;

public partial class FileSaver : Node
{
	const string savePath = "user://Your Precious Save Files/";
	const string worldSaveDataFile = "World Save Data";

	public void CreateNewSaveFile(Godot.Collections.Dictionary<string, Variant> data)
	{
		//Ensure saves folder exists
		if(!DirAccess.DirExistsAbsolute(savePath))
		{
			DirAccess.MakeDirAbsolute(savePath);
		}

		//Create random folder name
		string hashFolderName;
		while(true)
		{
			hashFolderName = RandomString(32);
			if(!DirAccess.DirExistsAbsolute(savePath + hashFolderName))
			{
				break;
			}
			Console.Instance.Print("One in a 218,169,540,588,403,680 chance you got the same random folder name, freak!");
		}
		GD.Print(savePath + hashFolderName);
		DirAccess.MakeDirAbsolute(savePath + hashFolderName);

		//Create 
		FileAccess.Open(savePath + hashFolderName + "/" + worldSaveDataFile, FileAccess.ModeFlags.Write).StoreLine(Json.Stringify(data));
	}


	public Chunk LoadChunkFromRegion()
	{
		return null;
	}

	#region pure functions
	string RandomString(int length)
	{
		Random random = new Random();
		const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}

	#endregion
}
