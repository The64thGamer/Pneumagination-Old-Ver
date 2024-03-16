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
                //Sprite3D sprite = new Sprite3D();
                //sprite.Texture = (Texture2D)GD.Load("res://Textures/testtexture.png");
                //GetTree().Root.AddChild(sprite);
                //sprite.GlobalPosition = (Vector3)result["position"];

                worldGen.DestroyBlock(((Node3D)result["collider"]).GetParent().GetParent() as Node3D, Mathf.FloorToInt(((int)result["face_index"]) / 12.0f)); //"/12.0f" for each face of the cube

            }
        }
    }
}
