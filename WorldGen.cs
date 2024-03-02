using Godot;
using System;
using System.Collections.Generic;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 3;
	List<Chunk> currentlyLoadedChunks = new List<Chunk>();

	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		for (int x = -chunkRenderSize; x < chunkRenderSize; x++)
		{
			for (int y = -chunkRenderSize; y < chunkRenderSize; y++)
			{
				RenderChunk(GenerateChunk(x,y));
			}
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	//Chunks are 128x128x128
	int GenerateChunk(int x, int y)
	{
		Chunk chunk = new Chunk();
		chunk.positionX = x;
		chunk.positionY = y;
		chunk.brushes = new List<Brush>();

		int size = 4;
		for (int i = 0; i < 200; i++)
		{
			int randomX = (int)(GD.Randi() % 128 - size);
			int randomY = (int)GD.Randi() % 128- size;
			int randomZ = (int)GD.Randi() % 128 - size;

			chunk.brushes.Add(CreateBrush(new Vector3(randomX,randomY,randomZ),new Vector3(size, size, size)));
		}

		currentlyLoadedChunks.Add(chunk);
		return currentlyLoadedChunks.Count - 1;
	}

	void RenderChunk(int index)
	{
		Node3D chunk = new Node3D();
		chunk.GlobalPosition = new Vector3(currentlyLoadedChunks[index].positionX * 128,0, currentlyLoadedChunks[index].positionY * 128);
		AddChild(chunk);
		for (int i = 0; i < currentlyLoadedChunks[index].brushes.Count; i++)
		{
			chunk.AddChild(RenderBrush(currentlyLoadedChunks[index].brushes[i]));
		}
	}

	Brush CreateBrush(Vector3 pos, Vector3 size)
	{
		Brush brush = new Brush();
		brush.vertices = new byte[8, 3];

		brush.vertices[0, 0] = (byte)(0 + pos.X);
		brush.vertices[0, 1] = (byte)(0 + pos.Y);
		brush.vertices[0, 2] = (byte)(0 + pos.Z);

		brush.vertices[1, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[1, 1] = (byte)(0 + pos.Y);
		brush.vertices[1, 2] = (byte)(0 + pos.Z);

		brush.vertices[2, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[2, 1] = (byte)(0 + pos.Y);
		brush.vertices[2, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[3, 0] = (byte)(0 + pos.X);
		brush.vertices[3, 1] = (byte)(0 + pos.Y);
		brush.vertices[3, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[4, 0] = (byte)(0 + pos.X);
		brush.vertices[4, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[4, 2] = (byte)(0 + pos.Z);

		brush.vertices[5, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[5, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[5, 2] = (byte)(0 + pos.Z);

		brush.vertices[6, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[6, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[6, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[7, 0] = (byte)(0 + pos.X);
		brush.vertices[7, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[7, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.indicies = new byte[]
		{
				// Bottom face
				(byte)(0), (byte)(1), (byte)(2),
				(byte)(2), (byte)(3), (byte)(0),

				// Front face
				(byte)(3), (byte)(2), (byte)(6),
				(byte)(6), (byte)(7), (byte)(3),

				// Right face
				(byte)(7), (byte)(6), (byte)(5),
				(byte)(5), (byte)(4), (byte)(7),

				// Back face
				(byte)(4), (byte)(5), (byte)(1),
				(byte)(1), (byte)(0), (byte)(4),

				// Left face
				(byte)(0), (byte)(3), (byte)(7),
				(byte)(7), (byte)(4), (byte)(0),

				// Top face
				(byte)(1), (byte)(5), (byte)(6),
				(byte)(6), (byte)(2), (byte)(1)
		};

		return brush;
	}

	MeshInstance3D RenderBrush(Brush brush)
	{
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// C# arrays cannot be resized or expanded, so use Lists to create geometry.
		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		int maxX = int.MinValue;
		int maxY = int.MinValue;
		int maxZ = int.MinValue;
		int minX = int.MaxValue;
		int minY = int.MaxValue;
		int minZ = int.MaxValue;

		for (int i = 0; i < brush.vertices.Length / 3; i++)
		{
			maxX = Math.Max(maxX, brush.vertices[i, 0]);
			maxY = Math.Max(maxY, brush.vertices[i, 1]);
			maxZ = Math.Max(maxZ, brush.vertices[i, 2]);
			minX = Math.Min(minX, brush.vertices[i, 0]);
			minY = Math.Min(minY, brush.vertices[i, 1]);
			minZ = Math.Min(minZ, brush.vertices[i, 2]);
		}

		Vector3 origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

		for (int i = 0; i < brush.vertices.Length / 3; i++)
		{
			Vector3 vert = new Vector3(brush.vertices[i, 0], brush.vertices[i, 1], brush.vertices[i, 2]);
			verts.Add(vert);
			normals.Add(((vert - origin)).Normalized()*-1);
			uvs.Add(new Vector2(0,0));
		}

		for (int i = 0; i < brush.indicies.Length; i++)
		{
			indices.Add(brush.indicies[i]);
		}

		// Convert Lists to arrays and assign to surface array
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		var arrMesh = new ArrayMesh();
		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
		arrMesh.SurfaceSetMaterial(0, mat);
		MeshInstance3D meshObject = new MeshInstance3D();
		meshObject.Mesh = arrMesh;
		return meshObject;
	}

	public struct Chunk
	{
		public int positionX;
		public int positionY;
		public List<Brush> brushes;
	}

	public struct Brush
	{
		public byte[,] vertices;
		public byte[] indicies;
		public uint[] textures;
	}
}
