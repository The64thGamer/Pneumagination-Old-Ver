using Godot;
using System;
using System.Diagnostics;

public partial class DebugInfo : Label
{
    string text = "";
    public override void _Process(double delta)
    {
        text = "\n " + (string)ProjectSettings.GetSetting("application/config/name") + " v" + (string)ProjectSettings.GetSetting("application/config/version") + " Seed- " + WorldGen.seedA.ToString("D10")+ WorldGen.seedB.ToString("D10") + WorldGen.seedC.ToString("D10") + WorldGen.seedD.ToString("D10") + " X" + (int)PlayerMovement.currentPosition.X + " Y" + (int)PlayerMovement.currentPosition.Y + " Z"+ (int)PlayerMovement.currentPosition.Z + " " + DateTime.Today.Add(TimeSpan.FromDays(EnvironmentController.timeOfDay)).ToString("hhtt").TrimStart('0'); ;
        if(this.Text != text)
        {
            this.Text = text;
        }
    }
}
