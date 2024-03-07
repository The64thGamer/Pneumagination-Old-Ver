using Godot;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 9;
	PackedScene cubePrefab;
	int totalChunksRendered = 0;

	List<ChunkRenderData> ongoingChunkRenderData = new List<ChunkRenderData>();

	//Noise
	FastNoiseLite celNoiseA = new FastNoiseLite();
	FastNoiseLite celNoiseB = new FastNoiseLite();
	FastNoiseLite os2NoiseA = new FastNoiseLite();
	FastNoiseLite os2NoiseB = new FastNoiseLite();

	const int bigBlockSize = 3;
	const int chunkSize = 126;

	//TODO: add crafting system where you get shako for 2 metal
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
		if (ongoingChunkRenderData.Count == 0)
		{
			return;
		}

		bool check = false;
		for (int e = 0; e < ongoingChunkRenderData.Count; e++)
		{
			if (ongoingChunkRenderData[e].state == ChunkRenderDataState.ready)
			{
				AddChild(ongoingChunkRenderData[e].chunkNode);
				ongoingChunkRenderData[e].chunkNode.AddChild(ongoingChunkRenderData[e].meshNode);
				ongoingChunkRenderData[e].chunkNode.GlobalPosition = ongoingChunkRenderData[e].position;
				ongoingChunkRenderData[e].state = ChunkRenderDataState.garbageCollector;
				GD.Print("Finishing Chunk (ID " + ongoingChunkRenderData[e].id + ")");
			}
			if (ongoingChunkRenderData[e].state == ChunkRenderDataState.running)
			{
				check = true;
			}
		}

		if (!check)
		{
			GD.Print("All Chunks Done Rendering");
			ongoingChunkRenderData = new List<ChunkRenderData>();
		}
	}

	void RenderChunk(int x, int z)
	{
		totalChunksRendered++;
		int id = totalChunksRendered;
		ongoingChunkRenderData.Add(new ChunkRenderData() { state = ChunkRenderDataState.running, id = id });
		GD.Print("Generating Chunk (ID " + totalChunksRendered + "): X = " + x + " Z = " + z);

		Task.Run(async () =>
		{
			Chunk chunk = await GenerateChunk(x, z);
			ChunkRenderData chunkData = await GetChunkMesh(chunk, id);
			bool check = false;
			for (int i = 0; i < ongoingChunkRenderData.Count; i++)
			{
				if (ongoingChunkRenderData[i].id == id)
				{
					ongoingChunkRenderData[i] = chunkData;
					check = true;
					break;
				}
			}

			if (!check)
			{
				GD.Print("Chunk Mesh Could Not Be Finalized (ID " + id + ")");
			}

		});
	}


	//Chunks are 126x126x126
	async Task<Chunk> GenerateChunk(int x, int z)

	{
		Chunk chunk = new Chunk();
		chunk.positionX = x;
		chunk.positionZ = z;
		chunk.brushes = new List<Brush>();

		for (int posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (int posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (int posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					float newX = (posX * bigBlockSize) + (chunkSize * x);
					float newY = (posY * bigBlockSize);
					float newZ = (posZ * bigBlockSize) + (chunkSize * z);

					float noiseValue = (GetClampedNoise(celNoiseA.GetNoise(newX, newY, newZ)));
					noiseValue += (GetClampedNoise(celNoiseB.GetNoise(newX, newY, newZ)));
					noiseValue *= noiseValue * 20.0f * noiseValue;
					noiseValue *= (1 - (posY * bigBlockSize / (float)chunkSize)) * (GetClampedNoise(os2NoiseA.GetNoise(newX, newZ)) - 0.7f) * 25.0f;
					noiseValue = 1 - noiseValue;

					if (posY * bigBlockSize / (float)chunkSize < Math.Pow(GetClampedNoise(os2NoiseA.GetNoise(newX, newZ)), 3.0f))
					{
						noiseValue = 0;
					}
					if (posY * bigBlockSize / (float)chunkSize < (0.9 * GetClampedNoise(os2NoiseB.GetNoise(newX, newZ))) - 0.4f)
					{
						noiseValue = 0;
					}
					if (posY * bigBlockSize < 1)
					{
						noiseValue = 0;
					}
					if (noiseValue <= 0.25f)
					{
						chunk.brushes.Add(CreateBrush(new Vector3(posX * bigBlockSize, posY * bigBlockSize, posZ * bigBlockSize), new Vector3(bigBlockSize, bigBlockSize, bigBlockSize)));
					}
				}
			}
		}
		return chunk;
	}

	async Task<ChunkRenderData> GetChunkMesh(Chunk chunkData, int id)
	{
		Node3D chunk = new Node3D();
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		// C# arrays cannot be resized or expanded, so use Lists to create geometry.
		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var normals = new List<Vector3>();
		var indices = new List<int>();

		//Initialized Values
		int maxX,maxY,maxZ,minX,minY,minZ,duplicateVert;
		Vector3 origin,vert;
		int[] newVertIndexNumbers;

		for (int e = 0; e < chunkData.brushes.Count; e++)
		{
			 maxX = int.MinValue;
			 maxY = int.MinValue;
			 maxZ = int.MinValue;
			 minX = int.MaxValue;
			 minY = int.MaxValue;
			 minZ = int.MaxValue;

			for (int i = 0; i < chunkData.brushes[e].vertices.Length / 3; i++)
			{
				maxX = Math.Max(maxX, chunkData.brushes[e].vertices[i, 0]);
				maxY = Math.Max(maxY, chunkData.brushes[e].vertices[i, 1]);
				maxZ = Math.Max(maxZ, chunkData.brushes[e].vertices[i, 2]);
				minX = Math.Min(minX, chunkData.brushes[e].vertices[i, 0]);
				minY = Math.Min(minY, chunkData.brushes[e].vertices[i, 1]);
				minZ = Math.Min(minZ, chunkData.brushes[e].vertices[i, 2]);
			}

			origin = new Vector3((minX + maxX) / 2, (minY + maxY) / 2, (minZ + maxZ) / 2);

			newVertIndexNumbers = new int[chunkData.brushes[e].vertices.Length];

			for (int i = 0; i < chunkData.brushes[e].vertices.Length / 3; i++)
			{
				vert = new Vector3(chunkData.brushes[e].vertices[i, 0], chunkData.brushes[e].vertices[i, 1], chunkData.brushes[e].vertices[i, 2]);

				//Check if duplicate vert
				duplicateVert = -1;
				for (int j = 0; j < verts.Count; j++)
				{
					if(vert == verts[j])
					{
                        newVertIndexNumbers[i] = j;
                        break;
					}
				}

				if(duplicateVert < 0)
				{
					verts.Add(vert);
					normals.Add(((vert - origin)).Normalized());
					uvs.Add(new Vector2(0, 0));
					newVertIndexNumbers[i] = verts.Count-1;
				}
			}

			for (int i = 0; i < chunkData.brushes[e].indicies.Length; i++)
			{
				indices.Add(newVertIndexNumbers[chunkData.brushes[e].indicies[i]]);
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


		return new ChunkRenderData() { id = id, state = ChunkRenderDataState.ready, chunkNode = chunk, meshNode = meshObject, position = new Vector3(chunkData.positionX * chunkSize, 0, chunkData.positionZ * chunkSize) };
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
		public Thread thread;
		public ChunkRenderDataState state;
		public Node3D chunkNode;
		public MeshInstance3D meshNode;
		public Vector3 position;
		public int id;
	}

	public enum ChunkRenderDataState
	{
		running,
		ready,
		garbageCollector,
	}
}
