using Godot;
using Godot.Collections;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using static WorldGen;
using media.Laura.SofiaConsole;
using Console = media.Laura.SofiaConsole.Console;


public partial class WorldGen : Node3D
{
	//Exports
	[Export] bool hideBigBlocks = false;
	[Export] EnvironmentController envController;
	[Export] Curve curve1;
	[Export] Curve curve2;
	[Export] Curve curve3;
	[Export] Curve curve4;
	[Export] Curve curve5;
	[Export] Curve curve6;
	[Export] Curve curve7;
	[Export] public GpuParticles3D destroyBrushParticles;

	//Globals
	public static int seedA, seedB, seedC, seedD, seedE, seedF, seedG;
	public static int totalChunksRendered = 0;
	public static bool firstChunkLoaded;

	//Locals
	Vector3 oldChunkPos = new Vector3(float.MinValue, float.MinValue, float.MinValue);
	bool lastFrameMaxChunkLimitReached;
	List<LoadedChunkData> loadedChunks = new List<LoadedChunkData>();
	List<ChunkRenderData> ongoingChunkRenderData = new List<ChunkRenderData>();
	Material[] mats;
	ServerClient server;
	FastNoiseContainer noise, noiseB, noiseC, noiseD, noiseE, noiseF, noiseG;
	int maxChunksLoadingRampUp = 1;

	//Consts
	public const int chunkLoadingDistance = 8;
	public const int chunkUnloadingDistance = 11;
	const int bigBlockSize = 6;
	public const int chunkSize = 84;
	public const int chunkMarginSize = 86; //(256 - Chunksize) / 2. 
	const int maxChunksLoading = 24;
	readonly byte[] brushIndices = new byte[36]
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
		server = GetTree().Root.GetNode("World/Server") as ServerClient;

		seedA = Convert.ToInt32(PlayerPrefs.GetString("Seed"));
		Random rnd = new Random(seedA);
		seedB = rnd.Next();
		seedC = rnd.Next();
		seedD = rnd.Next();
		seedE = rnd.Next();
		seedF = rnd.Next();
		seedG = rnd.Next();

		mats = new Material[9];
		for (int i = 0; i < 9; i++)
		{
			mats[i] = GD.Load("res://Materials/" + i + ".tres") as Material;
		}


