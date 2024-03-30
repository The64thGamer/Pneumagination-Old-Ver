using Godot;
using System;

public partial class TotalBrushes : Label
{
    public override void _Process(double delta)
    {
        string text;
        if(Placing.currentPlacementSize == 1)
        {
            text = Mining.totalBrushes.ToString();
        }
        else
        {
            text = (Mathf.FloorToInt(Mining.totalBrushes / Mathf.Pow(Placing.currentPlacementSize, 3))).ToString() + "^" + Placing.currentPlacementSize.ToString();
        }
        if (this.Text != text)
        {
            this.Text = text;
        }
    }
}
