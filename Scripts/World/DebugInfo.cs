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

        text = "\n " + (string)ProjectSettings.GetSetting("application/config/name") + " v" + (string)ProjectSettings.GetSetting("application/config/version");
        if(this.Text != text)
        {
            this.Text = text;
        }
    }
}
