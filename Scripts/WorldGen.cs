using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static WorldGen;

public partial class WorldGen : Node3D
{
	[Export] Material mat;
	[Export] int chunkRenderSize = 3;
	[Export] bool hideBigBlocks = false;
	int seedA = 0;
	int seedB = 0;
	int seedC = 0;
	int seedD = 0;
	int totalChunksRendered = 0;

	List<ChunkRenderData> ongoingChunkRenderData = new List<ChunkRenderData>();

	//Noise
	FastNoiseLite celNoiseA = new FastNoiseLite();
	FastNoiseLite celNoiseB = new FastNoiseLite();
	FastNoiseLite os2NoiseA = new FastNoiseLite();
	FastNoiseLite os2NoiseB = new FastNoiseLite();

	//Consts
	const int bigBlockSize = 6;
	const int chunkSize = 252;
	readonly byte[] brushIndices = new byte[]
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

	//TODO: add crafting system where you get shako for 2 metal
	public override void _Ready()
	{
		SetSurfaceBrushCache();

		Random rnd = new Random();
		if (seedA == 0)
		{
			seedA = rnd.Next();
		}
		if (seedB == 0)
		{
			seedB = rnd.Next();
		}
		if (seedC == 0)
		{
			seedC = rnd.Next();
		}
		if (seedD == 0)
		{
			seedD = rnd.Next();
		}

		celNoiseA.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		celNoiseA.SetFrequency(.01f);
		celNoiseA.SetSeed(seedA);
		celNoiseA.SetFractalType(FastNoiseLite.FractalType.PingPong);
		celNoiseA.SetFractalOctaves(1);
		celNoiseA.SetFractalPingPongStrength(1.5f);

		celNoiseB.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		celNoiseB.SetFrequency(.02f);
		celNoiseB.SetSeed(seedB);
		celNoiseB.SetFractalType(FastNoiseLite.FractalType.FBm);
		celNoiseB.SetFractalOctaves(3);

		os2NoiseA.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		os2NoiseA.SetFrequency(0.001f);
		os2NoiseA.SetSeed(seedC);
		os2NoiseA.SetFractalType(FastNoiseLite.FractalType.FBm);
		os2NoiseA.SetFractalOctaves(4);

		os2NoiseB.SetNoiseType(FastNoiseLite.NoiseType.Perlin);
		os2NoiseB.SetFrequency(0.005f);
		os2NoiseB.SetSeed(seedD);
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
					else
					{
						int bitMask = CheckSurfaceBrushType(bigBlockArray, posX, posY, posZ);
						if (bitMask != 0)
						{
							List<Brush> brushes = CreateSurfaceBrushes(bitMask, (byte)(posX * bigBlockSize), (byte)(posY * bigBlockSize), (byte)(posZ * bigBlockSize));
							if (brushes != null)
							{
								chunk.brushes.AddRange(brushes);
							}
						}
					}
				}
			}
		}

		return chunk;
	}

	int CheckSurfaceBrushType(bool[,,] bigBlockArray, int x, int y, int z)
	{
		int bitmask = 0;
		int bitWise = 0;
		int newX, newY, newZ;

		for (newY = 1; newY > -2; newY--)
		{
			for (newZ = 1; newZ > -2; newZ--)
			{
				for (newX = -1; newX < 2; newX++)
				{

					if (x + newX < bigBlockArray.GetLength(0) &&
					   y + newY < bigBlockArray.GetLength(1) &&
					   z + newZ < bigBlockArray.GetLength(2) &&
					   x + newX >= 0 &&
					   y + newY >= 0 &&
					   z + newZ >= 0 &&
					   bigBlockArray[x + newX, y + newY, z + newZ]
					   )
					{
						bitmask |= 1 << bitWise;
					}
					bitWise++;
				}
			}
		}
		return bitmask;
	}

	bool CheckBrushVisibility(bool[,,] bigBlockArray, int x, int y, int z)
	{
		if (hideBigBlocks)
		{
			return true;
		}

		//Check if block on chunk boundary
		if (CheckIndexInvalidity(x, bigBlockArray.GetLength(0)) || CheckIndexInvalidity(y, bigBlockArray.GetLength(1)) || CheckIndexInvalidity(z, bigBlockArray.GetLength(2)))
		{
			return false;
		}

		return bigBlockArray[x - 1, y, z] &&
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
		Godot.Collections.Dictionary<Vector3, int> vertexHashTable = new Godot.Collections.Dictionary<Vector3, int>();
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

			for (int i = 0; i < currentBrush.vertices.Length; i += 3)
			{
				maxX = Math.Max(maxX, currentBrush.vertices[i]);
				maxY = Math.Max(maxY, currentBrush.vertices[i + 1]);
				maxZ = Math.Max(maxZ, currentBrush.vertices[i + 2]);
				minX = Math.Min(minX, currentBrush.vertices[i]);
				minY = Math.Min(minY, currentBrush.vertices[i + 1]);
				minZ = Math.Min(minZ, currentBrush.vertices[i + 2]);
			}

			origin.X = (minX + maxX) / 2;
			origin.Y = (minY + maxY) / 2;
			origin.Z = (minZ + maxZ) / 2;

			for (int i = 0; i < brushIndices.Length; i++)
			{
				vert.X = currentBrush.vertices[brushIndices[i] * 3];
				vert.Y = currentBrush.vertices[(brushIndices[i] * 3) + 1];
				vert.Z = currentBrush.vertices[(brushIndices[i] * 3) + 2];
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
		System.Collections.Generic.Dictionary<Vector3, List<int>> triangleAdjacencyList = new System.Collections.Generic.Dictionary<Vector3, List<int>>();
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

			System.Collections.Generic.Dictionary<int, List<int>> finalAdjacencyList = new System.Collections.Generic.Dictionary<int, List<int>>();
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
		byte newX = (byte)pos.X;
		byte newY = (byte)pos.Y;
		byte newZ = (byte)pos.Z;

		byte maxX = (byte)(newX + (byte)size.X);
		byte maxY = (byte)(newY + (byte)size.Y);
		byte maxZ = (byte)(newZ + (byte)size.Z);

		Brush brush = new Brush();
		brush.vertices = new byte[24]
			{
				newX,newY,newZ,
				maxX,newY,newZ,
				maxX,newY,maxZ,
				newX,newY,maxZ,

				newX,maxY,newZ,
				maxX,maxY,newZ,
				maxX,maxY,maxZ,
				newX,maxY,maxZ,
			};
		brush.hiddenFlag = hiddenFlag;
		return brush;
	}

	List<Brush> CreateSurfaceBrushes(int id, byte posX, byte posY, byte posZ)
	{
		List<Brush> brushCopies = new List<Brush>();
		if (surfaceBrushes.TryGetValue(id, out byte[] verts))
		{
			for (int i = 0; i < verts.Length / 24; i++)
			{
				Brush b = new Brush { hiddenFlag = false, vertices = new byte[24] };
				for (int e = 0; e < 24; e++)
				{
					b.vertices[e] = verts[e + (i * 24)];
				}
				brushCopies.Add(b);

			}

			for (int e = 0; e < brushCopies.Count; e++)
			{
				for (int i = 0; i < brushCopies[e].vertices.Length; i += 3)
				{
					brushCopies[e].vertices[i] += posX;
					brushCopies[e].vertices[i + 1] += posY;
					brushCopies[e].vertices[i + 2] += posZ;
				}
			}

			return brushCopies;
		}

		return null;
	}

	float GetClampedNoise(float noise)
	{
		return (noise + 1.0f) / 2.0f;
	}

	public class Brush
	{
		//Vert Order

		//Bottom
		//0 Back Left
		//1 Back Right
		//2 Front Right
		//3 Front Left

		//Top
		//4 Back Left
		//5 Back Right
		//6 Front Right
		//7 Front Left
		public byte[] vertices;
		public uint[] textures;
		public bool hiddenFlag;
	}

	public class Chunk
	{
		public int positionX;
		public int positionZ;
		public List<Brush> brushes;
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

	/*Norm       
	24 25 26	
	21 22 23	
	18 19 20	

	15 16 17	
	12 13 14	
	9  10 11	

	6  7  8		
	3  4  5		
	0  1  2		
*/
	void SetSurfaceBrushCache()
	{
		for (int i = 0; i < cachedBrushes.Length; i++)
		{
			byte[,,] newBitMask = new byte[3, 3, 3];
			for (int x = 0; x < 3; x++)
			{
				for (int y = 0; y < 3; y++)
				{
					for (int z = 0; z < 3; z++)
					{
						newBitMask[x, y, z] = cachedBrushes[i].bitMask[x, y, z];
					}
				}
			}
			Vector3[,,] newVertices = new Vector3[2, 2, 2];
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					for (int z = 0; z < 2; z++)
					{
						newVertices[x, y, z] = cachedBrushes[i].vertices[x, y, z];
					}
				}
			}
			GD.Print(String.Join("", newBitMask.Cast<byte>()));

			RotateSquareMatrixX(newBitMask);
			//RotateSquareMatrixX(newVertices);

			byte[] finalVerts = new byte[24];
			int e = 0;
			for (int x = 0; x < 2; x++)
			{
				for (int y = 0; y < 2; y++)
				{
					for (int z = 0; z < 2; z++)
					{
						finalVerts[e] = (byte)newVertices[x, y, z].X;
						finalVerts[e+1] = (byte)newVertices[x, y, z].Y;
						finalVerts[e+2] = (byte)newVertices[x, y, z].Z;
						e += 3;
					}
				}
			}
			surfaceBrushes.Add(Convert.ToInt32(String.Join("", newBitMask.Cast<byte>()), 2),finalVerts);
			GD.Print(String.Join("", newBitMask.Cast<byte>()));
		}
	}
	void RotateSquareMatrixX<T>(T[,,] a)
	{
		int N = a.GetLength(0);
		for (int z = 0; z < N; z++)
		{
			for (int i = 0; i < N / 2; i++)
			{
				for (int j = i; j < N - i - 1; j++)
				{
					var temp = a[i, j, z];
					a[i, j, z] = a[N - 1 - j, i, z];
					a[N - 1 - j, i, z] = a[N - 1 - i, N - 1 - j, z];
					a[N - 1 - i, N - 1 - j, z] = a[j, N - 1 - i, z];
					a[j, N - 1 - i, z] = temp;
				}
			}
		}
	}

	struct CachedSurfaceBrush
	{
		public byte[,,] bitMask;
		public Vector3[,,] vertices;
	}

	CachedSurfaceBrush[] cachedBrushes = new CachedSurfaceBrush[] {
		new CachedSurfaceBrush(){
			bitMask = new byte[,,]{
				{ { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } },
				{ { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } },
				{ { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } } },
			vertices = new Vector3[,,]{
				{ {new Vector3(0,0,0),new Vector3(6,0,0) }, {new Vector3(6,0,6),new Vector3(0,0,6) }, },
				{ {new Vector3(0,1,0),new Vector3(6,1,0) }, {new Vector3(6,1,6),new Vector3(0,1,6) } }}},
        new CachedSurfaceBrush(){
            bitMask = new byte[,,]{
                { { 1, 1, 1 }, { 1, 1, 1 }, { 1, 1, 1 } },
                { { 0, 0, 0 }, { 0, 0, 0 }, { 1, 1, 0 } },
                { { 0, 0, 0 }, { 0, 0, 0 }, { 0, 0, 0 } } },
            vertices = new Vector3[,,]{
                { {new Vector3(0,0,0),new Vector3(6,0,0) }, {new Vector3(6,0,6),new Vector3(0,0,6) }, },
                { {new Vector3(0,1,0),new Vector3(6,1,0) }, {new Vector3(6,1,6),new Vector3(0,1,6) } }}},
    };

	System.Collections.Generic.Dictionary<int, byte[]> surfaceBrushes = new System.Collections.Generic.Dictionary<int, byte[]>
	{
		{0b111111111000000000000000000,new byte[]{
					0,0,0, 6,0,0, 6,0,6, 0,0,6,
					0,1,0, 6,1,0, 6,1,6, 0,1,6,
		}},
		{ 0b000000000000000000111111111,new byte[]{
					0,5,0, 6,5,0, 6,5,6, 0,5,6,
					0,6,0, 6,6,0, 6,6,6, 0,6,6,
		}},
		{ 0b111111111000000111000000000,new byte[]{
					0,0,0, 6,0,0, 6,0,3, 0,0,3,
					0,1,0, 6,1,0, 6,2,2, 0,2,2,

					0,0,3, 6,0,3, 6,0,6, 0,0,6,
					0,2,2, 6,2,2, 6,4,4, 0,4,4,

					0,4,4, 6,4,4, 6,0,6, 0,0,6,
					0,6,5, 6,6,5, 6,6,6, 0,6,6,
		}}
	};
}
