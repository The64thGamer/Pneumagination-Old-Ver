using System.Collections.Generic;
using Godot;
using MemoryPack;

[MemoryPackable]
public partial class Chunk
{
	public bool hasGeneratedBorders;
	public int positionX;
	public int positionY;
	public int positionZ;
	public List<Brush> brushes;
	public Dictionary<byte[], int> brushBBPositions;
}
