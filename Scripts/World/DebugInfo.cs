using Godot;
using System;
using System.Diagnostics;

public partial class DebugInfo : Label
{
    string text = "";

    ServerClient server;

    public override void _Ready()
    {
		server = GetTree().Root.GetNode("World/Server") as ServerClient;
        text = "\n " + Tr("CURRENT_VERSION");
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

        if(this.Text != text)
        {
            this.Text = text;
        }
    }
}
