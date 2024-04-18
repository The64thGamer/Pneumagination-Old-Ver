using Godot;
using System;
using System.Linq;
using Chunk = WorldGen.Chunk;

public partial class FileSaver : Node
{
	const string savePath = "user://Your Precious Save Files/";
    public override void _Ready()
    {
        CreateNewSaveFile();
    }

	public void CreateNewSaveFile()
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
			GD.Print("One in a 7,007,092,303,604,023,000 chance you got the same random folder name, freak!");
		}
		GD.Print(savePath + hashFolderName);
		DirAccess.MakeDirAbsolute(savePath + hashFolderName);
	}


	public Chunk LoadChunkFromRegion()
	{
		return null;
	}

	string RandomString(int length)
	{
		Random random = new Random();
		const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
		return new string(Enumerable.Repeat(chars, length)
			.Select(s => s[random.Next(s.Length)]).ToArray());
	}
}
