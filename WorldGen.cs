using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 7;
	PackedScene cubePrefab;

	List<Thread> ongoingChunkThreads =	new List<Thread>();
	List<ChunkRenderData> ongoingChunkRenderData = new List<ChunkRenderData>();

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
		if (chunkRenderSize <= 0)
		{
			RenderChunk(0, 0);
		}
		else
		{
			List<Vector2> chunks = new List<Vector2>();
			for (int x = -chunkRenderSize; x <= chunkRenderSize; x++)
			{
				for (int y = -chunkRenderSize; y <= chunkRenderSize; y++)
				{
					chunks.Add(new Vector2(x, y));
				}
			}

			for (int i = 0; i < chunks.Count; i++)
			{
				RenderChunk((int)chunks[i].X, (int)chunks[i].Y);
			}
		}
	}

	public override void _Process(double delta)
	{
		CheckForAnyPendingFinishedChunks();
	}

	void CheckForAnyPendingFinishedChunks()
	{
		for (int i = 0; i < ongoingChunkThreads.Count; i++)
		{
			if (!ongoingChunkThreads[i].IsAlive)
			{
				for (int e = 0; e < ongoingChunkRenderData.Count; e++)
				{
					if (ongoingChunkRenderData[e] != null)
					{
						AddChild(ongoingChunkRenderData[e].chunkNode);
						ongoingChunkRenderData[e].chunkNode.GlobalPosition = ongoingChunkRenderData[e].position;
						ongoingChunkRenderData[e].chunkNode.AddChild(ongoingChunkRenderData[e].meshNode);
					}
				}
				ongoingChunkThreads.RemoveAt(i);

				//Really cheap way to deal with two unsynched Lists<> but probably not good if someone
				//managed to keep rendering chunks constantly and this filled up RAM. Fix later when its monetarily viable.
				if(ongoingChunkThreads.Count == 0 )
				{
					ongoingChunkRenderData = new List<ChunkRenderData>();
				}
				break;
			}
		}
	}

	void RenderChunk(int x, int z)
	{
		ongoingChunkRenderData.Add(null);
		Thread renderThread = new Thread(() => RenderChunkThread(x,z, ongoingChunkRenderData[ongoingChunkRenderData.Count - 1]));
		renderThread.Start();
		ongoingChunkThreads.Add(renderThread);
	}

	async void RenderChunkThread(int x, int z, ChunkRenderData returnChunkData)
	{
		Task<Chunk> generateTask = GenerateChunk(x,z);
		Chunk chunk = await generateTask;

		Task<ChunkRenderData> meshTask = GetChunkMesh(chunk);
		returnChunkData = await meshTask; 
	}

	async Task<Chunk> GenerateChunk(int x, int z)
	{
		Chunk chunk = await Task.Run(() => GenerateChunkThread(x,z));
		return chunk;
	}


	//Chunks are 128x128x128
	Chunk GenerateChunkThread(int x, int z)
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
		return chunk;
	}

	async Task<ChunkRenderData> GetChunkMesh(Chunk chunkData)
	{
		Node3D chunk = new Node3D();
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// C# arrays cannot be resized or expanded, so use Lists to create geometry.
		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		for (int e = 0; e < chunkData.brushes.Count; e++)
		{
			int maxX = int.MinValue;
			int maxY = int.MinValue;
			int maxZ = int.MinValue;
			int minX = int.MaxValue;
			int minY = int.MaxValue;
			int minZ = int.MaxValue;

			for (int i = 0; i < chunkData.brushes[e].vertices.Length / 3; i++)
			{
				maxX = Math.Max(maxX, chunkData.brushes[e].vertices[i, 0]);
				maxY = Math.Max(maxY, chunkData.brushes[e].vertices[i, 1]);
				maxZ = Math.Max(maxZ, chunkData.brushes[e].vertices[i, 2]);
				minX = Math.Min(minX, chunkData.brushes[e].vertices[i, 0]);
				minY = Math.Min(minY, chunkData.brushes[e].vertices[i, 1]);
				minZ = Math.Min(minZ, chunkData.brushes[e].vertices[i, 2]);
			}

			Vector3 origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

			int oldVertCount = verts.Count;


			for (int i = 0; i < chunkData.brushes[e].vertices.Length / 3; i++)
			{
				Vector3 vert = new Vector3(chunkData.brushes[e].vertices[i, 0], chunkData.brushes[e].vertices[i, 1], chunkData.brushes[e].vertices[i, 2]);
				verts.Add(vert);
				normals.Add(((vert - origin)).Normalized());
				uvs.Add(new Vector2(0, 0));
			}

			for (int i = 0; i < chunkData.brushes[e].indicies.Length; i++)
			{
				indices.Add(oldVertCount + chunkData.brushes[e].indicies[i]);
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


		return new ChunkRenderData() { chunkNode = chunk, meshNode = meshObject, position = new Vector3(chunkData.positionX * 128, 0, chunkData.positionZ * 128) };
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

	public class Chunk
	{
		public int positionX;
		public int positionZ;
		public List<Brush> brushes;
	}

	public class Brush
	{
		public byte[,] vertices;
		public byte[] indicies;
		public uint[] textures;
	}

	public class ChunkRenderData
	{
		public Node3D chunkNode;
		public MeshInstance3D meshNode;
		public Vector3 position;
	}
}
