using Godot;
using System;

public partial class TotalBrushes : Label
{
    public override void _Process(double delta)
    {
        string text = Mining.totalBrushes.ToString();
        if (this.Text != text)
        {
            this.Text = text;
        }
    }
}
