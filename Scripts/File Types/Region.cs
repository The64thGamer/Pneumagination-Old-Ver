using System.Collections.Generic;
using Godot;
using MemoryPack;

[MemoryPackable]
public partial class Region
{
	public int positionX;
	public int positionY;
	public int positionZ;
	public Dictionary<Vector3,Chunk> chunks;
}
