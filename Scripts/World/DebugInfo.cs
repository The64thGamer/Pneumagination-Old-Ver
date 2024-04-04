using Godot;
using System;
using System.Diagnostics;

public partial class DebugInfo : Label
{
    string text = "";

    Node3D mainPlayer;

    public override void _Ready()
    {
        mainPlayer = (GetTree().Root.GetNode("World/Server") as ServerClient).GetMainPlayer();
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

        text = "\n " + (string)ProjectSettings.GetSetting("application/config/name") + " v" + (string)ProjectSettings.GetSetting("application/config/version") + " Seed- " + WorldGen.seedA.ToString("D10")+ WorldGen.seedB.ToString("D10") + WorldGen.seedC.ToString("D10") + WorldGen.seedD.ToString("D10") + " X" + (int)mainPlayer.GlobalPosition.X + " Y" + (int)mainPlayer.GlobalPosition.Y + " Z"+ (int)mainPlayer.GlobalPosition.Z + " " + DateTime.Today.Add(TimeSpan.FromDays(EnvironmentController.timeOfDay)).ToString("hhtt").TrimStart('0'); ;
        if(this.Text != text)
        {
            this.Text = text;
        }
    }
}
