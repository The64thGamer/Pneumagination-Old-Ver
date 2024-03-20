using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public partial class WorldGen : Node3D
{
	//Exports
	[Export] bool hideBigBlocks = false;
	[Export] Curve curve1;
	[Export] Curve curve2;
	[Export] Curve curve3;

	//Globals
	public static uint seedA = 0;
	public static uint seedB = 0;
	public static uint seedC = 0;
	public static uint seedD = 0;
	public static int totalChunksRendered = 0;
	public static bool firstChunkLoaded;

	//Locals
	Vector3 oldChunkPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
	List<LoadedChunkData> loadedChunks = new List<LoadedChunkData>();
	List<ChunkRenderData> ongoingChunkRenderData = new List<ChunkRenderData>();
	Material[] mats;
	FastNoiseLite noise, noiseB, noiseC;

	//Consts
	const int chunkLoadingDistance = 6;
	const int chunkUnloadingDistance = 8;
	const int bigBlockSize = 6;
	const int chunkSize = 84;
	readonly byte[] brushIndices = new byte[]
				{
					//Bottom
					2, 1, 0,
					0, 3, 2,
					//North
					6, 2, 3,
					3, 7, 6,
					//Top
					5, 6, 7,
					7, 4, 5,
					//South
					1, 5, 4,
					4, 0, 1,
					//West
					7, 3, 0,
					0, 4, 7,
					//East
					6, 5, 1,
					1, 2, 6
				};

	//TODO: add crafting system where you get shako for 2 metal
	public override void _Ready()
	{
		Random rnd = new Random();
		if (seedA == 0)
		{
			seedA = ((uint)rnd.Next(1 << 30) << 2) | (uint)rnd.Next(1 << 2);
		}
		if (seedB == 0)
		{
			seedB = ((uint)rnd.Next(1 << 30) << 2) | (uint)rnd.Next(1 << 2);
		}
		if (seedC == 0)
		{
			seedC = ((uint)rnd.Next(1 << 30) << 2) | (uint)rnd.Next(1 << 2);
		}
		if (seedD == 0)
		{
			seedD = ((uint)rnd.Next(1 << 30) << 2) | (uint)rnd.Next(1 << 2);
		}

		mats = new Material[5];
		for (int i = 0; i < 5; i++)
		{
			mats[i] = GD.Load("res://Materials/" + i + ".tres") as Material;
		}

		noise = new FastNoiseLite();
		noise.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		noise.SetFrequency(0.002f);
		noise.SetSeed((int)seedA);
		noise.SetFractalType(FastNoiseLite.FractalType.PingPong);
		noise.SetFractalOctaves(3);
		noise.SetFractalPingPongStrength(2);
		noise.SetCellularJitter(1.2f);
		noise.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		noise.SetDomainWarpAmp(400);

		noiseB = new FastNoiseLite();
		noiseB.SetNoiseType(FastNoiseLite.NoiseType.OpenSimplex2);
		noiseB.SetFrequency(0.06f);
		noiseB.SetSeed((int)seedB);
		noiseB.SetDomainWarpType(FastNoiseLite.DomainWarpType.OpenSimplex2);
		noiseB.SetDomainWarpAmp(250);

		noiseC = new FastNoiseLite();
		noiseC.SetNoiseType(FastNoiseLite.NoiseType.Cellular);
		noiseC.SetFrequency(0.005f);
		noiseC.SetSeed((int)seedC);
		noiseC.SetCellularDistanceFunction(FastNoiseLite.CellularDistanceFunction.Manhattan);
		noiseC.SetCellularReturnType(FastNoiseLite.CellularReturnType.CellValue);

	}

	public override void _Process(double delta)
	{
		CheckForAnyPendingFinishedChunks();
		LoadAndUnloadChunks();
	}

	void LoadAndUnloadChunks()
	{
		//Check
		Vector3 chunkPos = new Vector3(Mathf.RoundToInt(PlayerMovement.currentPosition.X / chunkSize), Mathf.RoundToInt(PlayerMovement.currentPosition.Y / chunkSize), Mathf.RoundToInt(PlayerMovement.currentPosition.Z / chunkSize));
		if (chunkPos == oldChunkPos)
		{
			return;
		}
		oldChunkPos = chunkPos;

		//Load New Chunks
		HashSet<Vector3> loadPositions = new HashSet<Vector3>();
		Vector3 temp = Vector3.Zero;
		int x, y, z;

		for (x = -chunkLoadingDistance; x < chunkLoadingDistance; x++)
		{
			for (y = -chunkLoadingDistance; y < chunkLoadingDistance; y++)
			{
				for (z = -chunkLoadingDistance; z < chunkLoadingDistance; z++)
				{
					temp.X = chunkPos.X + x;
					temp.Y = chunkPos.Y + y;
					temp.Z = chunkPos.Z + z;

					if (chunkPos.DistanceTo(temp) <= chunkLoadingDistance)
					{
						loadPositions.Add(temp);
					}
				}
			}
		}

		for (int i = 0; i < loadedChunks.Count; i++)
		{
			loadPositions.Remove(loadedChunks[i].position);
		}
		for (int i = 0; i < ongoingChunkRenderData.Count; i++)
		{
			loadPositions.Remove(ongoingChunkRenderData[i].position);
		}

		List<Vector3> sortedPosition = loadPositions.OrderBy(n => (chunkPos.DistanceTo(n))).ToList();

		foreach (Vector3 chunk in sortedPosition)
		{
			RenderChunk((int)chunk.X, (int)chunk.Y, (int)chunk.Z);
		}

		//Unload Far Away Chunks
		List<LoadedChunkData> remainingChunks = new List<LoadedChunkData>();
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			if (chunkPos.DistanceTo(loadedChunks[i].position) >= chunkUnloadingDistance)
			{
				GD.Print("Unloading Chunk (ID " + loadedChunks[i].id + "): X = " + loadedChunks[i].position.X + " Y = " + loadedChunks[i].position.Y + " Z = " + loadedChunks[i].position.Z);

				loadedChunks[i].node.QueueFree();
			}
			else
			{
				remainingChunks.Add(loadedChunks[i]);
			}
		}
		loadedChunks = remainingChunks;
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
				ongoingChunkRenderData[e].chunkNode.GlobalPosition = new Vector3(ongoingChunkRenderData[e].position.X * chunkSize, ongoingChunkRenderData[e].position.Y * chunkSize, ongoingChunkRenderData[e].position.Z * chunkSize);
				ongoingChunkRenderData[e].state = ChunkRenderDataState.garbageCollector;
				ongoingChunkRenderData[e].meshNode.AddChild(ongoingChunkRenderData[e].staticBody);
				ongoingChunkRenderData[e].staticBody.AddChild(ongoingChunkRenderData[e].collisionShape);

				loadedChunks.Add(new LoadedChunkData()
				{
					id = ongoingChunkRenderData[e].id,
					node = ongoingChunkRenderData[e].chunkNode,
					position = ongoingChunkRenderData[e].position,
					chunk = ongoingChunkRenderData[e].chunk,
					visibleBrushIndices = ongoingChunkRenderData[e].visibleBrushIndices
				});

				GD.Print("Finishing Chunk (ID " + ongoingChunkRenderData[e].id + ")");
			}
			if (ongoingChunkRenderData[e].state == ChunkRenderDataState.running)
			{
				check = true;
			}
		}

		if (!check)
		{
			if (!firstChunkLoaded)
			{
				firstChunkLoaded = true;
			}
			GD.Print("All Chunks Done Rendering");
			ongoingChunkRenderData = new List<ChunkRenderData>();
		}
	}

	void RenderChunk(int x, int y, int z)
	{
		totalChunksRendered++;
		int id = totalChunksRendered;
		ongoingChunkRenderData.Add(new ChunkRenderData() { state = ChunkRenderDataState.running, id = id, position = new Vector3(x, y, z) });
		GD.Print("Generating Chunk (ID " + totalChunksRendered + "): X = " + x + " Y = " + y + " Z = " + z);

		Task.Run(async () =>
		{
			Chunk chunk = await GenerateChunk(x, y, z, id);
			ChunkRenderData chunkData = await GetChunkMeshAsync(chunk);

			bool check = false;
			for (int i = 0; i < ongoingChunkRenderData.Count; i++)
			{
				if (ongoingChunkRenderData[i].id == id)
				{
					if (chunkData == null)
					{
						ongoingChunkRenderData[i].state = ChunkRenderDataState.garbageCollector;
						GD.Print("Chuck had 0 visible blocks (ID " + ongoingChunkRenderData[i].id + ")");
						check = true;
					}
					else
					{
						ongoingChunkRenderData[i] = chunkData;
						check = true;
					}
					break;
				}
			}

			if (!check)
			{
				GD.PrintErr("Chunk Mesh Could Not Be Finalized (ID " + id + ")");
			}

		});
	}


	//Chunks are 126x126x126
	async Task<Chunk> GenerateChunk(int x, int y, int z, int id)
	{

		Chunk chunk = new Chunk();
		chunk.id = id;
		chunk.positionX = x;
		chunk.positionY = y;
		chunk.positionZ = z;
		chunk.brushes = new List<Brush>();
		chunk.connectedInvisibleBrushes = new System.Collections.Generic.Dictionary<Brush, List<Brush>>();

		//BlockArray Setup
		byte[,,] bigBlockArray = new byte[chunkSize / bigBlockSize, chunkSize / bigBlockSize, chunkSize / bigBlockSize];
		Brush[,,] bigBlockBrushArray = new Brush[chunkSize / bigBlockSize, chunkSize / bigBlockSize, chunkSize / bigBlockSize];

		bool noiseValue = false;
		int posX, posY, posZ, newX, newY, newZ;
		float temp, temp2;

		for (posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					//Within BigBlock space, not unit space
					newX = posX + (chunkSize * x / bigBlockSize);
					newY = posY + (chunkSize * y / bigBlockSize);
					newZ = posZ + (chunkSize * z / bigBlockSize);

					if (y < 0)
					{
						//Below-Surface Generation
						noiseValue = true;
					}
					if (y >= 0)
					{
						//Above-Surface Generation
						noiseValue = false;

						if (y < 6 && y >= 0 &&
							curve1.SampleBaked(GetClampedNoise(noise.GetNoise(newX, newY, newZ))) > GetClampedChunkRange(0, 6 * chunkSize / bigBlockSize, newY))
						{
							noiseValue = true;
						}
					}

					//Both Surface Generation
					if (y < 5 && y >= -10 &&
						curve2.SampleBaked(GetClampedNoise(noiseB.GetNoise(newX, newY, newZ))) > curve3.SampleBaked(GetClampedChunkRange(-10 * chunkSize / bigBlockSize, 5 * chunkSize / bigBlockSize, newY)))
					{
						noiseValue = false;
					}

					//Apply
					if (noiseValue)
					{
						SetBitOfByte(ref bigBlockArray[posX, posY, posZ], 0, true);
					}
				}
			}
		}

		List<Brush> brushes;
		byte bitMask;
		Brush bigBlock;
		bool regionBordercheck;
		float region;

		//Big Blocks and First Surface Layer
		for (posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					if (GetBitOfByte(bigBlockArray[posX, posY, posZ], 0))
                    {
                        newX = posX + (chunkSize * x / bigBlockSize);
                        newZ = posZ + (chunkSize * z / bigBlockSize);
                        
						//Regular Square "Big Blocks"
                        bigBlock = CreateBrush(
								new Vector3(posX * bigBlockSize, posY * bigBlockSize, posZ * bigBlockSize),
								new Vector3(bigBlockSize, bigBlockSize, bigBlockSize));
						bigBlock.hiddenFlag = CheckBrushVisibility(bigBlockArray, posX, posY, posZ, 0);
						bitMask = (byte)CheckSurfaceBrushType(bigBlockArray, posX, posY, posZ, 0);
						if ((bitMask & (1 << 1)) == 0 && (bitMask & (1 << 0)) != 0 && y >= 0)
						{
							region = GetClampedNoise(noiseC.GetNoise(posX + (chunkSize * x / bigBlockSize), posZ + (chunkSize * z / bigBlockSize)));
							regionBordercheck = false;
							if(region != GetClampedNoise(noiseC.GetNoise(newX - 1, newZ))||
								region != GetClampedNoise(noiseC.GetNoise(newX + 1, newZ))||
								region != GetClampedNoise(noiseC.GetNoise(newX, newZ - 1))||
								region != GetClampedNoise(noiseC.GetNoise(newX, newZ + 1)))
							{
								regionBordercheck = true;
							}
							if (!regionBordercheck)
							{
								bigBlock.textures = new uint[] { 4, 4, 4, 4, 4, 4 };
							}
							else
							{
								bigBlock.textures = new uint[] { 1, 1, 1, 1, 1, 1 };
							}
						}
						else
						{
							bigBlock.textures = new uint[] { 3, 3, 3, 3, 3, 3 };
						}

						chunk.brushes.Add(bigBlock);
						bigBlockBrushArray[posX, posY, posZ] = bigBlock;
					}
					else
					{
						//First layer of "Surface Brushes"
						bitMask = CheckSurfaceBrushType(bigBlockArray, posX, posY, posZ, 0);
						if (bitMask != 0)
						{
							brushes = CreateSurfaceBrushes(bitMask, (byte)(posX * bigBlockSize), (byte)(posY * bigBlockSize), (byte)(posZ * bigBlockSize), false, x,y, z);
							if (brushes != null)
							{
								SetBitOfByte(ref bigBlockArray[posX, posY, posZ], 1, true);
								chunk.brushes.AddRange(brushes);
							}

						}
					}
				}
			}
		}

		//Second Surface Layer & Visibility Assigning
		for (posX = 0; posX < chunkSize / bigBlockSize; posX++)
		{
			for (posY = 0; posY < chunkSize / bigBlockSize; posY++)
			{
				for (posZ = 0; posZ < chunkSize / bigBlockSize; posZ++)
				{
					if (bigBlockBrushArray[posX, posY, posZ] != null && bigBlockBrushArray[posX, posY, posZ].hiddenFlag)
					{
						if (posX - 1 >= 0 && bigBlockBrushArray[posX - 1, posY, posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX - 1, posY, posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX - 1, posY, posZ]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX - 1, posY, posZ]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}

						if (posY - 1 >= 0 && bigBlockBrushArray[posX, posY - 1, posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX, posY - 1, posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY - 1, posZ]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY - 1, posZ]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}

						if (posZ - 1 >= 0 && bigBlockBrushArray[posX, posY, posZ - 1] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX, posY, posZ - 1]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY, posZ - 1]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY, posZ - 1]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}

						if (posX + 1 < chunkSize && bigBlockBrushArray[posX + 1, posY, posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX + 1, posY, posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX + 1, posY, posZ]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX + 1, posY, posZ]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}

						if (posY + 1 < chunkSize && bigBlockBrushArray[posX, posY + 1, posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX, posY + 1, posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY + 1, posZ]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY + 1, posZ]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}

						if (posZ + 1 < chunkSize && bigBlockBrushArray[posX, posY, posZ + 1] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[posX, posY, posZ + 1]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY, posZ + 1]].Add(bigBlockBrushArray[posX, posY, posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[posX, posY, posZ + 1]] = new List<Brush>() { bigBlockBrushArray[posX, posY, posZ] }; }
						}
					}

					if (!GetBitOfByte(bigBlockArray[posX, posY, posZ], 1) && !GetBitOfByte(bigBlockArray[posX, posY, posZ], 0))
					{
						//Second layer of "Sub-Surface Brushes"
						bitMask = (byte)(CheckSurfaceBrushType(bigBlockArray, posX, posY, posZ, 0) | CheckSurfaceBrushType(bigBlockArray, posX, posY, posZ, 1));
						if (bitMask != 0)
						{
							brushes = CreateSurfaceBrushes(bitMask, (byte)(posX * bigBlockSize), (byte)(posY * bigBlockSize), (byte)(posZ * bigBlockSize), true,x,y,z);
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


	List<Brush> CreateSurfaceBrushes(byte id, byte posX, byte posY, byte posZ, bool subSurface, int x, int y, int z)
	{
		List<Brush> brushCopies = new List<Brush>();
		bool check;
		byte[] verts;
		if (subSurface)
		{
			check = subSurfaceBrushes.TryGetValue(id, out verts);
		}
		else
		{
			check = surfaceBrushes.TryGetValue(id, out verts);
		}

		if (check)
		{
			Brush b;
			float newX = (posX / bigBlockSize) + (chunkSize * x / bigBlockSize);
			float newZ = (posZ / bigBlockSize) + (chunkSize * z / bigBlockSize);

            float region;
			bool regionBordercheck;
			region = GetClampedNoise(noiseC.GetNoise(newX, newZ));
			regionBordercheck = false;
			if (region != GetClampedNoise(noiseC.GetNoise(newX - 1, newZ)) ||
				region != GetClampedNoise(noiseC.GetNoise(newX + 1, newZ)) ||
				region != GetClampedNoise(noiseC.GetNoise(newX, newZ - 1)) ||
				region != GetClampedNoise(noiseC.GetNoise(newX, newZ + 1)))
			{
				regionBordercheck = true;
			}

			for (int i = 0; i < verts.Length / 24; i++)
			{
				b = new Brush { hiddenFlag = false, vertices = new byte[24] };
				if ((id & (1 << 1)) == 0 && (id & (1 << 0)) != 0 && y >= 0)
				{
					if (!regionBordercheck)
					{
						b.textures = new uint[] { 4, 4, 4, 4, 4, 4 };
					}
					else
					{
						b.textures = new uint[] { 1, 1, 1, 1, 1, 1 };
					}
				}
				else
				{
					b.textures = new uint[] { 3, 3, 3, 3, 3, 3 };
				}
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

	byte CheckSurfaceBrushType(byte[,,] bigBlockArray, int x, int y, int z, int pos)
	{
		int bitmask = 0;
		//North
		if (z < bigBlockArray.GetLength(2) - 1 && GetBitOfByte(bigBlockArray[x, y, z + 1], pos))
		{
			bitmask |= 1 << 5;
		}
		//East
		if (x < bigBlockArray.GetLength(0) - 1 && GetBitOfByte(bigBlockArray[x + 1, y, z], pos))
		{
			bitmask |= 1 << 4;
		}
		//South
		if (z > 0 && GetBitOfByte(bigBlockArray[x, y, z - 1], pos))
		{
			bitmask |= 1 << 3;
		}
		//West
		if (x > 0 && GetBitOfByte(bigBlockArray[x - 1, y, z], pos))
		{
			bitmask |= 1 << 2;
		}
		//Top
		if (y < bigBlockArray.GetLength(1) - 1 && GetBitOfByte(bigBlockArray[x, y + 1, z], pos))
		{
			bitmask |= 1 << 1;
		}
		//Bottom
		if (y > 0 && GetBitOfByte(bigBlockArray[x, y - 1, z], pos))
		{
			bitmask |= 1 << 0;
		}
		return (byte)bitmask;
	}


	bool CheckBrushVisibility(byte[,,] bigBlockArray, int x, int y, int z, int pos)
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

		//If hidden
		return GetBitOfByte(bigBlockArray[x - 1, y, z], pos) &&
				GetBitOfByte(bigBlockArray[x + 1, y, z], pos) &&
				GetBitOfByte(bigBlockArray[x, y - 1, z], pos) &&
				GetBitOfByte(bigBlockArray[x, y + 1, z], pos) &&
				GetBitOfByte(bigBlockArray[x, y, z - 1], pos) &&
				GetBitOfByte(bigBlockArray[x, y, z + 1], pos);
	}

	bool CheckIndexInvalidity(int index, int length)
	{
		if (index == 0 || index == length - 1)
		{
			return true;
		}
		return false;
	}

	async Task<ChunkRenderData> GetChunkMeshAsync(Chunk chunkData)
	{
		return GetChunkMesh(chunkData);
	}

	ChunkRenderData GetChunkMesh(Chunk chunkData)
	{
		if (chunkData.brushes.Count == 0)
		{
			return null;
		}

		//Find just visible brushes
		List<int> visibleBrushes = new List<int>();

		for (int h = 0; h < chunkData.brushes.Count; h++)
		{
			if (!chunkData.brushes[h].hiddenFlag)
			{
				visibleBrushes.Add(h);
			}
		}

		if (visibleBrushes.Count == 0)
		{
			return null;
		}

		//Add all vertex data
		Vector3[] verts = new Vector3[visibleBrushes.Count * 36];
		int[] indices = new int[visibleBrushes.Count * 36];
		Vector3 vert;


		int visBrushIndiciesIndex;
		for (int h = 0; h < visibleBrushes.Count; h++)
		{
			for (visBrushIndiciesIndex = 0; visBrushIndiciesIndex < brushIndices.Length; visBrushIndiciesIndex++)
			{
				vert.X = chunkData.brushes[visibleBrushes[h]].vertices[brushIndices[visBrushIndiciesIndex] * 3];
				vert.Y = chunkData.brushes[visibleBrushes[h]].vertices[(brushIndices[visBrushIndiciesIndex] * 3) + 1];
				vert.Z = chunkData.brushes[visibleBrushes[h]].vertices[(brushIndices[visBrushIndiciesIndex] * 3) + 2];

				indices[(h * 36) + visBrushIndiciesIndex] = (h * 36) + visBrushIndiciesIndex;
				verts[(h * 36) + visBrushIndiciesIndex] = vert;
			}
		}

		//Setup normals
		Vector3[] normals = new Vector3[verts.Length];


		//Create a fast lookup table for adjacent triangles
		System.Collections.Generic.Dictionary<Vector3, List<int>> triangleAdjacencyList = new System.Collections.Generic.Dictionary<Vector3, List<int>>();
		Vector3 lookupVertex;
		int startIndex;
		for (int i = 0; i < indices.Length; i++)
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
		Vector3[] adjacentFaceNormals;
		System.Collections.Generic.Dictionary<int, List<int>> finalAdjacencyList;
		int k, j;
		Vector3 finalNormal;

		foreach ((Vector3 currentVert, List<int> adjacentTriangleIndices) in triangleAdjacencyList)
		{

			//Gather all face normals
			adjacentFaceNormals = new Vector3[adjacentTriangleIndices.Count / 3];
			for (int i = 0; i < adjacentTriangleIndices.Count; i += 3)
			{
				adjacentFaceNormals[i / 3] =
					(verts[indices[adjacentTriangleIndices[i]]] -
					verts[indices[adjacentTriangleIndices[i + 1]]]).Cross(verts[indices[adjacentTriangleIndices[i + 2]]] -
					verts[indices[adjacentTriangleIndices[i + 1]]]);
			}

			for (int i = 0; i < adjacentFaceNormals.Length; i++)
			{
				for (int e = 0; e < adjacentFaceNormals.Length; e++)
				{
					if (adjacentFaceNormals[i].Normalized().Dot(adjacentFaceNormals[e].Normalized()) > 0.45)
					{
						for (k = 0; k < 3; k++)
						{
							if (currentVert.Equals(verts[indices[adjacentTriangleIndices[(i * 3) + k]]]))
							{
								for (j = 0; j < 3; j++)
								{
									if (currentVert.Equals(verts[indices[adjacentTriangleIndices[(e * 3) + j]]]))
									{
										indices[adjacentTriangleIndices[(e * 3) + j]] = indices[adjacentTriangleIndices[(i * 3) + k]];
										break;
									}
								}
								break;
							}
						}

					}
				}
			}

			finalAdjacencyList = new System.Collections.Generic.Dictionary<int, List<int>>();
			for (int i = 0; i < adjacentTriangleIndices.Count; i++)
			{
				int lookupIndex = indices[adjacentTriangleIndices[i]];

				if (verts[lookupIndex].Equals(currentVert))
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
				finalNormal = Vector3.Zero;
				for (k = 0; k < value.Count; k++)
				{
					finalNormal += adjacentFaceNormals[value[k] / 3];
				}
				normals[key] = finalNormal.Normalized();
			}

		}

		//Split everything based on material
		System.Collections.Generic.Dictionary<uint, PreMesh> splitMeshes = new System.Collections.Generic.Dictionary<uint, PreMesh>();
		for (int i = 0; i < visibleBrushes.Count; i++)
		{
			for (int e = 0; e < 6; e++)
			{
				if (!splitMeshes.ContainsKey(chunkData.brushes[visibleBrushes[i]].textures[e]))
				{
					splitMeshes.Add(chunkData.brushes[visibleBrushes[i]].textures[e],
						new PreMesh()
						{
							vertices = new List<Vector3>(),
							indices = new List<int>(),
							normals = new List<Vector3>(),
							brushIndexes = new List<int>()
						});
				}
			}
		}

		//Assign the surface to a mesh and return
		Node3D chunk = new Node3D();
		ArrayMesh arrMesh = new ArrayMesh();
		uint[] matIDs = new uint[splitMeshes.Count];
		List<int> triangletoBrushIndex = new List<int>();


		for (int i = 0; i < visibleBrushes.Count; i++)
		{
			for (int e = 0; e < 6; e++)
			{
				int temp = (i * 36) + (e * 6);
				if (splitMeshes.TryGetValue(chunkData.brushes[visibleBrushes[i]].textures[e], out PreMesh preMesh))
				{
					for (int o = 0; o < 6; o++)
					{
						preMesh.indices.Add(preMesh.vertices.Count);
						preMesh.vertices.Add(verts[indices[temp + o]]);
						preMesh.normals.Add(normals[indices[temp + o]]);
					}
					preMesh.brushIndexes.Add(visibleBrushes[i]);
					preMesh.brushIndexes.Add(visibleBrushes[i]);
				}
			}
		}


		int currentPremesh = 0;
		Godot.Collections.Array surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);
		foreach ((uint key, PreMesh value) in splitMeshes)
		{
			// Convert Lists to arrays and assign to surface array
			surfaceArray[(int)Mesh.ArrayType.Vertex] = value.vertices.ToArray();
			surfaceArray[(int)Mesh.ArrayType.TexUV] = new Vector2[value.vertices.Count];
			surfaceArray[(int)Mesh.ArrayType.Normal] = value.normals.ToArray();
			surfaceArray[(int)Mesh.ArrayType.Index] = value.indices.ToArray();
			arrMesh.AddSurfaceFromArrays(Mesh.PrimitiveType.Triangles, surfaceArray);
			matIDs[currentPremesh] = key;
			currentPremesh++;
			triangletoBrushIndex.AddRange(value.brushIndexes);
		}

		MeshInstance3D meshObject = new MeshInstance3D();
		meshObject.Mesh = arrMesh;
		for (int i = 0; i < matIDs.Length; i++)
		{
			meshObject.Mesh.SurfaceSetMaterial(i, mats[(int)matIDs[i]]);
		}

		//Assign Collision
		ConcavePolygonShape3D collisionMesh = new ConcavePolygonShape3D();
		collisionMesh.Data = arrMesh.GetFaces();

		CollisionShape3D collisionShape = new CollisionShape3D();
		collisionShape.Shape = collisionMesh;

		StaticBody3D body = new StaticBody3D();
		body.CollisionLayer = 0b00000000_00000000_00000000_00000101; //Default, Brushes


		return new ChunkRenderData()
		{
			id = chunkData.id,
			state = ChunkRenderDataState.ready,
			chunkNode = chunk,
			meshNode = meshObject,
			position = new Vector3(chunkData.positionX, chunkData.positionY, chunkData.positionZ),
			collisionShape = collisionShape,
			staticBody = body,
			chunk = chunkData,
			visibleBrushIndices = triangletoBrushIndex
		};
	}

	Brush CreateBrush(Vector3 pos, Vector3 size)
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
		brush.textures = new uint[] { 0, 0, 0, 0, 0, 0 };
		return brush;
	}

	float GetClampedNoise(float noise)
	{
		return (noise + 1.0f) / 2.0f;
	}

	float GetClampedChunkRange(float lowerBound, float upperBound, float yPos)
	{
		return Mathf.Clamp((yPos - lowerBound) / (upperBound - lowerBound), 0, 1);
	}

	void SetBitOfByte(ref byte aByte, int pos, bool value)
	{
		if (value)
		{
			//left-shift 1, then bitwise OR
			aByte = (byte)(aByte | (1 << pos));
		}
		else
		{
			//left-shift 1, then take complement, then bitwise AND
			aByte = (byte)(aByte & ~(1 << pos));
		}
	}

	bool GetBitOfByte(byte aByte, int pos)
	{
		//left-shift 1, then bitwise AND, then check for non-zero
		return ((aByte & (1 << pos)) != 0);
	}

	public void DestroyBlock(Node3D chunkNode, int brushID)
	{
		if (!firstChunkLoaded)
		{
			return;
		}
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			if (loadedChunks[i].node == chunkNode)
			{
				if (loadedChunks[i].chunk.connectedInvisibleBrushes.TryGetValue(loadedChunks[i].chunk.brushes[loadedChunks[i].visibleBrushIndices[brushID]], out List<Brush> updateBrushes))
				{
					foreach (Brush pendingBrush in updateBrushes)
					{
						pendingBrush.hiddenFlag = false;
					}

					loadedChunks[i].chunk.connectedInvisibleBrushes.Remove(loadedChunks[i].chunk.brushes[loadedChunks[i].visibleBrushIndices[brushID]]);
				}
				loadedChunks[i].chunk.brushes.RemoveAt(loadedChunks[i].visibleBrushIndices[brushID]);
				RerenderLoadedChunk(loadedChunks[i]);
				return;
			}
		}
	}

	void RerenderLoadedChunk(LoadedChunkData chunk)
	{
		//Generate Mesh
		ChunkRenderData chunkData = GetChunkMesh(chunk.chunk);
		if (chunkData == null)
		{
			return;
		}
		MeshInstance3D meshNode = chunk.node.GetChild(0) as MeshInstance3D;
		meshNode.Mesh = chunkData.meshNode.Mesh;
		(meshNode.GetChild(0).GetChild(0) as CollisionShape3D).Shape = chunkData.collisionShape.Shape;//THIS WILL BREAK WITH MORE CHILD SHAPES
		chunk.visibleBrushIndices = chunkData.visibleBrushIndices;

		GD.Print("Regenerated Chunk (ID " + chunkData.id + ")");
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
		public uint[] textures = new uint[] { 0, 0, 0, 0, 0, 0 };
		public bool hiddenFlag;
	}

	class PreMesh
	{
		public List<Vector3> vertices;
		public List<int> indices;
		public List<Vector3> normals;
		public List<int> brushIndexes;
	}

	public class Chunk
	{
		public int id;
		public int positionX;
		public int positionY;
		public int positionZ;
		public List<Brush> brushes;
		public System.Collections.Generic.Dictionary<Brush, List<Brush>> connectedInvisibleBrushes;
	}

	public class ChunkRenderData
	{
		public ChunkRenderDataState state;
		public Node3D chunkNode;
		public MeshInstance3D meshNode;
		public Vector3 position;
		public int id;
		public CollisionShape3D collisionShape;
		public StaticBody3D staticBody;
		public List<int> visibleBrushIndices;
		public Chunk chunk;
	}

	public class LoadedChunkData
	{
		public Node3D node;
		public Vector3 position;
		public int id;
		public List<int> visibleBrushIndices;
		public Chunk chunk;
	}

	public enum ChunkRenderDataState
	{
		running,
		ready,
		garbageCollector,
	}

	System.Collections.Generic.Dictionary<int, byte[]> subSurfaceBrushes = new System.Collections.Generic.Dictionary<int, byte[]>
	{
		{ 0b110011,new byte[]{ //North-East Crushed Connection
					3,0,1, 6,0,0, 6,0,6, 5,0,5,
					3,6,1, 6,6,0, 6,6,6, 5,6,5,

					3,0,1, 5,0,5, 5,0,5, 1,0,3,
					3,6,1, 5,6,5, 5,6,5, 1,6,3,

					1,0,3, 5,0,5, 6,0,6, 0,0,6,
					1,6,3, 5,6,5, 6,6,6, 0,6,6,
		}},
		{ 0b011011,new byte[]{ //East-South Crushed Connection
					0,0,0, 6,0,0, 5,0,1, 1,0,3,
					0,6,0, 6,6,0, 5,6,1, 1,6,3,

					1,0,3, 5,0,1, 5,0,1, 3,0,5,
					1,6,3, 5,6,1, 5,6,1, 3,6,5,

					5,0,1, 6,0,0, 6,0,6, 3,0,5,
					5,6,1, 6,6,0, 6,6,6, 3,6,5,
		}},
		{ 0b001111,new byte[]{ //South-West Crushed Connection
					0,0,0, 6,0,0, 5,0,3, 1,0,1,
					0,6,0, 6,6,0, 5,6,3, 1,6,1,

					1,0,1, 1,0,1, 5,0,3, 3,0,5,
					1,6,1, 1,6,1, 5,6,3, 3,6,5,

					0,0,0, 1,0,1, 3,0,5, 0,0,6,
					0,6,0, 1,6,1, 3,6,5, 0,6,6,
		}},
		{ 0b100111,new byte[]{ //West-North Crushed Connection
					
					0,0,0, 3,0,1, 1,0,5, 0,0,6,
					0,6,0, 3,6,1, 1,6,5, 0,6,6,

					3,0,1, 5,0,3, 1,0,5, 1,0,5,
					3,6,1, 5,6,3, 1,6,5, 1,6,5,

					1,0,5, 5,0,3, 6,0,6, 0,0,6,
					1,6,5, 5,6,3, 6,6,6, 0,6,6,
		}},
		{ 0b110001,new byte[]{ //North-East Bottom Corner Connection
					
					3,0,1, 6,0,0, 6,0,2, 4,0,3,
					3,0,1, 6,0,0, 6,1,2, 4,1,3,

					3,0,1, 4,0,3, 3,0,4, 1,0,3,
					3,0,1, 4,1,3, 3,1,4, 1,0,3,

					1,0,3, 3,0,4, 2,0,6, 0,0,6,
					1,0,3, 3,1,4, 2,1,6, 0,0,6,

					4,0,3, 6,0,2, 6,2,4, 3,0,4,
					4,1,3, 6,1,2, 6,4,3, 3,1,4,

					3,0,4, 6,0,2, 6,0,6, 2,0,6,
					3,1,4, 6,2,4, 6,2,6, 2,1,6,

					3,1,4, 6,2,4, 6,2,6, 2,1,6,
					6,4,3, 6,4,3, 6,4,6, 3,4,6,

					3,4,6, 6,4,3, 6,4,6, 6,4,6,
					4,5,6, 6,5,4, 6,6,6, 6,6,6,
		}},
		{ 0b110101,new byte[]{ //North Ramp 3-Direction Smushed
					0,0,0, 6,0,0, 6,0,3, 0,0,3,
					0,1,2, 6,1,2, 6,1,2, 0,1,2,

					0,0,3, 6,0,3, 6,0,6, 0,0,6,
					0,1,2, 6,1,2, 6,4,3, 0,4,3,

					0,4,3, 6,4,3, 6,0,6, 0,0,6,
					0,5,4, 6,5,4, 6,6,6, 0,6,6,
		}},
		{ 0b111001,new byte[]{ //East Ramp 3-Direction Smushed
					0,0,0, 3,0,0, 3,0,6, 0,0,6,
					2,1,0, 2,1,0, 2,1,6, 2,1,6,

					3,0,0, 6,0,0, 6,0,6, 3,0,6,
					2,1,0, 3,4,0, 3,4,6, 2,1,6,

					3,4,0, 6,0,0, 6,0,6, 3,4,6,
					4,5,0, 6,6,0, 6,6,6, 4,5,6,
		}},
		{ 0b011101,new byte[]{ //South Ramp 3-Direction Smushed

					0,0,3, 6,0,3, 6,0,6, 0,0,6,
					0,1,4, 6,1,4, 6,1,4, 0,1,4,

					0,0,0, 6,0,0, 6,0,3, 0,0,3,
					0,4,3, 6,4,3, 6,1,4, 0,1,4,

					0,0,0, 6,0,0, 6,4,3, 0,4,3,
					0,6,0, 6,6,0, 6,5,2, 0,5,2,
		}},
		{ 0b101101,new byte[]{ //West Ramp 3-Direction Smushed
					3,0,0, 6,0,0, 6,0,6, 3,0,6,
					4,1,0, 4,1,0, 4,1,6, 4,1,6,

					0,0,0, 3,0,0, 3,0,6, 0,0,6,
					3,4,0, 4,1,0, 4,1,6, 3,4,6,

					0,0,0, 3,4,0, 3,4,6, 0,0,6,
					0,6,0, 2,5,0, 2,5,6, 0,6,6,
		}},
	};

	System.Collections.Generic.Dictionary<int, byte[]> surfaceBrushes = new System.Collections.Generic.Dictionary<int, byte[]>
	{
		{ 0b100001,new byte[]{ //North Ramp
					0,0,0, 6,0,0, 6,0,3, 0,0,3,
					0,1,2, 6,1,2, 6,1,2, 0,1,2,

					0,0,3, 6,0,3, 6,0,6, 0,0,6,
					0,1,2, 6,1,2, 6,4,3, 0,4,3,

					0,4,3, 6,4,3, 6,0,6, 0,0,6,
					0,5,4, 6,5,4, 6,6,6, 0,6,6,
		}},
		{ 0b010001,new byte[]{ //East Ramp
					0,0,0, 3,0,0, 3,0,6, 0,0,6,
					2,1,0, 2,1,0, 2,1,6, 2,1,6,

					3,0,0, 6,0,0, 6,0,6, 3,0,6,
					2,1,0, 3,4,0, 3,4,6, 2,1,6,

					3,4,0, 6,0,0, 6,0,6, 3,4,6,
					4,5,0, 6,6,0, 6,6,6, 4,5,6,
		}},
		{ 0b001001,new byte[]{ //South Ramp

					0,0,3, 6,0,3, 6,0,6, 0,0,6,
					0,1,4, 6,1,4, 6,1,4, 0,1,4,

					0,0,0, 6,0,0, 6,0,3, 0,0,3,
					0,4,3, 6,4,3, 6,1,4, 0,1,4,

					0,0,0, 6,0,0, 6,4,3, 0,4,3,
					0,6,0, 6,6,0, 6,5,2, 0,5,2,
		}},
		{ 0b000101,new byte[]{ //West Ramp
					3,0,0, 6,0,0, 6,0,6, 3,0,6,
					4,1,0, 4,1,0, 4,1,6, 4,1,6,

					0,0,0, 3,0,0, 3,0,6, 0,0,6,
					3,4,0, 4,1,0, 4,1,6, 3,4,6,

					0,0,0, 3,4,0, 3,4,6, 0,0,6,
					0,6,0, 2,5,0, 2,5,6, 0,6,6,
		}},
		{ 0b100010,new byte[]{ //Upside-Down North Ramp
					0,5,2, 6,5,2, 6,5,2, 0,5,2,
					0,6,0, 6,6,0, 6,6,3, 0,6,3,

					0,5,2, 6,5,2, 6,2,3, 0,2,3,
					0,6,3, 6,6,3, 6,6,6, 0,6,6,

					0,1,4, 6,1,4, 6,0,6, 0,0,6,
					0,2,3, 6,2,3, 6,6,6, 0,6,6,
		}},
		{ 0b010010, new byte[]{ //Upside-Down East Ramp
					2,5,0, 2,5,0, 2,5,6, 2,5,6,
					0,6,0, 3,6,0, 3,6,6, 0,6,6,

					2,5,0, 3,2,0, 3,2,6, 2,5,6,
					3,6,0, 6,6,0, 6,6,6, 3,6,6,

					4,1,0, 6,0,0, 6,0,6, 4,1,6,
					3,2,0, 6,6,0, 6,6,6, 3,2,6,
		}},
		{ 0b001010, new byte[]{//Upside-Down South Ramp
					0,5,4, 6,5,4, 6,5,4, 0,5,4,
					0,6,3, 6,6,3, 6,6,6, 0,6,6,

					0,2,3, 6,2,3, 6,5,4, 0,5,4,
					0,6,0, 6,6,0, 6,6,3, 0,6,3,

					0,0,0, 6,0,0, 6,1,2, 0,1,2,
					0,6,0, 6,6,0, 6,2,3, 0,2,3,
		} },
		{ 0b000110, new byte[]{//Upside-Down West Ramp
					4,5,0, 4,5,0, 4,5,6, 4,5,6,
					3,6,0, 6,6,0, 6,6,6, 3,6,6,

					3,2,0, 4,5,0, 4,5,6, 3,2,6,
					0,6,0, 3,6,0, 3,6,6, 0,6,6,

					0,0,0, 2,1,0, 2,1,6, 0,0,6,
					0,6,0, 3,2,0, 3,2,6, 0,6,6,
		}},
		{ 0b110000,new byte[]{ //North-East Connection
					3,0,1, 6,0,0, 6,0,6, 5,0,5,
					3,6,1, 6,6,0, 6,6,6, 5,6,5,

					3,0,1, 5,0,5, 5,0,5, 1,0,3,
					3,6,1, 5,6,5, 5,6,5, 1,6,3,

					1,0,3, 5,0,5, 6,0,6, 0,0,6,
					1,6,3, 5,6,5, 6,6,6, 0,6,6,
		}},
		{ 0b011000,new byte[]{ //East-South Connection
					0,0,0, 6,0,0, 5,0,1, 1,0,3,
					0,6,0, 6,6,0, 5,6,1, 1,6,3,

					1,0,3, 5,0,1, 5,0,1, 3,0,5,
					1,6,3, 5,6,1, 5,6,1, 3,6,5,

					5,0,1, 6,0,0, 6,0,6, 3,0,5,
					5,6,1, 6,6,0, 6,6,6, 3,6,5,
		}},
		{ 0b001100,new byte[]{ //South-West Connection
					0,0,0, 6,0,0, 5,0,3, 1,0,1,
					0,6,0, 6,6,0, 5,6,3, 1,6,1,

					1,0,1, 1,0,1, 5,0,3, 3,0,5,
					1,6,1, 1,6,1, 5,6,3, 3,6,5,

					0,0,0, 1,0,1, 3,0,5, 0,0,6,
					0,6,0, 1,6,1, 3,6,5, 0,6,6,
		}},
		{ 0b100100,new byte[]{ //West-North Connection
					
					0,0,0, 3,0,1, 1,0,5, 0,0,6,
					0,6,0, 3,6,1, 1,6,5, 0,6,6,

					3,0,1, 5,0,3, 1,0,5, 1,0,5,
					3,6,1, 5,6,3, 1,6,5, 1,6,5,

					1,0,5, 5,0,3, 6,0,6, 0,0,6,
					1,6,5, 5,6,3, 6,6,6, 0,6,6,
		}},

	};
}
