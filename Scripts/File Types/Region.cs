using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Region : Resource
{
	public const int regionSize = 5;

	public int positionX;
	public int positionY;
	public int positionZ;
	public Dictionary<Vector3,Chunk> chunks;

	public Region(int regX, int regY, int regZ)
	{
		positionX = regX;
		positionY = regY;
		positionZ = regZ;
		chunks = new Dictionary<Vector3, Chunk>();
	}
}
