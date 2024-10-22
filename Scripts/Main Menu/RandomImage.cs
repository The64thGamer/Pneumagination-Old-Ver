using Godot;
using System;

public partial class RandomImage : TextureRect
{
	[Export] public Texture2D[] textures;
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		if(GetTree().CurrentScene.Name != "Menu")
		{
			QueueFree();
			return;
		}
		Random rng = new Random();
		Texture = textures[rng.Next() % textures.Length];
	}

}
