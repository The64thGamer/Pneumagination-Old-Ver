using Godot;
using System;

public partial class WorldGen : Node3D
{
	[Export] float bruh;
	[Export] List<Chunk> currentlyLoadedChunks = new List<Chunk>();
	
	const PackedByteArrayint[]
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
	
	//Chunks are 128x128x128
	void GenerateChunk(int x, int y)
	{
		Chunk chunk = new Chunk;
		chunk.positionX = x;
		chunk.positionY = y;

		for (int i = 0; i < 20; i++)
		{
			int randomX = GD.Randi() % 128;
			int randomY = GD.Randi() % 128;
			int randomZ = GD.Randi() % 128;
			Brush brush = new Brush;
			brush.vertices = new BrushVertex[8, 3];

			brush.vertices[0, 0] = Convert.ToByte(0);
			brush.vertices[0, 1] = Convert.ToByte(0);
			brush.vertices[0, 2] = Convert.ToByte(0);

			brush.vertices[1, 0] = Convert.ToByte(1);
			brush.vertices[1, 1] = Convert.ToByte(0);
			brush.vertices[1, 2] = Convert.ToByte(0);

			brush.vertices[2, 0] = Convert.ToByte(1);
			brush.vertices[2, 1] = Convert.ToByte(0);
			brush.vertices[2, 2] = Convert.ToByte(1);

			brush.vertices[3, 0] = Convert.ToByte(0);
			brush.vertices[3, 1] = Convert.ToByte(0);
			brush.vertices[3, 2] = Convert.ToByte(1);

			brush.vertices[4, 0] = Convert.ToByte(0);
			brush.vertices[4, 1] = Convert.ToByte(1);
			brush.vertices[4, 2] = Convert.ToByte(0);

			brush.vertices[5, 0] = Convert.ToByte(1);
			brush.vertices[5, 1] = Convert.ToByte(1);
			brush.vertices[5, 2] = Convert.ToByte(0);

			brush.vertices[6, 0] = Convert.ToByte(1);
			brush.vertices[6, 1] = Convert.ToByte(1);
			brush.vertices[6, 2] = Convert.ToByte(1);

			brush.vertices[7, 0] = Convert.ToByte(0);
			brush.vertices[7, 1] = Convert.ToByte(1);
			brush.vertices[7, 2] = Convert.ToByte(1);

			brush.indicies = new PackedByteArray[]
			{
				// Bottom face
				Convert.ToByte(0), Convert.ToByte(1), Convert.ToByte(2),
				Convert.ToByte(2), Convert.ToByte(3), Convert.ToByte(0),

				// Front face
				Convert.ToByte(3), Convert.ToByte(2), Convert.ToByte(6),
				Convert.ToByte(6), Convert.ToByte(7), Convert.ToByte(3),

				// Right face
				Convert.ToByte(7), Convert.ToByte(6), Convert.ToByte(5),
				Convert.ToByte(5), Convert.ToByte(4), Convert.ToByte(7),

				// Back face
				Convert.ToByte(4), Convert.ToByte(5), Convert.ToByte(1),
				Convert.ToByte(1), Convert.ToByte(0), Convert.ToByte(4),

				// Left face
				Convert.ToByte(0), Convert.ToByte(3), Convert.ToByte(7),
				Convert.ToByte(7), Convert.ToByte(4), Convert.ToByte(0),

				// Top face
				Convert.ToByte(1), Convert.ToByte(5), Convert.ToByte(6),
				Convert.ToByte(6), Convert.ToByte(2), Convert.ToByte(1)
			};

			chunk.brushes.add(brush);
		}	

		currentlyLoadedChunks.add(chunk)
	}
	
	public struct Chunk
	{
		int positionX;
		int positionY;
		List<Brush> brushes = new List<Brush>();
	}
	
	public struct Brush
	{
		PackedByteArray[,] verticies;
		PackedByteArray[] indicies;
		uint[] textures;
	}
}