		noise = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("DgAFAAAAAAAAQAsAAQAAAAAAAAABAAAAAAAAAACamZk/AAAAAD8AAAAAAAAAAABA"),
			frequency = 0.002f,
			seed = seedA,
		};
		//noise.SetDomainWarpType(FastNoise.DomainWarpType.OpenSimplex2);
		//noise.SetDomainWarpAmp(400);

		noiseB = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("KQA="),
			frequency = 0.06f,
			seed = seedB,
		};
		//noiseB.SetDomainWarpType(FastNoise.DomainWarpType.OpenSimplex2);
		//noiseB.SetDomainWarpAmp(400);


		noiseC = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("CgACAAAAAAAAAAAAAMA/"),
			frequency = 0.005f,
			seed = seedC,
		};
		//noiseC.SetCellularDistanceFunction(FastNoise.CellularDistanceFunction.Manhattan);
		//noiseC.SetCellularReturnType(FastNoise.CellularReturnType.CellValue);


		noiseD = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("DQAFAAAAAAAAQAkAAAAAAD8AAAAAAA=="),
			frequency = 0.002f,
			seed = seedD,
		};
		//noiseD.SetFractalType(FastNoise.FractalType.FBm);
		//noiseD.Set("Octaves", 5);


		noiseE = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("DQAEAAAAAAAAQAkAAAAAAD8AAAAAAA=="),
			frequency = 0.0005f,
			seed = seedE,
		};
		//noiseE.SetFractalType(FastNoise.FractalType.FBm);
		//noiseE.Set("Octaves", 4);


		noiseF = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("DQAEAAAAAAAAQAkAAAAAAD8AAAAAAA=="),
			frequency = 0.001f,
			seed = seedF,
		};
		//noiseF.SetFractalType(FastNoise.FractalType.FBm);
		//noiseF.Set("Octaves", 4);


		noiseG = new FastNoiseContainer(){
			noise = FastNoise.FromEncodedNodeTree("DwACAAAAAAAAQAkAAAAAAD8AAAAAAA=="),
			frequency = 0.0005f,
			seed = seedG,
		};
		//noiseG.SetFractalType(FastNoise.FractalType.Ridged);
		//noiseG.Set("Octaves", 2);
	}

	public override void _Process(double delta)
	{
		CheckForAnyPendingFinishedChunks();
		LoadChunks();
		UnloadChunks();
	}

	void LoadChunks()
	{
		if(server.GetMainPlayer() == null)
		{
			return;
		}

		//Check
		Vector3 chunkPos = new Vector3(Mathf.RoundToInt(server.GetMainPlayer().GlobalPosition.X / chunkSize), Mathf.RoundToInt(server.GetMainPlayer().GlobalPosition.Y / chunkSize), Mathf.RoundToInt(server.GetMainPlayer().GlobalPosition.Z / chunkSize));
		if (chunkPos == oldChunkPos && !lastFrameMaxChunkLimitReached)
		{
			return;
		}
		oldChunkPos = chunkPos;
		lastFrameMaxChunkLimitReached = false;

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
					temp.X = (int)chunkPos.X + x;
					temp.Y = (int)chunkPos.Y + y;
					temp.Z = (int)chunkPos.Z + z;

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

		int loadedChunksVar = ongoingChunkRenderData.Count;
		foreach (Vector3 chunk in sortedPosition)
		{
			if (loadedChunksVar >= maxChunksLoadingRampUp)
			{
				lastFrameMaxChunkLimitReached = true;
				break;
			}

			RenderChunk((int)chunk.X, (int)chunk.Y, (int)chunk.Z);
			loadedChunksVar++;
		}
	}

	void UnloadChunks()
	{		
		if(server.GetMainPlayer == null)
		{
			return;
		}
		//Unload Far Away Chunks
		for (int i = loadedChunks.Count - 1; i > -1; i--)
		{
			if (oldChunkPos.DistanceTo(loadedChunks[i].position) >= chunkUnloadingDistance)
			{
				if (loadedChunks[i].node != null)
				{
					loadedChunks[i].node.QueueFree();
				}
				loadedChunks.RemoveAt(i);
			}
		}
	}

	void CheckForAnyPendingFinishedChunks()
	{
		if (ongoingChunkRenderData.Count == 0)
		{
			return;
		}
		for (int i = ongoingChunkRenderData.Count - 1; i > -1; i--)
		{
			if (ongoingChunkRenderData[i].state == ChunkRenderDataState.garbageCollector)
			{
				ongoingChunkRenderData.RemoveAt(i);
			}
		}

		bool check = false;
		for (int e = 0; e < ongoingChunkRenderData.Count; e++)
		{
			if (ongoingChunkRenderData[e].state == ChunkRenderDataState.ready)
			{
				ongoingChunkRenderData[e].state = ChunkRenderDataState.garbageCollector;

				if (ongoingChunkRenderData[e].chunkNode != null)
				{
					AddChild(ongoingChunkRenderData[e].chunkNode);
					ongoingChunkRenderData[e].chunkNode.Name = "Chunk " + ongoingChunkRenderData[e].id.ToString();
					ongoingChunkRenderData[e].chunkNode.AddChild(ongoingChunkRenderData[e].meshNode);
					ongoingChunkRenderData[e].chunkNode.GlobalPosition = new Vector3((ongoingChunkRenderData[e].position.X * chunkSize) - chunkMarginSize, (ongoingChunkRenderData[e].position.Y * chunkSize) - chunkMarginSize, (ongoingChunkRenderData[e].position.Z * chunkSize) - chunkMarginSize);
					ongoingChunkRenderData[e].meshNode.AddChild(ongoingChunkRenderData[e].staticBody);
					ongoingChunkRenderData[e].staticBody.AddChild(ongoingChunkRenderData[e].collisionShape);

					loadedChunks.Add(new LoadedChunkData()
					{
						id = ongoingChunkRenderData[e].id,
						node = ongoingChunkRenderData[e].chunkNode,
						position = ongoingChunkRenderData[e].position,
						chunk = ongoingChunkRenderData[e].chunk,
						triangleIndexToBrushIndex = ongoingChunkRenderData[e].triangleIndexToBrushIndex,
						triangleIndexToBrushTextureIndex = ongoingChunkRenderData[e].triangleIndexToBrushTextureIndex
					});
				}
				else
				{
					loadedChunks.Add(new LoadedChunkData()
					{
						id = ongoingChunkRenderData[e].id,
						position = ongoingChunkRenderData[e].position,
						chunk = ongoingChunkRenderData[e].chunk,
					});
				}
			}
			if (ongoingChunkRenderData[e].state == ChunkRenderDataState.running)
			{
				check = true;
			}
		}
		if (!check)
		{
			if (!lastFrameMaxChunkLimitReached)
			{
				firstChunkLoaded = true;
			}
			else if (maxChunksLoadingRampUp != maxChunksLoading)
			{
				maxChunksLoadingRampUp = Mathf.Min(maxChunksLoadingRampUp + 1, maxChunksLoading);
			}
			ongoingChunkRenderData = new List<ChunkRenderData>();
		}
	}

	void RenderChunk(int x, int y, int z)
	{
		totalChunksRendered++;
		int id = totalChunksRendered;
		ongoingChunkRenderData.Add(new ChunkRenderData() { state = ChunkRenderDataState.running, id = id, position = new Vector3(x, y, z) });

		Task.Run(() =>
		{
			Chunk chunk = GenerateChunk(x, y, z, id);

			ChunkRenderData chunkData = GetChunkMesh(chunk);

			bool check = false;
			for (int i = 0; i < ongoingChunkRenderData.Count; i++)
			{
				if (ongoingChunkRenderData[i].id == id)
				{
					if (chunkData == null)
					{
						ongoingChunkRenderData[i].state = ChunkRenderDataState.garbageCollector;
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
				GD.PrintErr("Chunk missing ID in ongoing chunk pool. (ID " + id + ")");
				firstChunkLoaded = true; //Bandaid fix, please figure out why chunks are missing IDs
			}

		});
	}

	public struct PreGenNoiseValues
	{
		public float oceanMultiplier;
		public float noiseDSampled;
		public float noiseESampled;
		public int posX, posY, posZ, newX, newY, newZ, chunkY;
		public bool regionBordercheck, regionBorderCornercheck;
		public float region;
		public float biome;
	}

	//Chunks are 126x126x126
	Chunk GenerateChunk(int x, int y, int z, int id)
	{

		Chunk chunk = new Chunk();
		chunk.id = id;
		chunk.hasGeneratedBorders = false;
		chunk.positionX = x;
		chunk.positionY = y;
		chunk.positionZ = z;
		chunk.brushes = new List<Brush>();
		chunk.connectedInvisibleBrushes = new System.Collections.Generic.Dictionary<Brush, List<Brush>>();

		//BlockArray Setup
		byte[,,] bigBlockArray = new byte[chunkSize / bigBlockSize, chunkSize / bigBlockSize, chunkSize / bigBlockSize];
		Brush[,,] bigBlockBrushArray = new Brush[chunkSize / bigBlockSize, chunkSize / bigBlockSize, chunkSize / bigBlockSize];


		PreGenNoiseValues preGen = new PreGenNoiseValues();
		preGen.chunkY = y;

		for (preGen.posX = 0; preGen.posX < chunkSize / bigBlockSize; preGen.posX++)
		{					
			preGen.newX = preGen.posX + (chunkSize * x / bigBlockSize);
			
			for (preGen.posZ = 0; preGen.posZ < chunkSize / bigBlockSize; preGen.posZ++)
			{
				preGen.newZ = preGen.posZ + (chunkSize * z / bigBlockSize);

				PregenNoiseValues(ref preGen);
				
				for (preGen.posY = 0; preGen.posY < chunkSize / bigBlockSize; preGen.posY++)
				{
					preGen.newY = preGen.posY + (chunkSize * y / bigBlockSize);
					if (CheckBigBlock(ref preGen))
					{
						SetBitOfByte(ref bigBlockArray[preGen.posX, preGen.posY, preGen.posZ], 0, true);
					}
				}
			}
		}

		Brush[] brushes;
		byte bitMask;
		Brush bigBlock;
		bool isSurface;

		//Big Blocks and First Surface Layer
		for (preGen.posX = 0; preGen.posX < chunkSize / bigBlockSize; preGen.posX++)
		{						
			preGen.newX = preGen.posX + (chunkSize * x / bigBlockSize);

			for (preGen.posZ = 0; preGen.posZ < chunkSize / bigBlockSize; preGen.posZ++)
			{
				preGen.newZ = preGen.posZ + (chunkSize * z / bigBlockSize);

				PregenNoiseValues(ref preGen);
				//Find Values
				preGen.biome = GetClampedFastNoise2D(noiseF,preGen.newX,preGen.newZ);
				preGen.region = GetClampedFastNoise2D(noiseC,preGen.newX, preGen.newZ);
				preGen.regionBordercheck = FindIfRoadBlock(ref preGen);
				preGen.regionBorderCornercheck = FindIfCornerRoadBlock(ref preGen);

				for (preGen.posY = 0; preGen.posY < chunkSize / bigBlockSize; preGen.posY++)
				{
					if (GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ], 0))
					{

						//Regular Square "Big Blocks"
						bigBlock = CreateBrush(
								new Vector3((preGen.posX * bigBlockSize) + chunkMarginSize, (preGen.posY * bigBlockSize) + chunkMarginSize, (preGen.posZ * bigBlockSize) + chunkMarginSize),
								new Vector3(bigBlockSize, bigBlockSize, bigBlockSize));
						bigBlock.hiddenFlag = CheckBrushVisibility(ref bigBlockArray, 0, ref preGen);
						bigBlock.borderFlag = CheckBrushOnBorder(ref preGen);
						bitMask = (byte)CheckSurfaceBrushType(bigBlockArray, 0, ref preGen);

						isSurface = (bitMask & (1 << 1)) == 0 && (bitMask & (1 << 0)) != 0 && y >= -1;

						//Assign textures
						bigBlock.textures = FindTextureOfGeneratingBrush(isSurface, ref preGen);

						chunk.brushes.Add(bigBlock);
						bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] = bigBlock;
					}
					else
					{
						//First layer of "Surface Brushes"
						bitMask = CheckSurfaceBrushType(bigBlockArray, 0, ref preGen);
						if (bitMask != 0)
						{

								brushes = CreateSurfaceBrushes(bitMask, false, ref preGen);
								if (brushes != null)
								{
									SetBitOfByte(ref bigBlockArray[preGen.posX, preGen.posY, preGen.posZ], 1, true);
									chunk.brushes.AddRange(brushes);
								}

						}
					}
				}
			}
		}

		//Second Surface Layer & Visibility Assigning
		for (preGen.posX = 0; preGen.posX < chunkSize / bigBlockSize; preGen.posX++)
		{
			preGen.newX = preGen.posX + (chunkSize * x / bigBlockSize);

			for (preGen.posZ = 0; preGen.posZ < chunkSize / bigBlockSize; preGen.posZ++)
			{
				preGen.newZ = preGen.posZ + (chunkSize * z / bigBlockSize);

				PregenNoiseValues(ref preGen);

				for (preGen.posY = 0; preGen.posY < chunkSize / bigBlockSize; preGen.posY++)
				{

					if (bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] != null && (bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ].hiddenFlag || bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ].borderFlag))
					{
						if (preGen.posX - 1 >= 0 && bigBlockBrushArray[preGen.posX - 1, preGen.posY, preGen.posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX - 1, preGen.posY, preGen.posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX - 1, preGen.posY, preGen.posZ]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX - 1, preGen.posY, preGen.posZ]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}

						if (preGen.posY - 1 >= 0 && bigBlockBrushArray[preGen.posX, preGen.posY - 1, preGen.posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX, preGen.posY - 1, preGen.posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY - 1, preGen.posZ]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY - 1, preGen.posZ]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}

						if (preGen.posZ - 1 >= 0 && bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ - 1] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ - 1]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ - 1]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ - 1]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}

						if (preGen.posX + 1 < chunkSize / bigBlockSize && bigBlockBrushArray[preGen.posX + 1, preGen.posY, preGen.posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX + 1, preGen.posY, preGen.posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX + 1, preGen.posY, preGen.posZ]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX + 1, preGen.posY, preGen.posZ]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}

						if (preGen.posY + 1 < chunkSize / bigBlockSize && bigBlockBrushArray[preGen.posX, preGen.posY + 1, preGen.posZ] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX, preGen.posY + 1, preGen.posZ]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY + 1, preGen.posZ]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY + 1, preGen.posZ]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}

						if (preGen.posZ + 1 < chunkSize / bigBlockSize && bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ + 1] != null)
						{
							if (chunk.connectedInvisibleBrushes.ContainsKey(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ + 1]))
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ + 1]].Add(bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ]); }
							else
							{ chunk.connectedInvisibleBrushes[bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ + 1]] = new List<Brush>() { bigBlockBrushArray[preGen.posX, preGen.posY, preGen.posZ] }; }
						}
					}

					if (!GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ], 1) && !GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ], 0))
					{
						//Second layer of "Sub-Surface Brushes"
						bitMask = (byte)(CheckSurfaceBrushType(bigBlockArray, 0, ref preGen) | CheckSurfaceBrushType(bigBlockArray, 1, ref preGen));
						if (bitMask != 0)
						{
							brushes = CreateSurfaceBrushes(bitMask, true, ref preGen);
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
	
	void PregenNoiseValues(ref PreGenNoiseValues preGen)
	{
		//Pregenerate values independent of Y level
		preGen.oceanMultiplier = (curve7.SampleBaked(GetClampedFastNoise2D(noiseG,preGen.newX, preGen.newZ)) * 2) - 1;
		preGen.noiseDSampled = curve4.SampleBaked(GetClampedFastNoise2D(noiseD,preGen.newX, preGen.newZ));
		preGen.noiseESampled = curve5.SampleBaked(GetClampedFastNoise2D(noiseE,preGen.newX, preGen.newZ));
	}

	bool FindIfRoadBlock(ref PreGenNoiseValues preGen)
	{
		if (preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX - 1, preGen.newZ) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX + 1, preGen.newZ) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX, preGen.newZ - 1) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX, preGen.newZ + 1))
		{
			return true;
		}
		return false;
	}

	bool FindIfCornerRoadBlock(ref PreGenNoiseValues preGen)
	{
		if (preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX - 1, preGen.newZ - 1) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX + 1, preGen.newZ - 1) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX - 1, preGen.newZ + 1) ||
			preGen.region != GetClampedFastNoise2D(noiseC,preGen.newX + 1, preGen.newZ + 1))
		{
			return true;
		}
		return false;
	}


	//Bottom,North,Top,South,West,East
	uint[] FindTextureOfGeneratingBrush(bool isSurface, ref PreGenNoiseValues preGen)
	{
		if(preGen.oceanMultiplier < 0.1f)
		{
			if (isSurface)
			{
				return new uint[] { 3, 8, 8, 8, 8, 8 };
			}
			else
			{
				return new uint[] { 3, 3, 3, 3, 3, 3 };
			}
		}
		if (preGen.biome <= 0.5f) //Grass
		{
			if ((preGen.regionBordercheck || preGen.regionBorderCornercheck) && isSurface)
			{
				return new uint[] { 3, 1, 1, 1, 1, 1 };
			}
			if (isSurface)
			{
				return new uint[] { 3, 4, 4, 4, 4, 4 };
			}
			else
			{
				return new uint[] { 3, 3, 3, 3, 3, 3 };
			}
		}
		else if (preGen.biome > 0.5f && preGen.biome <= 0.75f) //Desert
		{
			if ((preGen.regionBordercheck || preGen.regionBorderCornercheck) && isSurface)
			{
				return new uint[] { 3, 6, 6, 6, 6, 6 };
			}
			if (isSurface)
			{
				return new uint[] { 3, 5, 5, 5, 5, 5 };
			}
			else
			{
				return new uint[] { 3, 3, 3, 3, 3, 3 };
			}
		}
		else //Quarry
		{
			if ((preGen.regionBordercheck || preGen.regionBorderCornercheck) && isSurface)
			{
				return new uint[] { 7, 1, 1, 1, 1, 1 };
			}
			return new uint[] { 7, 7, 7, 7, 7, 7 };

		}
	}

	bool CheckBigBlock(ref PreGenNoiseValues preGen)
	{
		bool noiseValue = false;
		
		int chunkY = Mathf.FloorToInt(preGen.newY / (float)chunkSize / bigBlockSize);
		
		float terrain = Math.Clamp((curve1.SampleBaked(GetClampedFastNoise3D(noise,preGen.newX, preGen.newY, preGen.newZ))
				+ preGen.noiseDSampled)
				* preGen.noiseESampled,0,1);

		if (chunkY < 0)
		{
			//Below-Surface Generation
			noiseValue = true;

			if(preGen.oceanMultiplier < 0)
			{
				if (chunkY >= -6 && 1 - (terrain * -preGen.oceanMultiplier) < GetClampedChunkRange(-6 * chunkSize / bigBlockSize, 0, preGen.newY))
				{
					noiseValue = false;
				}
			}
		}
		if (chunkY >= 0 && preGen.oceanMultiplier >= 0)
		{
			//Above-Surface Generation
			noiseValue = false;

			if (chunkY < 6 && terrain * preGen.oceanMultiplier > GetClampedChunkRange(0, 6 * chunkSize / bigBlockSize, preGen.newY))
			{
				noiseValue = true;
			}
		}

		//Both Surface Generation
		if (chunkY < 5 && chunkY >= -10 &&
			curve2.SampleBaked(GetClampedFastNoise3D(noiseB,preGen.newX, preGen.newY, preGen.newZ))
			* curve6.SampleBaked(GetClampedFastNoise3D(noiseE,preGen.newX, preGen.newY, preGen.newZ))
			> curve3.SampleBaked(1 - GetClampedChunkRange(-10 * chunkSize / bigBlockSize, 5 * chunkSize / bigBlockSize, preGen.newY)))

		{
			noiseValue = false;
		}

		return noiseValue;
	}

	Brush[] CreateSurfaceBrushes(byte bitMask, bool subSurface, ref PreGenNoiseValues preGen)
	{
		bool check;
		byte[] verts;
		if (subSurface)
		{
			check = subSurfaceBrushes.TryGetValue(bitMask, out verts);
		}
		else
		{
			check = surfaceBrushes.TryGetValue(bitMask, out verts);
		}
		if(verts == null)
		{
			return new Brush[0];
		}

		Brush[] brushCopies = new Brush[verts.Length / 24];

		if (check)
		{
			Brush b;
			
			bool isSurface = (bitMask & (1 << 1)) == 0 && (bitMask & (1 << 0)) != 0 && preGen.chunkY >= -1;

			//Assign textures
			uint[] textures = FindTextureOfGeneratingBrush(isSurface, ref preGen);

			for (int i = 0; i < verts.Length / 24; i++)
			{
				b = new Brush { hiddenFlag = false, vertices = new byte[24], borderFlag = CheckBrushOnBorder(ref preGen), textures = textures };
				for (int e = 0; e < 24; e++)
				{
					b.vertices[e] = (byte)(verts[e + (i * 24)] + chunkMarginSize);
				}
				brushCopies[i] = b;
			}

			for (int e = 0; e < brushCopies.Length; e++)
			{
				for (int i = 0; i < brushCopies[e].vertices.Length; i += 3)
				{
					brushCopies[e].vertices[i] += (byte)preGen.posX;
					brushCopies[e].vertices[i + 1] += (byte)preGen.posY;
					brushCopies[e].vertices[i + 2] += (byte)preGen.posZ;
				}
			}

			return brushCopies;
		}

		return null;
	}

	byte CheckSurfaceBrushType(byte[,,] bigBlockArray, int pos, ref PreGenNoiseValues preGen)
	{

		int bitmask = 0;

		//HEY HEY!!!
		//Optimization, top and bottom go FIRST
		//Because PreGen values are the same, THEN
		//Pregen goes again when shifting X and Y

		//Top
		if (preGen.posY < bigBlockArray.GetLength(1) - 1)
		{
			if (GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY + 1, preGen.posZ], pos))
			{
				bitmask |= 1 << 1;
			}
		}
		else
		{
			preGen.newY += 1;
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 1;

			}
			preGen.newY -= 1;
		}
		//Bottom
		if (preGen.posY > 0)
		{
			if (GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY - 1, preGen.posZ], pos))
			{
				bitmask |= 1 << 0;
			}
		}
		else
		{
			preGen.newY -= 1;
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 0;

			}
			preGen.newY += 1;
		}
		//North
		if (preGen.posZ < bigBlockArray.GetLength(2) - 1)
		{			
			if (GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ + 1], pos))
			{
				bitmask |= 1 << 5;
			}
		}
		else
		{
			preGen.newZ += 1;
			PregenNoiseValues(ref preGen);
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 5;
			}
			preGen.newZ -= 1;
		}
		//East
		if (preGen.posX < bigBlockArray.GetLength(0) - 1)
		{
			if (GetBitOfByte(bigBlockArray[preGen.posX + 1, preGen.posY, preGen.posZ], pos))
			{
				bitmask |= 1 << 4;
			}
		}
		else
		{					
			preGen.newX += 1;
			PregenNoiseValues(ref preGen);
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 4;

			}
			preGen.newX -= 1;
		}
		//South
		if (preGen.posZ > 0)
		{
			if (GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ - 1], pos))
			{
				bitmask |= 1 << 3;

			}
		}
		else
		{		
			preGen.newZ -= 1;
			PregenNoiseValues(ref preGen);
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 3;

			}
			preGen.newZ += 1;
		}
		//West
		if (preGen.posX > 0)
		{
			if (GetBitOfByte(bigBlockArray[preGen.posX - 1, preGen.posY, preGen.posZ], pos))
			{
				bitmask |= 1 << 2;

			}
		}
		else
		{
			preGen.newX -= 1;
			PregenNoiseValues(ref preGen);
			if (CheckBigBlock(ref preGen))
			{
				bitmask |= 1 << 2;

			}
			preGen.newX += 1;
		}
		PregenNoiseValues(ref preGen);
		return (byte)bitmask;
	}


	bool CheckBrushVisibility(ref byte[,,] bigBlockArray, int byteIndex, ref PreGenNoiseValues preGen)
	{
		if (hideBigBlocks)
		{
			return true;
		}
		bool visibility = true;

		//OPTIMIZATION, Y goes first to not pregen noise
		//Y
		if (preGen.posY == 0)
		{
			preGen.newY -= 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY + 1, preGen.posZ], byteIndex);
			preGen.newY += 1;
		}
		else if (preGen.posY >= bigBlockArray.GetLength(1) - 1)
		{
			preGen.newY += 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY - 1, preGen.posZ], byteIndex);
			preGen.newY -= 1;
		}
		else
		{
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY - 1, preGen.posZ], byteIndex);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY + 1, preGen.posZ], byteIndex);
		}
		//X
		if (preGen.posX == 0)
		{
			preGen.newX -= 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX + 1, preGen.posY, preGen.posZ], byteIndex);
			preGen.newX += 1;
		}
		else if (preGen.posX >= bigBlockArray.GetLength(0) - 1)
		{
			preGen.newX += 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX - 1, preGen.posY, preGen.posZ], byteIndex);
			preGen.newX -= 1;
		}
		else
		{
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX - 1, preGen.posY, preGen.posZ], byteIndex);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX + 1, preGen.posY, preGen.posZ], byteIndex);
		}

		//Z
		if (preGen.posZ == 0)
		{
			preGen.newZ -= 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ + 1], byteIndex);
			preGen.newZ += 1;
		}
		else if (preGen.posZ >= bigBlockArray.GetLength(2) - 1)
		{
			preGen.newZ += 1;
			PregenNoiseValues(ref preGen);
			visibility &= CheckBigBlock(ref preGen);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ - 1], byteIndex);
			preGen.newZ -= 1;
		}
		else
		{
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ - 1], byteIndex);
			visibility &= GetBitOfByte(bigBlockArray[preGen.posX, preGen.posY, preGen.posZ + 1], byteIndex);
		}
		PregenNoiseValues(ref preGen);

		//If hidden
		return visibility;
	}

	bool CheckBrushOnBorder(ref PreGenNoiseValues preGen)
	{
		int length = chunkSize / bigBlockSize;
		return preGen.posX == 0 || preGen.posY == 0 || preGen.posZ == 0 || preGen.posX >= length - 1 || preGen.posY >= length - 1 || preGen.posZ >= length - 1;
	}


	ChunkRenderData GetChunkMesh(Chunk chunkData)
	{
		if (chunkData.brushes.Count == 0)
		{
			return new ChunkRenderData()
			{
				id = chunkData.id,
				state = ChunkRenderDataState.ready,
				position = new Vector3(chunkData.positionX, chunkData.positionY, chunkData.positionZ),
				chunk = chunkData,
				triangleIndexToBrushIndex = new List<int>(),
				triangleIndexToBrushTextureIndex = new List<int>()

			};
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
			return new ChunkRenderData()
			{
				id = chunkData.id,
				state = ChunkRenderDataState.ready,
				position = new Vector3(chunkData.positionX, chunkData.positionY, chunkData.positionZ),
				chunk = chunkData,
				triangleIndexToBrushIndex = new List<int>(),
				triangleIndexToBrushTextureIndex = new List<int>(),
			};
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
							brushIndexes = new List<int>(),
							brushface = new List<int>(),
						});
				}
			}
		}

		//Assign the surface to a mesh and return
		Node3D chunk = new Node3D();
		ArrayMesh arrMesh = new ArrayMesh();
		uint[] matIDs = new uint[splitMeshes.Count];


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
					preMesh.brushface.Add(e);
				}
			}
		}


		int currentPremesh = 0;
		Godot.Collections.Array surfaceArray = new Godot.Collections.Array();
		surfaceArray.Resize((int)Mesh.ArrayType.Max);
		List<int> triangletoBrushIndex = new List<int>();
		List<int> triangletoBrushTextureIndex = new List<int>();
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
			triangletoBrushTextureIndex.AddRange(value.brushface);
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
			triangleIndexToBrushIndex = triangletoBrushIndex,
			triangleIndexToBrushTextureIndex = triangletoBrushTextureIndex
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

	public float VolumeOfMesh(byte[] verts)
	{
		float total = 0;
		for (int i = 0; i < brushIndices.Length; i += 3)
		{
			total += SignedVolumeOfTriangle(
				new Vector3(verts[brushIndices[i] * 3], verts[1 + (brushIndices[i] * 3)], verts[2 + (brushIndices[i] * 3)]),
				new Vector3(verts[brushIndices[i + 1] * 3], verts[1 + (brushIndices[i + 1] * 3)], verts[2 + (brushIndices[i + 1] * 3)]),
				new Vector3(verts[brushIndices[i + 2] * 3], verts[1 + (brushIndices[i + 2] * 3)], verts[2 + (brushIndices[i + 2] * 3)])
				);
		}
		return Mathf.Abs(total);
	}

	float SignedVolumeOfTriangle(Vector3 p1, Vector3 p2, Vector3 p3)
	{
		var v321 = p3.X * p2.Y * p1.Z;
		var v231 = p2.X * p3.Y * p1.Z;
		var v312 = p3.X * p1.Y * p2.Z;
		var v132 = p1.X * p3.Y * p2.Z;
		var v213 = p2.X * p1.Y * p3.Z;
		var v123 = p1.X * p2.Y * p3.Z;
		return (1.0f / 6.0f) * (-v321 + v231 + v312 - v132 - v213 + v123);
	}

	int mod(int x, int m)
	{
		int r = x % m;
		return r < 0 ? r + m : r;
	}

	float GetClampedFastNoise2D(FastNoiseContainer container, float x, float z)
	{
		return GetClampedNoise(container.noise.GenSingle2D(x * container.frequency, z * container.frequency,container.seed));
	}

	float GetClampedFastNoise3D(FastNoiseContainer container, int x, int y, int z)
	{
		return GetClampedNoise(container.noise.GenSingle3D(x * container.frequency,y * container.frequency, z * container.frequency,container.seed));
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

	public int AssignBrushTexture(Node3D chunkNode, int brushID, uint materialID)
	{
		if (!firstChunkLoaded)
		{
			return -1;
		}
		LoadedChunkData foundChunk = FindChunkFromChunkNode(chunkNode);
		if (foundChunk == null)
		{
			return -1;
		}
		int index = Mathf.FloorToInt(brushID / 2.0f);

		int oldtex = (int)foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]].textures[foundChunk.triangleIndexToBrushTextureIndex[index]];
		if (oldtex == materialID)
		{
			return -1;
		}

		foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]].textures[foundChunk.triangleIndexToBrushTextureIndex[index]] = materialID;
		RerenderLoadedChunk(foundChunk);
		return oldtex;
	}

	public Vector3[] GetVertsFromFaceCollision(Node3D chunkNode, int brushID)
	{
		if (!firstChunkLoaded)
		{
			return null;
		}
		LoadedChunkData foundChunk = FindChunkFromChunkNode(chunkNode);
		if (foundChunk == null)
		{
			return null;
		}
		int index = Mathf.FloorToInt(brushID / 2.0f);
		Brush foundBrush = foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]];
		int foundFace = foundChunk.triangleIndexToBrushTextureIndex[index] * 6;

		Vector3[] verts = new Vector3[4];

		//Verts are accessed by 0,1,2,4 as they are unique in the set
		verts[0] = new Vector3(
				foundBrush.vertices[brushIndices[foundFace] * 3] - chunkMarginSize,
				foundBrush.vertices[(brushIndices[foundFace] * 3) + 1] - chunkMarginSize,
				foundBrush.vertices[(brushIndices[foundFace] * 3) + 2] - chunkMarginSize)
				+ (foundChunk.position * chunkSize);
		verts[1] = new Vector3(
			foundBrush.vertices[brushIndices[foundFace + 1] * 3] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 1] * 3) + 1] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 1] * 3) + 2] - chunkMarginSize)
			+ (foundChunk.position * chunkSize);
		verts[2] = new Vector3(
			foundBrush.vertices[brushIndices[foundFace + 2] * 3] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 2] * 3) + 1] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 2] * 3) + 2] - chunkMarginSize)
			+ (foundChunk.position * chunkSize);
		verts[3] = new Vector3(
			foundBrush.vertices[brushIndices[foundFace + 4] * 3] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 4] * 3) + 1] - chunkMarginSize,
			foundBrush.vertices[(brushIndices[foundFace + 4] * 3) + 2] - chunkMarginSize)
			 + (foundChunk.position * chunkSize);
		return verts;
	}

	public enum MoveType
	{
		face,
		edge,
		vert
	}

	public bool MoveVertsFromFaceCollision(Node3D chunkNode, int brushID, Vector3 move, ref int units, MoveType moveType, ref Vector3 hitPoint)
	{
		if (!firstChunkLoaded)
		{
			return false;
		}
		LoadedChunkData foundChunk = FindChunkFromChunkNode(chunkNode);
		if (foundChunk == null)
		{
			return false;
		}
		move = new Vector3(Mathf.Round(move.X), Mathf.Round(move.Y), Mathf.Round(move.Z));
		int index = Mathf.FloorToInt(brushID / 2.0f);
		Brush foundBrush = foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]];
		int foundFace = foundChunk.triangleIndexToBrushTextureIndex[index] * 6;

		float cost = VolumeOfMesh(foundBrush.vertices);
		int currentIndices;
		int finalVert = 0;
		int finalVertB = 0;
		int testMove;
		bool meshChanged = false;
		byte[] backupCopy = new byte[foundBrush.vertices.Length];
		System.Array.Copy(foundBrush.vertices, backupCopy, foundBrush.vertices.Length);

		switch (moveType)
		{
			case MoveType.face:
				for (int i = 0; i < 4; i++)
				{
					switch (i)
					{
						case 0:
							finalVert = foundFace;
							break;
						case 1:
							finalVert = foundFace + 1;
							break;
						case 2:
							finalVert = foundFace + 2;
							break;
						case 3:
							finalVert = foundFace + 4;
							break;
						default:
							break;
					}
					currentIndices = brushIndices[finalVert] * 3;

					testMove = foundBrush.vertices[currentIndices] + (int)move.X;
					if (testMove > 0 && testMove <= byte.MaxValue)
					{
						foundBrush.vertices[currentIndices] = (byte)testMove;
						meshChanged = true;
					}
					testMove = foundBrush.vertices[currentIndices + 1] + (int)move.Y;
					if (testMove > 0 && testMove <= byte.MaxValue)
					{
						foundBrush.vertices[currentIndices + 1] = (byte)testMove;
						meshChanged = true;
					}
					testMove = foundBrush.vertices[currentIndices + 2] + (int)move.Z;
					if (testMove > 0 && testMove <= byte.MaxValue)
					{
						foundBrush.vertices[currentIndices + 2] = (byte)testMove;
						meshChanged = true;
					}
				}
				break;
			case MoveType.edge:
				int testVertex = 0;
				float lowestDistance = float.MaxValue;
				float testDistance;
				finalVertB = -100;
				finalVert = -100;
				for (int i = 0; i < 4; i++)
				{
					switch (i)
					{
						case 0:
							testVertex = brushIndices[foundFace] * 3;
							break;
						case 1:
							testVertex = brushIndices[foundFace + 1] * 3;
							break;
						case 2:
							testVertex = brushIndices[foundFace + 2] * 3;
							break;
						case 3:
							testVertex = brushIndices[foundFace + 4] * 3;
							break;
						default:
							break;
					}
					testDistance = hitPoint.DistanceTo(new Vector3(
						foundBrush.vertices[testVertex] - chunkMarginSize + (chunkSize * foundChunk.position.X),
						foundBrush.vertices[testVertex + 1] - chunkMarginSize + (chunkSize * foundChunk.position.Y),
						foundBrush.vertices[testVertex + 2] - chunkMarginSize + (chunkSize * foundChunk.position.Z)
						));
					if (testDistance <= lowestDistance)
					{
						finalVertB = finalVert;
						finalVert = testVertex;
						lowestDistance = testDistance;
					}
				}

				if(finalVertB < 0 || finalVert < 0)
				{
					break;
				}

				hitPoint = new Vector3(
					((foundBrush.vertices[finalVert]     + foundBrush.vertices[finalVertB]) / 2.0f)     - chunkMarginSize + (chunkSize * foundChunk.position.X),
					((foundBrush.vertices[finalVert + 1] + foundBrush.vertices[finalVertB + 1]) / 2.0f) - chunkMarginSize + (chunkSize * foundChunk.position.Y),
					((foundBrush.vertices[finalVert + 2] + foundBrush.vertices[finalVertB + 2]) / 2.0f) - chunkMarginSize + (chunkSize * foundChunk.position.Z));
				testMove = foundBrush.vertices[finalVert] + (int)move.X;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVert + 1] + (int)move.Y;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert + 1] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVert + 2] + (int)move.Z;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert + 2] = (byte)testMove;
					meshChanged = true;
				}

				testMove = foundBrush.vertices[finalVertB] + (int)move.X;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVertB] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVertB + 1] + (int)move.Y;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVertB + 1] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVertB + 2] + (int)move.Z;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVertB + 2] = (byte)testMove;
					meshChanged = true;
				}
				break;
			case MoveType.vert:
				testVertex = 0;
				lowestDistance = float.MaxValue;
				testDistance = 0;
				for (int i = 0; i < 4; i++)
				{
					switch (i)
					{
						case 0:
							testVertex = brushIndices[foundFace] * 3;
							break;
						case 1:
							testVertex = brushIndices[foundFace + 1] * 3;
							break;
						case 2:
							testVertex = brushIndices[foundFace + 2] * 3;
							break;
						case 3:
							testVertex = brushIndices[foundFace + 4] * 3;
							break;
						default:
							break;
					}
					testDistance = hitPoint.DistanceTo(new Vector3(
						foundBrush.vertices[testVertex] - chunkMarginSize + (chunkSize * foundChunk.position.X),
						foundBrush.vertices[testVertex + 1] - chunkMarginSize + (chunkSize * foundChunk.position.Y),
						foundBrush.vertices[testVertex + 2] - chunkMarginSize + (chunkSize * foundChunk.position.Z)
						));
					if (testDistance < lowestDistance)
					{
						finalVert = testVertex;
						lowestDistance = testDistance;
					}
				}
				hitPoint = new Vector3(
					foundBrush.vertices[finalVert] - chunkMarginSize + (chunkSize * foundChunk.position.X),
					foundBrush.vertices[finalVert + 1] - chunkMarginSize + (chunkSize * foundChunk.position.Y),
					foundBrush.vertices[finalVert + 2] - chunkMarginSize + (chunkSize * foundChunk.position.Z));

				testMove = foundBrush.vertices[finalVert] + (int)move.X;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVert + 1] + (int)move.Y;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert + 1] = (byte)testMove;
					meshChanged = true;
				}
				testMove = foundBrush.vertices[finalVert + 2] + (int)move.Z;
				if (testMove > 0 && testMove <= byte.MaxValue)
				{
					foundBrush.vertices[finalVert + 2] = (byte)testMove;
					meshChanged = true;
				}
				break;
			default:
				break;
		}



		//Check for valid size
		Vector3 minSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int j = 0; j < foundBrush.vertices.Length; j += 3)
		{
			if (minSize.X > foundBrush.vertices[j]) { minSize.X = foundBrush.vertices[j]; }
			if (minSize.Y > foundBrush.vertices[j + 1]) { minSize.Y = foundBrush.vertices[j + 1]; }
			if (minSize.Z > foundBrush.vertices[j + 2]) { minSize.Z = foundBrush.vertices[j + 2]; }
			if (maxSize.X < foundBrush.vertices[j]) { maxSize.X = foundBrush.vertices[j]; }
			if (maxSize.Y < foundBrush.vertices[j + 1]) { maxSize.Y = foundBrush.vertices[j + 1]; }
			if (maxSize.Z < foundBrush.vertices[j + 2]) { maxSize.Z = foundBrush.vertices[j + 2]; }
		}
		Vector3 size = maxSize - minSize;
		if (size.X == 0 ||
			size.Y == 0 ||
			size.Z == 0 ||
			size.X > 86 ||
			size.Y > 86 ||
			size.Z > 86 ||
			size.X < -86 ||
			size.Y < -86 ||
			size.Z < -86
			)
		{
			foundBrush.vertices = backupCopy;
			return false;
		}

		//Check if mesh changed
		if (meshChanged)
		{
			cost = (Mathf.Round(cost * 1000) / 1000.0f) - (Mathf.Round(VolumeOfMesh(foundBrush.vertices) * 1000) / 1000.0f);
			if (cost < 0)
			{
				cost = Mathf.Floor(cost);
			}
			else
			{
				cost = Mathf.Ceil(cost);
			}
			if (units + cost < 0)
			{
				foundBrush.vertices = backupCopy;
				return false;
			}
			units += (int)cost;
			RerenderLoadedChunk(foundChunk);
			return true;
		}

		return false;
	}

	public Brush FindBrushFromCollision(Node3D chunkNode, int brushID)
	{
		if (!firstChunkLoaded)
		{
			return null;
		}
		LoadedChunkData foundChunk = FindChunkFromChunkNode(chunkNode);
		if (foundChunk == null)
		{
			return null;
		}
		int index = Mathf.FloorToInt(brushID / 2.0f);
		return foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]];
	}


	public Brush DestroyBlock(Node3D chunkNode, int brushID)
	{
		if (!firstChunkLoaded)
		{
			return null;
		}
		LoadedChunkData foundChunk = FindChunkFromChunkNode(chunkNode);
		if (foundChunk == null)
		{
			return null;
		}
		int index = Mathf.FloorToInt(brushID / 2.0f);

		//Signal to reveal hidden blocks
		if (foundChunk.chunk.connectedInvisibleBrushes.TryGetValue(foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]], out List<Brush> updateBrushes))
		{
			foreach (Brush pendingBrush in updateBrushes)
			{
				pendingBrush.hiddenFlag = false;
			}
			foundChunk.chunk.connectedInvisibleBrushes.Remove(foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]]);
		}

		bool borderCheck = foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]].borderFlag;

		//Check for border generation
		Vector3 chunkPos;
		if (borderCheck)
		{
			chunkPos = new Vector3(foundChunk.chunk.positionX, foundChunk.chunk.positionY, foundChunk.chunk.positionZ);

			for (int e = 0; e < loadedChunks.Count; e++)
			{
				if (loadedChunks[e].chunk.hasGeneratedBorders)
				{
					continue;
				}

				if ((loadedChunks[e].chunk.positionX == chunkPos.X - 1 &&
					loadedChunks[e].chunk.positionY == chunkPos.Y &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z) ||

					(loadedChunks[e].chunk.positionX == chunkPos.X + 1 &&
					loadedChunks[e].chunk.positionY == chunkPos.Y &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z) ||

					(loadedChunks[e].chunk.positionX == chunkPos.X &&
					loadedChunks[e].chunk.positionY == chunkPos.Y - 1 &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z) ||

					(loadedChunks[e].chunk.positionX == chunkPos.X &&
					loadedChunks[e].chunk.positionY == chunkPos.Y + 1 &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z) ||

					(loadedChunks[e].chunk.positionX == chunkPos.X &&
					loadedChunks[e].chunk.positionY == chunkPos.Y &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z - 1) ||

					(loadedChunks[e].chunk.positionX == chunkPos.X &&
					loadedChunks[e].chunk.positionY == chunkPos.Y &&
					loadedChunks[e].chunk.positionZ == chunkPos.Z + 1))
				{
					RenderChunkBordersVisible(loadedChunks[e]);
				}
			}
		}
		//Set up Values
		byte[] brushVerts = foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]].vertices;
		Vector3 pos = Vector3.Zero;
		Vector3 minSize = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
		Vector3 maxSize = new Vector3(float.MinValue, float.MinValue, float.MinValue);
		for (int j = 0; j < brushVerts.Length; j += 3)
		{
			pos += new Vector3(brushVerts[j], brushVerts[j + 1], brushVerts[j + 2]);
			if (minSize.X > brushVerts[j]) { minSize.X = brushVerts[j]; }
			if (minSize.Y > brushVerts[j + 1]) { minSize.Y = brushVerts[j + 1]; }
			if (minSize.Z > brushVerts[j + 2]) { minSize.Z = brushVerts[j + 2]; }
			if (maxSize.X < brushVerts[j]) { maxSize.X = brushVerts[j]; }
			if (maxSize.Y < brushVerts[j + 1]) { maxSize.Y = brushVerts[j + 1]; }
			if (maxSize.Z < brushVerts[j + 2]) { maxSize.Z = brushVerts[j + 2]; }
		}
		pos /= brushVerts.Length / 3;

		//Particles
		Vector3 size = (maxSize - minSize);
		destroyBrushParticles.GlobalPosition = pos + new Vector3(
			(chunkSize * foundChunk.chunk.positionX) - chunkMarginSize,
			(chunkSize * foundChunk.chunk.positionY) - chunkMarginSize,
			(chunkSize * foundChunk.chunk.positionZ) - chunkMarginSize
			);
		(destroyBrushParticles.ProcessMaterial as ParticleProcessMaterial).EmissionBoxExtents = size / 2.0f;
		destroyBrushParticles.Amount = (int)Mathf.Max(size.X * size.Y * size.Z * 0.2f, 2);
		destroyBrushParticles.MaterialOverride = mats[foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]].textures[1]];
		destroyBrushParticles.Restart();
		destroyBrushParticles.Emitting = true;

		Brush b = foundChunk.chunk.brushes[foundChunk.triangleIndexToBrushIndex[index]];

		//Remove and rerender
		foundChunk.chunk.brushes.RemoveAt(foundChunk.triangleIndexToBrushIndex[index]);
		if (borderCheck && !foundChunk.chunk.hasGeneratedBorders)
		{
			RenderChunkBordersVisible(foundChunk);
		}
		else
		{
			RerenderLoadedChunk(foundChunk);
		}
		return b;
	}

	public LoadedChunkData FindChunkFromChunkNode(Node3D chunkNode)
	{
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			if (loadedChunks[i].node == chunkNode)
			{
				return loadedChunks[i];
			}
		}
		return null;
	}

	public bool PlaceBlock(Vector3 position, int size)
	{
		if (!firstChunkLoaded)
		{
			return false;
		}

		//Double check parameters
		size = Math.Clamp(size, 1, chunkMarginSize * 2);
		position = new Vector3(
			Mathf.Floor(Mathf.Floor(position.X) / size) * size,
			Mathf.Floor(Mathf.Floor(position.Y) / size) * size,
			Mathf.Floor(Mathf.Floor(position.Z) / size) * size
			);
		//Calculate
		Vector3 chunkPos = new Vector3(
			Mathf.Floor(position.X / chunkSize),
			Mathf.Floor(position.Y / chunkSize),
			Mathf.Floor(position.Z / chunkSize)
			);
		Vector3 insideChunkPos = new Vector3(
			(mod((int)position.X, chunkSize)) + chunkMarginSize,
			(mod((int)position.Y, chunkSize)) + chunkMarginSize,
			(mod((int)position.Z, chunkSize)) + chunkMarginSize
			);
		for (int i = 0; i < loadedChunks.Count; i++)
		{
			if (loadedChunks[i].position.Equals(chunkPos))
			{
				Brush b = CreateBrush(insideChunkPos, Vector3.One * size);
				b.hiddenFlag = false;
				b.borderFlag = false;
				loadedChunks[i].chunk.brushes.Add(b);
				RerenderLoadedChunk(loadedChunks[i]);
				return true;
			}
		}

		return false;
	}

	void RenderChunkBordersVisible(LoadedChunkData chunk)
	{
		chunk.chunk.hasGeneratedBorders = true;

		for (int i = 0; i < chunk.chunk.brushes.Count; i++)
		{
			if (chunk.chunk.brushes[i].borderFlag)
			{
				chunk.chunk.brushes[i].hiddenFlag = false;
			}
		}

		RerenderLoadedChunk(chunk);
	}

	void RerenderLoadedChunk(LoadedChunkData chunk)
	{
		//Generate Mesh
		ChunkRenderData chunkData = GetChunkMesh(chunk.chunk);
		if (chunkData == null)
		{
			return;
		}
		if (chunk.node != null)
		{
			MeshInstance3D meshNode = chunk.node.GetChild(0) as MeshInstance3D;
			if (meshNode == null)
			{
				return;
			}
			meshNode.Mesh = chunkData.meshNode.Mesh;
			(meshNode.GetChild(0).GetChild(0) as CollisionShape3D).Shape = chunkData.collisionShape.Shape;//THIS WILL BREAK WITH MORE CHILD SHAPES
			chunk.triangleIndexToBrushIndex = chunkData.triangleIndexToBrushIndex;
			chunk.triangleIndexToBrushTextureIndex = chunkData.triangleIndexToBrushTextureIndex;
		}
		else if (chunkData.chunkNode != null)
		{
			AddChild(chunkData.chunkNode);
			chunkData.chunkNode.Name = "Chunk " + chunkData.id.ToString();
			chunkData.chunkNode.AddChild(chunkData.meshNode);
			chunkData.chunkNode.GlobalPosition = new Vector3((chunkData.position.X * chunkSize) - chunkMarginSize, (chunkData.position.Y * chunkSize) - chunkMarginSize, (chunkData.position.Z * chunkSize) - chunkMarginSize);
			chunkData.state = ChunkRenderDataState.garbageCollector;
			chunkData.meshNode.AddChild(chunkData.staticBody);
			chunkData.staticBody.AddChild(chunkData.collisionShape);

			chunk.id = chunkData.id;
			chunk.node = chunkData.chunkNode;
			chunk.position = chunkData.position;
			chunk.chunk = chunkData.chunk;
			chunk.triangleIndexToBrushIndex = chunkData.triangleIndexToBrushIndex;
			chunk.triangleIndexToBrushTextureIndex = chunkData.triangleIndexToBrushTextureIndex;
		}
	}

	public Vector3 FindValidSpawnPosition()
	{
		if (!firstChunkLoaded)
		{
			GD.PrintErr("Attempted to spawn a player before chunks finish loading");
			return Vector3.Zero;
		}

		int y = 0;
		int i;
		for (i = 0; i < loadedChunks.Count; i++)
		{
			if (loadedChunks[i].position.X != 0 || loadedChunks[i].position.Z != 0 || loadedChunks[i].position.Y != y)
			{
				continue;
			}
			if (loadedChunks[i].chunk.brushes.Count == 0)
			{
				break;
			}
			y++;
		}

		Vector3 pos = (loadedChunks[i].position * chunkSize) + new Vector3(0, chunkSize / 2.0f, 0);

		PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
		PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(pos, pos + new Vector3(0, -2000, 0));
		query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
		Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
		if (result.Count > 0)
		{
			return (Vector3)result["position"];
		}
		else
		{
			GD.PrintErr("Found no valid spawn. Ray sent from chunk Y=" + y + ", Pos was " + pos);
			return Vector3.Zero;
		}
	}

	public float GetChunksLoadedToLoadingRatio()
	{
		return (float)loadedChunks.Count / (float)(loadedChunks.Count + ongoingChunkRenderData.Count);
	}

	[ConsoleCommand("getseed", Description = "Prints the hashed seed value.")]
	void GetSeed()
	{
		if(!IsInsideTree())
		{
			Console.Instance.Print("Not currently in game");
			return;
		}
		Console.Instance.Print(seedA.ToString());
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
		public bool borderFlag;
	}

	class FastNoiseContainer
	{
		public FastNoise noise;
		public int seed;
		public float frequency;
	}

	class PreMesh
	{
		public List<Vector3> vertices;
		public List<int> indices;
		public List<Vector3> normals;
		public List<int> brushIndexes;
		public List<int> brushface;
	}

	public class Chunk
	{
		public bool hasGeneratedBorders;
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
		public List<int> triangleIndexToBrushIndex;
		public List<int> triangleIndexToBrushTextureIndex;
		public Chunk chunk;
	}

	public class LoadedChunkData
	{
		public Node3D node;
		public Vector3 position;
		public int id;
		public List<int> triangleIndexToBrushIndex;
		public List<int> triangleIndexToBrushTextureIndex;
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
		{ 0b011001,new byte[]{ //East-South Bottom Corner Connection
					
					0,0,0, 2,0,0, 3,0,2, 1,0,3,
					0,0,0, 2,1,0, 3,1,2, 1,0,3,

					2,0,0, 6,0,0, 6,0,4, 4,0,3,
					4,2,0, 6,2,0, 6,1,4, 4,1,3,

					1,0,3, 3,0,2, 4,0,3, 3,0,5,
					1,0,3, 3,1,2, 4,1,3, 3,0,5,

					4,0,3, 6,0,4, 6,0,6, 3,0,5,
					4,1,3, 6,1,4, 6,0,6, 3,0,5,

					2,0,0, 4,2,0, 4,0,3, 3,0,2,
					2,1,0, 3,4,0, 4,1,3, 3,1,2,

					4,2,0, 6,2,0, 6,1,4, 4,1,3,
					3,4,0, 6,4,0, 6,4,3, 3,4,0,

					3,4,0, 6,4,0, 6,4,3, 3,4,0,
					4,5,0, 6,6,0, 6,5,2, 4,5,0,

		}},
		{ 0b001101,new byte[]{ //South-West Bottom Corner Connection
				
			0,0,0,4,0,0,3,0,2,0,0,4,
			0,2,0,4,1,0,3,1,2,0,2,2,

			4,0,0,6,0,0,5,0,3,3,0,2,
			4,1,0,6,0,0,5,0,3,3,1,2,

			3,0,2,5,0,3,3,0,5,2,0,3,
			3,1,2,5,0,3,3,0,5,2,1,3,

			2,0,3,3,0,5,0,0,6,0,0,4,
			2,1,3,3,0,5,0,0,6,0,1,4,

			0,2,2,3,0,2,2,0,3,0,0,4,
			0,4,3,3,1,2,2,1,3,0,1,4,

			0,2,0,4,1,0,3,1,2,0,2,2,
			0,4,0,3,4,0,0,4,3,0,4,3,

			0,4,0,3,4,0,0,4,3,0,4,3,
			0,6,0,2,5,0,0,5,2,0,5,2,
		}},
		{ 0b100101,new byte[]{ //West-North Bottom Corner Connection
				
			0,0,0,3,0,1,2,0,3,0,0,2,
			0,0,0,3,0,1,2,1,3,0,1,2,

			2,0,3,3,0,1,5,0,3,3,0,4,
			2,1,3,3,0,1,5,0,3,3,1,4,

			5,0,3,
6,0,6,
4,0,6,
3,0,4,
5,0,3,
6,0,6,
4,1,6,
3,1,4,
0,0,2,
2,0,3,
4,0,6,
0,0,6,
0,1,2,
2,1,3,
2,2,6,
0,2,6,
2,0,3,
3,0,4,
4,0,6,
2,2,6,
2,1,3,
3,1,4,
4,1,6,
3,4,6,
0,1,2,
2,1,3,
2,2,6,
0,2,6,
0,4,3,
3,4,6,
3,4,6,
0,4,6,
0,4,3,
3,4,6,
3,4,6,
0,4,6,
0,5,4,
2,5,6,
2,5,6,
0,6,6,
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
