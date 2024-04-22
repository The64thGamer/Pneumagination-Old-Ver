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
public partial class ByteVector3
{
	public byte x,y,z;
	public ByteVector3(byte x, byte y, byte z)
	{
        this.x = x;
		this.y = y;
		this.z = z;
	}

	public override bool Equals(object obj)
    {
        // If the object is null, return false.
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        // Cast the object to ByteVector3 to compare values.
        ByteVector3 other = (ByteVector3)obj;
        
        // Check if all components are equal.
        return x == other.x && y == other.y && z == other.z;
    }
    public override int GetHashCode()
    {
		return System.HashCode.Combine(x,y,z);
	}
}
