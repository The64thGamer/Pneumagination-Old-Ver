using Godot;
using System;

public partial class Placing : Node3D
{
    [Export] public WorldGen worldGen;

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled)
        {
            return;
        }
        if (Input.IsActionJustPressed("Placing"))
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                GD.Print(worldGen.PlaceBlock((Vector3)result["position"]));
            }
        }
    }
}
