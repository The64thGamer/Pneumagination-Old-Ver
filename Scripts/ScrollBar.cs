using Godot;
using System;
using System.Linq;

public partial class ScrollBar : HBoxContainer
{
    public static int currentHotbarSelection = miningSlot;

    public const int vertexSlot = 0;
    public const int edgeSlot = 1;
    public const int faceSlot = 2;
    public const int placeBrushSlot = 3;
    public const int miningSlot = 4;

    [Export] Color selectColor;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        UpdateHotbar();
    }

    public override void _Process(double delta)
    {
        if (PhotoMode.photoModeEnabled)
        {
            Visible = false;
            return;
        }
        else
        {
            Visible = true;
        }
    }

    public override void _PhysicsProcess(double delta)
    {
        if (PhotoMode.photoModeEnabled)
        {
            return;
        }
        if (Input.IsActionJustPressed("Scroll Up"))
        {
            currentHotbarSelection =  mod(currentHotbarSelection - 1, GetChildCount());
            UpdateHotbar();
        }
        if (Input.IsActionJustPressed("Scroll Down"))
        {
            currentHotbarSelection = mod(currentHotbarSelection + 1, GetChildCount());
            UpdateHotbar();
        }
    }

    void UpdateHotbar()
    {
        System.Collections.Generic.IEnumerable<TextureRect> icons = GetChildren().OfType<TextureRect>();

        int i = 0;
        foreach (TextureRect rect in icons)
        {
            if(i == currentHotbarSelection)
            {
                rect.SelfModulate = selectColor;
            }
            else
            {
                rect.SelfModulate = new Color(1, 1, 1);
            }
            i++;
        }
    }

    int mod(int x, int m)
    {
        int r = x % m;
        return r < 0 ? r + m : r;
    }
}
