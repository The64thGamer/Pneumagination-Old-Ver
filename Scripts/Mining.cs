using Godot;
using System;

public partial class Mining : Node3D
{
    [Export] public WorldGen worldGen;

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionJustPressed("Mining"))
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                Vector3 position = ((Node3D)result["collider"]).GlobalPosition;

                worldGen.DestroyBlock(((Node3D)result["collider"]).GetParent().GetParent() as Node3D, Mathf.FloorToInt(((int)result["face_index"])));

            }
        }
    }
}
