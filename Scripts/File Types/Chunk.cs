using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Chunk : Resource
{
	public bool hasGeneratedBorders;
	public int positionX;
	public int positionY;
	public int positionZ;
	public Array<Brush> brushes;
	public Godot.Collections.Dictionary<Brush, Godot.Collections.Array<Brush>> connectedInvisibleBrushes;
}
