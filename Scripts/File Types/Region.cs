using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Region : Resource
{
	[Export] public int positionX;
	[Export] public int positionY;
	[Export] public int positionZ;
	[Export] public Dictionary<Vector3,Chunk> chunks;
}
