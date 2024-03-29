using Godot;
using System;

public partial class Mining : Node3D
{
    [Export] public WorldGen worldGen;
    [Export] public ProgressBar miningBar;

    public static int totalBrushes;

    bool breaking;
    float breaktimer;
    float breakTimerStart;
    Node3D chunk;
    int faceID;
    Vector3 hitPos;

    public override void _Ready()
    {
        miningBar.Visible = false;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || ScrollBar.currentHotbarSelection != ScrollBar.miningSlot)
        {
            return;
        }
        if (Input.IsActionPressed("Action"))
        {
            if(!breaking)
            {
                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
                query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
                Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
                if (result.Count > 0)
                {
                    chunk = ((Node3D)result["collider"]).GetParent().GetParent() as Node3D;
                    faceID = (int)result["face_index"];
                    breaking = true;
                    breakTimerStart = 1;
                    breaktimer = breakTimerStart;
                    hitPos = (Vector3)result["position"];
                    miningBar.Visible = true;
                    GD.Print("Start");
                }
            }
            if (breaking && breaktimer > 0)
            {
                breaktimer = Mathf.Max(0, breaktimer - (float)delta);
                miningBar.Value = breaktimer / breakTimerStart;

                PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
                PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
                query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
                Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
                if (result.Count > 0)
                {
                    Node3D testchunk = ((Node3D)result["collider"]).GetParent().GetParent() as Node3D;
                    if (testchunk != chunk || (testchunk == chunk && (int)result["face_index"] != faceID))
                    {
                        DisableSelection();
                        return;
                    }
                }
            }
            if (breaking && breaktimer == 0)
            {
                //Destroy
                WorldGen.Brush b = worldGen.DestroyBlock(chunk, faceID);

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
                sound.GlobalPosition = hitPos;

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

                DisableSelection();
            }
        }
        else
        {
            DisableSelection();
        }
    }

    void DisableSelection()
    {
        GD.Print("Hide");
        miningBar.Visible = false;
        chunk = null;
        faceID = -1;
        breaktimer = 0;
        breaking = false;
        hitPos = Vector3.Zero;
    }
}
