using Godot;
using Godot.Collections;
using System;

public partial class Structure : Node
{
	[Export] Array<WorldGen.Brush> brushes { get; set; } = new Array<WorldGen.Brush>();
}
