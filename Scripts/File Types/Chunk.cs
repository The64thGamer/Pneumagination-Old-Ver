using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
	public Dictionary<ByteVector3, int> brushBBPositions;
}

[MemoryPackable]
public partial class ByteVector3: IEqualityComparer<ByteVector3>
{
	public byte x,y,z;

	public bool Equals(ByteVector3 a, ByteVector3 b)
	{
		if (a == null || b == null)
			return false;

		return a.x == b.x && a.y == b.y && a.z == b.z;
	}

	public int GetHashCode([DisallowNull] ByteVector3 obj)
	{
		return System.HashCode.Combine(obj.x, obj.y, obj.z);
	}
}
