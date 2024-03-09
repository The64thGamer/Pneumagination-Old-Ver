using Godot;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 0;
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

			newVertIndexNumbers = new int[currentBrush.vertices.Length];

			for (int i = 0; i < currentBrush.vertices.Length / 3; i++)
			{
				vert.X = currentBrush.vertices[i, 0];
				vert.Y = currentBrush.vertices[i, 1];
				vert.Z = currentBrush.vertices[i, 2];

				//Check if duplicate vert
				if (vertexHashTable.TryGetValue(vert, out int index))
				{
					newVertIndexNumbers[i] = index;
				}
				else
				{
					vertexHashTable.Add(vert, verts.Count);
					newVertIndexNumbers[i] = verts.Count;
					verts.Add(vert);
				}
			}

			for (int i = 0; i < currentBrush.indicies.Length; i++)
			{
				indices.Add(newVertIndexNumbers[currentBrush.indicies[i]]);
			}
		}

		/*
		//NEW MESH
		verts = new List<Vector3>()
		{
			new Vector3(0			,0	,0),
			new Vector3(0			,0	,1),
			new Vector3(0.866025f	,0	,0.5f),
			new Vector3(0.866025f	,1	,-0.5f),
			new Vector3(0			,0	,-1),
			new Vector3(-0.866025f	,1	,-0.5f),
			new Vector3(-0.866025f	,0	,0.5f),
		};

		indices = new List<int>()
		{
			0,2,1,
			0,3,2,
			0,4,3,
			0,5,4,
			0,6,5,
			0,1,6
		};
		*/

		//Create index copy to not mess up algoritm
		List<int> newIndices = new List<int>();
		for (int i = 0; i < indices.Count; i++)
		{
			newIndices.Add(indices[i]);
		}

		//Setup normals
		List<Vector3> normals = new List<Vector3>();
		for (int i = 0; i < verts.Count; i++)
		{
			normals.Add(new Vector3(0,1,0));
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

		//Vertex Splitter
		List<int> adjacentTriangleIndices = new List<int>();
		List<Vector3> adjacentFaceNormals = new List<Vector3>();
		int oldVertCount = verts.Count;
		Random rnd = new Random();

		for (int x = 0; x < oldVertCount; x++)
		{
			Vector3 currentVert = verts[x];
			if (triangleAdjacencyList.TryGetValue(currentVert, out adjacentTriangleIndices))
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
					Vector3 pos = (verts[indices[adjacentTriangleIndices[i]]]
					+ verts[indices[adjacentTriangleIndices[i + 1]]]
					+ verts[indices[adjacentTriangleIndices[i + 2]]]) / 3.0f;
					
					/*
					DrawLine3D.Instance.DrawLine(
						pos,
						adjacentFaceNormals[adjacentFaceNormals.Count - 1].Normalized() + pos,
						new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), 1),
						10000,
						-1
					);
					*/
				}

				//REMEMBER THAT "adjacentTriangleindices" is an INDEX for "indices", NOT a pointer to a vertex
				//This is REQUIRED for edge splitting

				//For loop of all triangles
				//On current vert, find triangle that matches the vertex. If none found, skip
				//Once a match is found, compare normal angles and see if vertex split is needed
				//If so, split the MAIN "currentVert" vert, not the one used to match the tris.
				//Do this by adding a new vert entry to verts.
				//Change the first of the two selected tris to point to this new vert-
				//- by modifying indices[adjacentTriangleIndices[????]-
				//- where ???? = the current triangle of adjacenttriangleindices's vertex that matches "currentvert" out of the 3.

				bool splitCheck = false;

				for (int i = 0; i < adjacentTriangleIndices.Count; i += 3)
				{
					for (int e = 0; e < 3; e++)
					{

						if (indices[adjacentTriangleIndices[i + e]] != x)
						{
							int otherTriangleStartingIndex = -1;
							for (int k = 0; k < adjacentTriangleIndices.Count; k++)
							{
								if (k != (i + e) && indices[adjacentTriangleIndices[i + e]] == indices[adjacentTriangleIndices[k]])
								{
									otherTriangleStartingIndex = k - (k % 3);
									break;
								}
							}
							if (otherTriangleStartingIndex >= 0 && adjacentFaceNormals[i / 3].Dot(adjacentFaceNormals[otherTriangleStartingIndex / 3]) <= 0)
							{
								splitCheck = true;
								verts.Add(currentVert);
								normals.Add(new Vector3(0, -1, 0));
								for (int k = 0; k < 3; k++)
								{
									if(indices[adjacentTriangleIndices[otherTriangleStartingIndex+k]] == x)
									{
										newIndices[adjacentTriangleIndices[otherTriangleStartingIndex + k]] = verts.Count - 1;
										break;
									}
								}

							}
						}
					}
				}


				if (splitCheck)
				{
					//Okay we're out of the for loop, verts are split
					//Next you wanna copy waaaay up above the "fast lookup table for adjacent triangles"
					//This one is now gonna iterate through "adjacenttriangleindices"
					//BUT, its gonna skip adding to the dictionary if the vertex POSITION does not match "currentvert"-
					//because we just care about which tris now belong to which split groups of verts, they all still-
					//potentially connect through the other verts right now.
					//In the value of the dictionary, JUST have it save a list of the indexes of the current triangles -> / 3.0f and Mathf.ToFloorInt()

					Dictionary<int, List<int>> finalAdjacencyList = new Dictionary<int, List<int>>();
					for (int i = 0; i < adjacentTriangleIndices.Count; i++)
					{
						int lookupIndex = newIndices[adjacentTriangleIndices[i]];

						if (currentVert.IsEqualApprox(verts[lookupIndex]))
						{
							startIndex = i - (i % 3);
							if (finalAdjacencyList.ContainsKey(lookupIndex))
							{
								finalAdjacencyList[lookupIndex].Add(startIndex);
								finalAdjacencyList[lookupIndex].Add(startIndex + 1);
								finalAdjacencyList[lookupIndex].Add(startIndex + 2);
							}
							else
							{
								finalAdjacencyList.Add(lookupIndex, new List<int>()
								{
									startIndex,
									startIndex+1,
									startIndex+2,
								});
							}
						}
					}

					//Out of that for loop, into another one.
					//Iterate through every part of the dictionary to get the List<int> values
					//Use each of these indexes to sample from adjacentFaceNormals and average them together
					//DONT normalize anything, we want the magnitude to weight the normals
					//AFTER the average is calculated, normalize it.
					//Apply this normal value to the vertex 

					//GD.Print("List " + finalAdjacencyList.Count);
					string additions = "Count = " + oldVertCount + " CurrentVert = " + x + " - ";
					foreach ((int key, List<int> value) in finalAdjacencyList)
					{
						Vector3 finalNormal = Vector3.Zero;
						for (int k = 0; k < value.Count; k += 3)
						{
							finalNormal += adjacentFaceNormals[value[k] / 3];
						}
						finalNormal = finalNormal.Normalized();
						additions += key + ", ";
						normals[key] = finalNormal;
					}
					//GD.Print(additions);
				}
				else
				{
					//No split occured, average all normals in the whole list and apply it.
					//This will never run if the world is generated full of right angled cubes.

					Vector3 finalNormal = Vector3.Zero;
					for (int k = 0; k < adjacentFaceNormals.Count; k++)
					{
						finalNormal += adjacentFaceNormals[k];
					}
					normals[x] = finalNormal.Normalized();
				}
			}
		}

		for (int i = 3000; i < 3300; i += 3)
		{
			Color c = new Color((float)rnd.NextDouble(), (float)rnd.NextDouble(), (float)rnd.NextDouble(), 1);
			for (int e = 0; e < 3; e++)
			{
				Vector3 pos = (verts[newIndices[i]] + verts[newIndices[i+1]] + verts[newIndices[i+2]])/3;
				DrawLine3D.Instance.DrawLine(
					(verts[newIndices[i+e]] +  pos + pos + pos + pos) / 5,
					normals[newIndices[i + e]] + ((verts[newIndices[i + e]] + pos + pos + pos + pos) / 5),
					c,
					10000,
					-1
				);
			}

		}

		// Convert Lists to arrays and assign to surface array
		ArrayMesh arrMesh = new ArrayMesh();
		surfaceArray[(int)Mesh.ArrayType.Vertex] = verts.ToArray();
		surfaceArray[(int)Mesh.ArrayType.TexUV] = new Vector2[verts.Count];
		surfaceArray[(int)Mesh.ArrayType.Normal] = normals.ToArray();
		surfaceArray[(int)Mesh.ArrayType.Index] = newIndices.ToArray();
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
