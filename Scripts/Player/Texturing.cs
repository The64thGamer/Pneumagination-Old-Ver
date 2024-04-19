using Godot;
using System;

public partial class Texturing : Node3D
{
    WorldGen worldGen;


    public override void _Ready()
    {
		worldGen = GetTree().Root.GetNode("World") as WorldGen;
    }
    
    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled || MenuToggle.pauseMenuEnabled|| WikiStart.wikiEnabled || ScrollBar.currentHotbarSelection <= ScrollBar.miningSlot)
        {
            return;
        }
        if (Input.IsActionJustPressed("Action") && Inventory.inventory[ScrollBar.currentHotbarSelection - ScrollBar.miningSlot] > 0)
        {
            PhysicsDirectSpaceState3D spaceState = GetWorld3D().DirectSpaceState;
            PhysicsRayQueryParameters3D query = PhysicsRayQueryParameters3D.Create(this.GlobalPosition, this.GlobalPosition + (-this.GlobalTransform.Basis.Z * PlayerMovement.playerReach));
            query.CollisionMask = 0b00000000_00000000_00000000_00000100; //Brushes
            Godot.Collections.Dictionary result = spaceState.IntersectRay(query);
            if (result.Count > 0)
            {
                int assignment = worldGen.AssignBrushTexture(((Node3D)result["collider"]).GetParent().GetParent() as Node3D, (int)result["face_index"], (uint)(ScrollBar.currentHotbarSelection - ScrollBar.miningSlot));
                if(assignment > -1)
                {
                    Inventory.inventory[ScrollBar.currentHotbarSelection - ScrollBar.miningSlot]--;
                }
                if (assignment > 0)
                {
                    Inventory.inventory[assignment]++;
                }
            }
        }
    }
}
