using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 3;
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
			if (chunkData == null)
			{
				return;
			}
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

		//BlockArray Setup
		bool[,,] bigBlockArray = new bool[chunkSize / bigBlockSize, chunkSize / bigBlockSize, chunkSize / bigBlockSize];
		float noiseValue;
		int posX, posY, posZ, newX, newY, newZ;

		for (posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					newX = (posX * bigBlockSize) + (chunkSize * x);
					newY = (posY * bigBlockSize);
					newZ = (posZ * bigBlockSize) + (chunkSize * z);

					noiseValue = (GetClampedNoise(celNoiseA.GetNoise(newX, newY, newZ)));
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
						bigBlockArray[posX, posY, posZ] = true;
					}
				}
			}
		}

		for (posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					if (bigBlockArray[posX, posY, posZ])
					{
						chunk.brushes.Add(
							CreateBrush(
								new Vector3(posX * bigBlockSize, posY * bigBlockSize, posZ * bigBlockSize),
								new Vector3(bigBlockSize, bigBlockSize, bigBlockSize),
								CheckBrushVisibility(bigBlockArray, posX, posY, posZ)
								));
					}
				}
			}
		}

		return chunk;
	}

	bool CheckBrushVisibility(bool[,,] bigBlockArray, int x, int y, int z)
	{
		//Check if block on chunk boundary
		if (CheckIndexInvalidity(x, bigBlockArray.GetLength(0)) || CheckIndexInvalidity(y, bigBlockArray.GetLength(1)) || CheckIndexInvalidity(z, bigBlockArray.GetLength(2)))
		{
			return false;
		}

		return	bigBlockArray[x - 1, y, z] &&
				bigBlockArray[x + 1, y, z] &&
				bigBlockArray[x, y - 1, z] &&
				bigBlockArray[x, y + 1, z] &&
				bigBlockArray[x, y, z - 1] &&
				bigBlockArray[x, y, z + 1];
	}

	bool CheckIndexInvalidity(int index, int length)
	{
		if (index == 0 || index == length - 1)
		{
			return true;
		}
		return false;
	}

	async Task<ChunkRenderData> GetChunkMesh(Chunk chunkData, int id)
	{
		Node3D chunk = new Node3D();
		var surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);

		//Initialized Values
		List<Vector3> verts = new List<Vector3>();
		Dictionary<Vector3, int> vertexHashTable = new Dictionary<Vector3, int>();
		List<int> indices = new List<int>();
		int maxX, maxY, maxZ, minX, minY, minZ;
		Vector3 origin, vert;
		int[] newVertIndexNumbers;

		//Collect all brush vertices, merge duplicate ones
		foreach (Brush currentBrush in chunkData.brushes)
		{
			if (currentBrush.hiddenFlag)
			{
				continue;
			}
			maxX = int.MinValue;
			maxY = int.MinValue;
			maxZ = int.MinValue;
			minX = int.MaxValue;
			minY = int.MaxValue;
			minZ = int.MaxValue;

			for (int i = 0; i < currentBrush.vertices.Length / 3; i++)
			{
				maxX = Math.Max(maxX, currentBrush.vertices[i, 0]);
				maxY = Math.Max(maxY, currentBrush.vertices[i, 1]);
				maxZ = Math.Max(maxZ, currentBrush.vertices[i, 2]);
				minX = Math.Min(minX, currentBrush.vertices[i, 0]);
				minY = Math.Min(minY, currentBrush.vertices[i, 1]);
				minZ = Math.Min(minZ, currentBrush.vertices[i, 2]);
			}

			origin.X = (minX + maxX) / 2;
			origin.Y = (minY + maxY) / 2;
			origin.Z = (minZ + maxZ) / 2;

			for (int i = 0; i < currentBrush.indicies.Length; i++)
			{
				vert.X = currentBrush.vertices[currentBrush.indicies[i], 0];
				vert.Y = currentBrush.vertices[currentBrush.indicies[i], 1];
				vert.Z = currentBrush.vertices[currentBrush.indicies[i], 2];
				indices.Add(verts.Count);
				verts.Add(vert);
			}
		}

		if (verts.Count == 0)
		{
			GD.Print("Chunk had no blocks / no visible blocks.");
			return null;
		}

		//Setup normals
		List<Vector3> normals = new List<Vector3>();
		for (int i = 0; i < verts.Count; i++)
		{
			normals.Add(new Vector3(0, 1, 0));
		}

		//Create a fast lookup table for adjacent triangles
		Dictionary<Vector3, List<int>> triangleAdjacencyList = new Dictionary<Vector3, List<int>>();
		Vector3 lookupVertex;
		int startIndex;
		for (int i = 0; i < indices.Count; i++)
		{
			lookupVertex = verts[indices[i]];
			startIndex = i - (i % 3);

			if (triangleAdjacencyList.ContainsKey(lookupVertex))
			{
				triangleAdjacencyList[lookupVertex].Add(startIndex);
				triangleAdjacencyList[lookupVertex].Add(startIndex + 1);
				triangleAdjacencyList[lookupVertex].Add(startIndex + 2);
			}
			else
			{
				triangleAdjacencyList.Add(lookupVertex, new List<int>()
				{
					startIndex,
					startIndex+1,
					startIndex+2,
				});
			}
		}

		//Vertex Merger
		List<Vector3> adjacentFaceNormals = new List<Vector3>();

		foreach ((Vector3 currentVert, List<int> adjacentTriangleIndices) in triangleAdjacencyList)
		{
			//Gather all face normals
			adjacentFaceNormals = new List<Vector3>();
			for (int i = 0; i < adjacentTriangleIndices.Count; i += 3)
			{
				adjacentFaceNormals.Add((
					verts[indices[adjacentTriangleIndices[i]]] -
					verts[indices[adjacentTriangleIndices[i + 1]]]).
					Cross(verts[indices[adjacentTriangleIndices[i + 2]]] -
					verts[indices[adjacentTriangleIndices[i + 1]]]
					));
			}

			//Check if verts should be merged
			for (int i = 0; i < adjacentTriangleIndices.Count; i++)
			{
				if (verts[indices[adjacentTriangleIndices[i]]].IsEqualApprox(currentVert))
				{
					for (int k = 0; k < adjacentTriangleIndices.Count; k++)
					{
						if (currentVert.IsEqualApprox(verts[indices[adjacentTriangleIndices[k]]])
							&& adjacentFaceNormals[(i - (i % 3)) / 3].Dot(adjacentFaceNormals[(k - (k % 3)) / 3]) > 0)
						{
							indices[adjacentTriangleIndices[k]] = indices[adjacentTriangleIndices[i]];
						}
					}
				}
			}

			Dictionary<int, List<int>> finalAdjacencyList = new Dictionary<int, List<int>>();
			for (int i = 0; i < adjacentTriangleIndices.Count; i++)
			{
				int lookupIndex = indices[adjacentTriangleIndices[i]];

				if (verts[lookupIndex].IsEqualApprox(currentVert))
				{
					startIndex = i - (i % 3);
					if (finalAdjacencyList.ContainsKey(lookupIndex))
					{
						finalAdjacencyList[lookupIndex].Add(startIndex);

					}
					else
					{
						finalAdjacencyList.Add(lookupIndex, new List<int>()
						{
							startIndex,
						});
					}
					i = startIndex + 2;
				}
			}

			foreach ((int key, List<int> value) in finalAdjacencyList)
			{
				Vector3 finalNormal = Vector3.Zero;
				for (int k = 0; k < value.Count; k++)
				{
					finalNormal += adjacentFaceNormals[value[k] / 3];
				}
				normals[key] = finalNormal.Normalized();
			}

		}

		// Convert Lists to arrays and assign to surface array
		ArrayMesh arrMesh = new ArrayMesh();
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = new Vector2[verts.Count];
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = indices.ToArray();
		arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);

		//Assign the surface to a mesh and return
		MeshInstance3D meshObject = new MeshInstance3D();
		meshObject.Mesh = arrMesh;
		meshObject.Mesh.SurfaceSetMaterial(0, mat);
		return new ChunkRenderData()
		{
			id = id,
			state = ChunkRenderDataState.ready,
			chunkNode = chunk,
			meshNode = meshObject,
			position = new Vector3(chunkData.positionX * chunkSize, 0, chunkData.positionZ * chunkSize)
		};
	}



	Brush CreateBrush(Vector3 pos, Vector3 size, bool hiddenFlag)
	{
		Brush brush = new Brush(63,pos);
		brush.hiddenFlag = hiddenFlag;
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
		public bool hiddenFlag;

		public Brush(int defaultShape, Vector3 pos)
		{
			switch (defaultShape)
			{
				case 63://3x3x3 Cube
										
					vertices = new byte[8, 3];

					vertices[0, 0] = (byte)(pos.X);
					vertices[0, 1] = (byte)(pos.Y);
					vertices[0, 2] = (byte)(pos.Z);

					vertices[1, 0] = (byte)(3 + pos.X);
					vertices[1, 1] = (byte)(pos.Y);
					vertices[1, 2] = (byte)(pos.Z);

					vertices[2, 0] = (byte)(3 + pos.X);
					vertices[2, 1] = (byte)(pos.Y);
					vertices[2, 2] = (byte)(3 + pos.Z);

					vertices[3, 0] = (byte)(pos.X);
					vertices[3, 1] = (byte)(pos.Y);
					vertices[3, 2] = (byte)(3 + pos.Z);

					vertices[4, 0] = (byte)(pos.X);
					vertices[4, 1] = (byte)(3 + pos.Y);
					vertices[4, 2] = (byte)(pos.Z);

					vertices[5, 0] = (byte)(3 + pos.X);
					vertices[5, 1] = (byte)(3 + pos.Y);
					vertices[5, 2] = (byte)(pos.Z);

					vertices[6, 0] = (byte)(3 + pos.X);
					vertices[6, 1] = (byte)(3 + pos.Y);
					vertices[6, 2] = (byte)(3 + pos.Z);

					vertices[7, 0] = (byte)(pos.X);
					vertices[7, 1] = (byte)(3 + pos.Y);
					vertices[7, 2] = (byte)(3 + pos.Z);

					indicies = new byte[]
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
					break;
				default: //1x1x1ft Cube
					vertices = new byte[8, 3];

					vertices[0, 0] = (byte)(pos.X);
					vertices[0, 1] = (byte)(pos.Y);
					vertices[0, 2] = (byte)(pos.Z);

					vertices[1, 0] = (byte)(1 + pos.X);
					vertices[1, 1] = (byte)(pos.Y);
					vertices[1, 2] = (byte)(pos.Z);

					vertices[2, 0] = (byte)(1 + pos.X);
					vertices[2, 1] = (byte)(pos.Y);
					vertices[2, 2] = (byte)(1 + pos.Z);

					vertices[3, 0] = (byte)(pos.X);
					vertices[3, 1] = (byte)(pos.Y);
					vertices[3, 2] = (byte)(1 + pos.Z);

					vertices[4, 0] = (byte)(pos.X);
					vertices[4, 1] = (byte)(1 + pos.Y);
					vertices[4, 2] = (byte)(pos.Z);

					vertices[5, 0] = (byte)(1 + pos.X);
					vertices[5, 1] = (byte)(1 + pos.Y);
					vertices[5, 2] = (byte)(pos.Z);

					vertices[6, 0] = (byte)(1 + pos.X);
					vertices[6, 1] = (byte)(1 + pos.Y);
					vertices[6, 2] = (byte)(1 + pos.Z);

					vertices[7, 0] = (byte)(pos.X);
					vertices[7, 1] = (byte)(1 + pos.Y);
					vertices[7, 2] = (byte)(1 + pos.Z);

					indicies = new byte[]
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
					break;
			}
		}
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
