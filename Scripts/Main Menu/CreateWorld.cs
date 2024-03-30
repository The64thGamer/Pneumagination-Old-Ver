using Godot;
using System;

public partial class CreateWorld : Button
{
	[Export] LineEdit seed;

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Pressed += PressedCreateWorld;

    }

	void PressedCreateWorld()
	{
		PlayerPrefs.SetInt("Seed", GetDeterministicHashCode(seed.Text));
		GetTree().ChangeSceneToFile("res://Scenes/World.tscn");
	}

	int GetDeterministicHashCode(string str)
	{
        int hash1 = (5381 << 16) + 5381;
        int hash2 = hash1;

        for (int i = 0; i < str.Length; i += 2)
        {
            hash1 = ((hash1 << 5) + hash1) ^ str[i];
            if (i == str.Length - 1)
                break;
            hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
        }

        return hash1 + (hash2 * 1566083941);
    }
}
