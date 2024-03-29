using Godot;
using System;

public partial class Mining : Node3D
{
    [Export] public WorldGen worldGen;

    public static int totalBrushes;

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.miningSlot)
        {
            return;
        }
        if (Input.IsActionJustPressed("Action"))
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                //Destroy
                WorldGen.Brush b = worldGen.DestroyBlock(((Node3D)result["collider"]).GetParent().GetParent() as Node3D, (int)result["face_index"]);

                //Sound
                int size = Mathf.CeilToInt(worldGen.VolumeOfMesh(b.vertices));
                Node3D sound;
                if (size <= 216)
                {
                    sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/Dig.tscn").Instantiate() as Node3D;
                }
                else
                {
                    sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/Dig Long.tscn").Instantiate() as Node3D;
                }
                GetTree().Root.AddChild(sound);
                sound.GlobalPosition = (Vector3)result["position"];
                
                //Size & Textures
                totalBrushes += size;
                for (int i = 0; i < b.textures.Length; i++)
                {
                    if (b.textures[i] == 0)
                    {
                        continue;
                    }
                    Inventory.inventory[b.textures[i]]++;
                }
            }
        }
    }
}
