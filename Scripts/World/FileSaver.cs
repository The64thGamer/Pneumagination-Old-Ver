using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Console = media.Laura.SofiaConsole.Console;

public partial class FileSaver : Node
{
	//Locals
	Godot.Collections.Dictionary<string, Variant> loadedWorldData;

	List<Region> loadedRegions = new List<Region>();
	string loadedFolderPath;


	//Consts
	public const string savePath = "user://Your Precious Save Files/";
	public const string worldSaveDataFile = "World Save Data";
	public const string regionPath = "/Chunks/Region ";
	public const string regionExtension = ".tres";

	public void SaveAllChunks(Chunk[] data)
	{
		Region loadedRegion;
		foreach(Chunk chunk in data)
		{
			loadedRegion = FindRegionFile(
				chunk.positionX / Region.regionSize,
				chunk.positionY / Region.regionSize,
				chunk.positionZ / Region.regionSize
			);

			Vector3 pos = new Vector3(
				chunk.positionX % Region.regionSize,
				chunk.positionY % Region.regionSize,
				chunk.positionZ % Region.regionSize);

			if(loadedRegion.chunks.TryGetValue(pos,out Chunk oldChunk))
			{
				oldChunk = chunk;
			}
			else
			{
				loadedRegion.chunks.Add(pos,chunk);
			}

			ResourceSaver.Save(loadedRegion, savePath + loadedFolderPath + regionPath +
				chunk.positionX / Region.regionSize + " " + 
				chunk.positionY / Region.regionSize + " " + 
				chunk.positionZ / Region.regionSize + 
				regionExtension
			);
		}
	}

	public void CreateNewSaveFile(Godot.Collections.Dictionary<string, Variant> data, bool alsoLoadFile)
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
			Console.Instance.Print("One in a 218,169,540,588,403,680 chance you got the same random folder name, freak!", Console.PrintType.Error);
		}
		DirAccess.MakeDirAbsolute(savePath + hashFolderName);

		//Create 
		FileAccess.Open(savePath + hashFolderName + "/" + worldSaveDataFile, FileAccess.ModeFlags.Write).StoreString(Json.Stringify(data));
		if(alsoLoadFile)
		{
			loadedWorldData = data;
			loadedFolderPath = hashFolderName;
		}
		Console.Instance.Print("Saved World File to :" + savePath + hashFolderName, Console.PrintType.Success);
	}

	//folderPath is just the random string folder name.
	public bool LoadNewSaveFile(string folderPath)
	{
		Godot.Collections.Dictionary<string, Variant> data = AcquireSaveData(folderPath);
		if(data == null)
		{
			return false;
		}
		loadedWorldData = data;
		loadedFolderPath = folderPath;
		Console.Instance.Print("Save File Loaded :" + savePath + folderPath, Console.PrintType.Success);
		return true;
	}

	public Godot.Collections.Dictionary<string, Variant> AcquireSaveData(string folderPath)
	{
		if(!DirAccess.DirExistsAbsolute(savePath))
		{
			Console.Instance.Print("No parent save folder found", Console.PrintType.Error);
			return null;
		}
		if (!DirAccess.DirExistsAbsolute(savePath + folderPath))
		{
			Console.Instance.Print("Given save folder name not found", Console.PrintType.Error);
			return null;
		}
		if (!FileAccess.FileExists(savePath + folderPath + "/" + worldSaveDataFile))
		{
			Console.Instance.Print("Save folder found but no World Data file found (?????)", Console.PrintType.Error);
			return null;
		}
    	FileAccess saveGame = FileAccess.Open(savePath + folderPath + "/" + worldSaveDataFile, FileAccess.ModeFlags.Read);
        Json json = new Json();
        Error parseResult = json.Parse(saveGame.GetLine());
        if (parseResult != Error.Ok)
        {
            Console.Instance.Print($"JSON Parse Error: {json.GetErrorMessage()} at line {json.GetErrorLine()}", Console.PrintType.Error);
			return null;
        }

        return new Godot.Collections.Dictionary<string, Variant>((Godot.Collections.Dictionary)json.Data);
	}


	public Chunk LoadChunkFromRegion(int chunkX, int chunkY, int chunkZ)
	{
		Region loadedRegion = FindRegionFile(
			chunkX / Region.regionSize,
			chunkY / Region.regionSize,
			chunkZ / Region.regionSize
		);

		//Load Chunks
		if(loadedRegion == null)
		{
			GD.Print("Region was null.");
			return null;
		}

		if(loadedRegion.chunks.TryGetValue(new Vector3(
				chunkX % Region.regionSize,
				chunkY % Region.regionSize,
				chunkZ % Region.regionSize
			),out Chunk oldChunk))
		{
			return oldChunk;
		}

		return null;
	}

	Region FindRegionFile(int regX, int regY, int regZ)
	{
		Region loadedRegion = null;

		//Check loaded regions
		foreach(Region reg in loadedRegions)
		{
			if(reg.positionX == regX && reg.positionY == regY && reg.positionZ == regZ)
			{
				return reg;
			}
		}

		string finalPath = savePath + loadedFolderPath + regionPath + regX + " " + regY + " " + regZ + regionExtension;

		if(!DirAccess.DirExistsAbsolute(savePath + loadedFolderPath + "/Chunks"))
		{
			DirAccess.MakeDirAbsolute(savePath + loadedFolderPath + "/Chunks");
		}

		//Check filesystem
		if(loadedRegion == null)
		{			
			if(FileAccess.FileExists(finalPath))
			{
				loadedRegion = ResourceLoader.Load<Region>(finalPath);
			}
		}

		//Go ahead and create region
		if(loadedRegion == null)
		{
			loadedRegion = new Region(regX,regY,regZ);
			Error e = ResourceSaver.Save(loadedRegion, finalPath);
			if(e != Error.Ok)
			{
				GD.PrintErr("SAVING ERROR: " + e);
			}
		}
		return loadedRegion;
	}

	public string GetSeed()
	{
		if(loadedWorldData.TryGetValue("World Seed",out Variant value))
		{
			return value.ToString();
		}
		return "";
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