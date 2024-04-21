using System.Collections.Generic;
using Godot;
using MemoryPack;

[MemoryPackable]
public partial class Brush
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
	public uint[] textures = new uint[]{ 0, 0, 0, 0, 0, 0 };
	public bool hiddenFlag;
	public bool borderFlag;
}
