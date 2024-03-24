using Godot;
using System;

public partial class ItemCount : Label
{
    [Export] public int checkItem;
    public override void _Process(double delta)
    {
        string text = Inventory.inventory[checkItem].ToString();
        if (this.Text != text)
        {
            this.Text = text;
        }
    }
}
