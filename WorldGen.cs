using Godot;
using System;
using System.Collections.Generic;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 1;
	List<Chunk> currentlyLoadedChunks = new List<Chunk>();
	PackedScene cubePrefab;

	//Noise
	FastNoiseLite celNoiseA = new FastNoiseLite();
	FastNoiseLite celNoiseB = new FastNoiseLite();
	FastNoiseLite os2NoiseA = new FastNoiseLite();
	FastNoiseLite os2NoiseB = new FastNoiseLite();


	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		Random rnd = new Random();
		int seed = rnd.Next();

		celNoiseA.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		celNoiseA.SetFrequency(.01f);
		celNoiseA.SetSeed(seed);
		celNoiseA.SetFractalType(FastNoiseLite.FractalType.PingPong);
		celNoiseA.SetFractalOctaves(1);
		celNoiseA.SetFractalPingPongStrength(1.5f);

		celNoiseB.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		celNoiseB.SetFrequency(.02f);
		celNoiseB.SetSeed(seed);
		celNoiseB.SetFractalType(FastNoiseLite.FractalType.FBm);
		celNoiseB.SetFractalOctaves(3);

		os2NoiseA.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		os2NoiseA.SetFrequency(0.001f);
		os2NoiseA.SetSeed(seed);
		os2NoiseA.SetFractalType(FastNoiseLite.FractalType.FBm);
		os2NoiseA.SetFractalOctaves(4);

		os2NoiseB.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		os2NoiseB.SetFrequency(0.005f);
		os2NoiseB.SetSeed(seed);
		os2NoiseB.SetFractalType(FastNoiseLite.FractalType.FBm);
		os2NoiseB.SetFractalOctaves(4);


		cubePrefab = GD.Load<PackedScene>("res://Prefabs/block.tscn");

		for (int x = -chunkRenderSize; x < chunkRenderSize; x++)
		{
			for (int y = -chunkRenderSize; y < chunkRenderSize; y++)
			{
				RenderChunk(GenerateChunk(x, y));
			}
		}
	}

	//Chunks are 128x128x128
	int GenerateChunk(int x, int z)
	{
		Chunk chunk = new Chunk();
		chunk.positionX = x;
		chunk.positionZ = z;
		chunk.brushes = new List<Brush>();

		int size = 4;

		for (int posX = 0; posX < 128 / size; posX++)
		{
			for (int posY = 0; posY < 128 / size; posY++)
			{
				for (int posZ = 0; posZ < 128 / size; posZ++)
				{
					float newX = (posX * size) + (128 * x);
					float newY = (posY * size);
					float newZ = (posZ * size) + (128 * z);

					float noiseValue = (GetClampedNoise(celNoiseA.GetNoise(newX, newY, newZ)));
					noiseValue += (GetClampedNoise(celNoiseB.GetNoise(newX, newY, newZ)));
					noiseValue *= noiseValue * 20.0f * noiseValue;
					noiseValue *= (1 - (posY * size / 128.0f)) * (GetClampedNoise(os2NoiseA.GetNoise(newX, newZ)) - 0.7f) * 25.0f;
					noiseValue = 1 - noiseValue;

					if (posY * size / 128.0f < Math.Pow(GetClampedNoise(os2NoiseA.GetNoise(newX, newZ)), 3.0f))
					{
						noiseValue = 0;
					}
					if (posY * size / 128.0f < (0.9 * GetClampedNoise(os2NoiseB.GetNoise(newX, newZ))) - 0.4f)
					{
						noiseValue = 0;
					}
					if (posY * size < 1)
					{
						noiseValue = 0;
					}
					if (noiseValue <= 0.25f)
					{
						chunk.brushes.Add(CreateBrush(new Vector3(posX * size, posY * size, posZ * size), new Vector3(size, size, size)));
					}
				}
			}
		}
		currentlyLoadedChunks.Add(chunk);
		return currentlyLoadedChunks.Count - 1;
	}

	void RenderChunk(int index)
	{
		Node3D chunk = new Node3D();
		AddChild(chunk);
		chunk.GlobalPosition = new Vector3(currentlyLoadedChunks[index].positionX * 128, 0, currentlyLoadedChunks[index].positionZ * 128);

		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// C# arrays cannot be resized or expanded, so use Lists to create geometry.
		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		for (int e = 0; e < currentlyLoadedChunks[index].brushes.Count; e++)
		{
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			int maxZ = int.MinValue;
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int minZ = int.MaxValue;

			for (int i = 0; i < currentlyLoadedChunks[index].brushes[e].vertices.Length / 3; i++)
			{
				maxX = Math.Max(maxX, currentlyLoadedChunks[index].brushes[e].vertices[i, 0]);
				maxY = Math.Max(maxY, currentlyLoadedChunks[index].brushes[e].vertices[i, 1]);
				maxZ = Math.Max(maxZ, currentlyLoadedChunks[index].brushes[e].vertices[i, 2]);
				minX = Math.Min(minX, currentlyLoadedChunks[index].brushes[e].vertices[i, 0]);
				minY = Math.Min(minY, currentlyLoadedChunks[index].brushes[e].vertices[i, 1]);
				minZ = Math.Min(minZ, currentlyLoadedChunks[index].brushes[e].vertices[i, 2]);
			}

			Vector3 origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

			int oldVertCount = verts.Count;


			for (int i = 0; i < currentlyLoadedChunks[index].brushes[e].vertices.Length / 3; i++)
			{
				Vector3 vert = new Vector3(currentlyLoadedChunks[index].brushes[e].vertices[i, 0], currentlyLoadedChunks[index].brushes[e].vertices[i, 1], currentlyLoadedChunks[index].brushes[e].vertices[i, 2]);
				verts.Add(vert);
				normals.Add(((vert - origin)).Normalized());
				uvs.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < currentlyLoadedChunks[index].brushes[e].indicies.Length; i++)
			{
				indices.Add(oldVertCount + currentlyLoadedChunks[index].brushes[e].indicies[i]);
			}
		}
		// Convert Lists to arrays and assign to surface array
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = uvs.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();

		var arrMesh = new ArrayMesh();
		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);


		MeshInstance3D meshObject = new MeshInstance3D();
		meshObject.Mesh = arrMesh;
		meshObject.Mesh.SurfaceSetMaterial(0, mat);

		chunk.AddChild(meshObject);
	}

	Brush CreateBrush(Vector3 pos, Vector3 size)
	{
		Brush brush = new Brush();
		brush.vertices = new byte[8, 3];

		brush.vertices[0, 0] = (byte)(pos.X);
		brush.vertices[0, 1] = (byte)(pos.Y);
		brush.vertices[0, 2] = (byte)(pos.Z);

		brush.vertices[1, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[1, 1] = (byte)(pos.Y);
		brush.vertices[1, 2] = (byte)(pos.Z);

		brush.vertices[2, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[2, 1] = (byte)(pos.Y);
		brush.vertices[2, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[3, 0] = (byte)(pos.X);
		brush.vertices[3, 1] = (byte)(pos.Y);
		brush.vertices[3, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[4, 0] = (byte)(pos.X);
		brush.vertices[4, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[4, 2] = (byte)(pos.Z);

		brush.vertices[5, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[5, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[5, 2] = (byte)(pos.Z);

		brush.vertices[6, 0] = (byte)((1 * size.X) + pos.X);
		brush.vertices[6, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[6, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.vertices[7, 0] = (byte)(pos.X);
		brush.vertices[7, 1] = (byte)((1 * size.Y) + pos.Y);
		brush.vertices[7, 2] = (byte)((1 * size.Z) + pos.Z);

		brush.indicies = new byte[]
		{
                2, 1, 0,
                0, 3, 2,

				6, 2, 3,
				3, 7, 6,

				5, 6, 7,
				7, 4, 5,

				1, 5, 4,
				4, 0, 1,

				7, 3, 0,
				0, 4, 7,

				6, 5, 1,
				1, 2, 6
		};

		return brush;
	}

	float GetClampedNoise(float noise)
	{
		return (noise + 1.0f) / 2.0f;
	}

	public struct Chunk
	{
		public int positionX;
		public int positionZ;
		public List<Brush> brushes;
	}

	public struct Brush
	{
		public byte[,] vertices;
		public byte[] indicies;
		public uint[] textures;
	}
}
