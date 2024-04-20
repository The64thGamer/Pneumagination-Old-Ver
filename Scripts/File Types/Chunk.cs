using Godot;
using Godot.Collections;

[GlobalClass]
public partial class Chunk : Resource
{
	[Export] public bool hasGeneratedBorders;
	[Export] public int positionX;
	[Export] public int positionY;
	[Export] public int positionZ;
	[Export] public Array<Brush> brushes;
	[Export] public Dictionary<Brush, Array<Brush>> connectedInvisibleBrushes;
}
