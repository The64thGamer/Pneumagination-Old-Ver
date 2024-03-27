using Godot;
using System;

public partial class MeshEditing : Node3D
{
    [Export] public WorldGen worldGen;


    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.faceSlot)
        {
            return;
        }
        if (Input.IsActionJustPressed("Action"))
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                worldGen.GetVertsFromFaceCollision(((Node3D)result["collider"]).GetParent().GetParent() as Node3D, (int)result["face_index"]);
            }
        }
    }

    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
