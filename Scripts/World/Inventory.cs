using Godot;
using System;

public partial class Inventory : Node
{
	public static int[] inventory;
	public override void _Ready()
    {
		int i = 0;
		while (true)
		{
			if(!FileAccess.FileExists("res://Materials/" + i + ".tres"))
			{
				inventory = new int[i];
				return;
			}
			i++;
		}    
	}
}
