using Godot;
using System;

public partial class Mining : Node3D
{

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
                GD.Print("Hit at Triangle: ", result["face_index"]);
                Sprite3D sprite = new Sprite3D();
                sprite.Texture = (Texture2D)GD.Load("res://Textures/testtexture.png");
                GetTree().Root.AddChild(sprite);
                sprite.GlobalPosition = (Vector3)result["position"];
            }
        }
    }
}
