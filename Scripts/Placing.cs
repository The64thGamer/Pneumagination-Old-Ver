using Godot;
using System;
using System.Drawing;

public partial class Placing : Node3D
{
    [Export] public WorldGen worldGen;
    int currentSizeIndex = 0;
    int[] sizes = new int[] { 1, 2, 3, 6, 12 };

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


            switch (currentSizeIndex)
            {
                case 0:
                    AddChild(GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockA.tscn").Instantiate());
                    break;
                case 1:
                    AddChild(GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockB.tscn").Instantiate());
                    break;
                case 2:
                    AddChild(GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockC.tscn").Instantiate());
                    break;
                case 3:
                    AddChild(GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockD.tscn").Instantiate());
                    break;
                case 4:
                    AddChild(GD.Load<PackedScene>("res://Prefabs/Sound Prefabs/BlockE.tscn").Instantiate());
                    break;
                default:
                    break;
            }

            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * 50));
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
                    PlayerMovement.currentPosition.X >= intPosition.X && PlayerMovement.currentPosition.X <= intPosition.X + size &&
                    (PlayerMovement.currentPosition.Y >= intPosition.Y && PlayerMovement.currentPosition.Y <= intPosition.Y + size ||
                    PlayerMovement.currentPosition.Y + 10 >= intPosition.Y && PlayerMovement.currentPosition.Y + 10 <= intPosition.Y + size ||
                    PlayerMovement.currentPosition.Y + 5 >= intPosition.Y && PlayerMovement.currentPosition.Y + 5 <= intPosition.Y + size) &&
                    PlayerMovement.currentPosition.Z >= intPosition.Z && PlayerMovement.currentPosition.Z <= intPosition.Z + size
                    )
                {
                    return;
                }

                if (worldGen.PlaceBlock(position, size))
                {
                    Mining.totalBrushes -= (int)Math.Pow(size, 3);
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
