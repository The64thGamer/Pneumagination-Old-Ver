using Godot;
using System;

public partial class HideSimplePlayerUI : Control
{
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
}
