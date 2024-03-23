using Godot;
using System;

public partial class Placing : Node3D
{
    [Export] public WorldGen worldGen;
    int size = 1;

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.placeBrushSlot)
        {
            return;
        }
        if (Input.IsActionJustPressed("Action"))
        {
            if(Math.Pow(size,3) > Mining.totalBrushes)
            {
                return;
            }

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                if(worldGen.PlaceBlock((Vector3)result["position"] + ((Vector3)result["normal"])*0.5f, size))
                {
                    Mining.totalBrushes -= (int)Math.Pow(size, 3);
                }
            }
        }
    }
}
