using Godot;
using System;

public partial class Placing : Node3D
{
    [Export] public WorldGen worldGen;
    int currentSizeIndex = 0;
    int[] sizes = new int[] { 1, 2, 3, 6 };

    public static int currentPlacementSize;
    public override void _Ready()
    {
        currentPlacementSize = sizes[currentSizeIndex];
    }
    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.placeBrushSlot)
        {
            return;
        }
        if (Input.IsActionJustPressed("Alt Action"))
        {
            currentSizeIndex = mod(currentSizeIndex + 1, sizes.Length);
            currentPlacementSize = sizes[currentSizeIndex];
        }
        if (Input.IsActionJustPressed("Action"))
        {
            if (Math.Pow(sizes[currentSizeIndex],3) > Mining.totalBrushes)
            {
                return;
            }

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                if(worldGen.PlaceBlock((Vector3)result["position"] + ((Vector3)result["normal"])*0.5f, sizes[currentSizeIndex]))
                {
                    Mining.totalBrushes -= (int)Math.Pow(sizes[currentSizeIndex], 3);
                }
            }
        }
    }

    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
