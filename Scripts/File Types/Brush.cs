using Godot;
using Godot.Collections;

	public partial class Brush : Resource
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
		[Export] public Array<byte> vertices;
		[Export] public Array<uint> textures = new Array<uint>{ 0, 0, 0, 0, 0, 0 };
		[Export] public bool hiddenFlag;
		[Export] public bool borderFlag;
	}
