using Godot;
using System;
using System.Drawing;

public partial class Placing : Node3D
{
    WorldGen worldGen;
    Node3D mainPlayer;
    int currentSizeIndex = 0;
    int[] sizes = new int[] { 1, 2, 3, 6, 12 };

    public static int currentPlacementSize;
            

    public override void _Ready()
    {
        currentPlacementSize = sizes[currentSizeIndex];
		worldGen = GetTree().Root.GetNode("World") as WorldGen;
        mainPlayer = (GetTree().Root.GetNode("World/Server") as ServerClient).GetMainPlayer();

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
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                int size = sizes[currentSizeIndex];
                Vector3 position = (Vector3)result["position"] + ((Vector3)result["normal"]) * 0.5f * size;
                Vector3 intPosition = new Vector3(
                Mathf.Floor(Mathf.Floor(position.X) / size) * size,
                Mathf.Floor(Mathf.Floor(position.Y) / size) * size,
                Mathf.Floor(Mathf.Floor(position.Z) / size) * size
                );

                //Check player position and height to not be inside Brush
                //THIS IS FLAWED it only checks 3 points, make a Raycast version someday
                if (
                    mainPlayer.GlobalPosition.X >= intPosition.X && mainPlayer.GlobalPosition.X <= intPosition.X + size &&
                    (mainPlayer.GlobalPosition.Y >= intPosition.Y && mainPlayer.GlobalPosition.Y <= intPosition.Y + size ||
                    mainPlayer.GlobalPosition.Y + 10 >= intPosition.Y && mainPlayer.GlobalPosition.Y + 10 <= intPosition.Y + size ||
                    mainPlayer.GlobalPosition.Y + 5 >= intPosition.Y && mainPlayer.GlobalPosition.Y + 5 <= intPosition.Y + size) &&
                    mainPlayer.GlobalPosition.Z >= intPosition.Z && mainPlayer.GlobalPosition.Z <= intPosition.Z + size
                    )
                {
                    return;
                }

                if (worldGen.PlaceBlock(position, size))
                {
                    Mining.totalBrushes -= (int)Math.Pow(size, 3);
                    Node3D sound = null;
                    switch (currentSizeIndex)
                    {
                        case 0:
                            sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockA.tscn").Instantiate() as Node3D;
                            break;
                        case 1:
                            sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockB.tscn").Instantiate() as Node3D;
                            break;
                        case 2:
                            sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockC.tscn").Instantiate() as Node3D;
                            break;
                        case 3:
                            sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockD.tscn").Instantiate() as Node3D;
                            break;
                        case 4:
                            sound = GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockE.tscn").Instantiate() as Node3D;
                            break;
                        default:
                            break;
                    }
                    GetTree().Root.AddChild(sound);
                    sound.GlobalPosition = position;
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
