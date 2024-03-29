using Godot;
using System;

public partial class LoadingBar : ProgressBar
{
	[Export] WorldGen worldGen;
	[Export] Curve curve;
	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
		if(!PhotoMode.photoModeEnabled)
		{
			QueueFree();
		}

		Value = curve.SampleBaked(worldGen.GetChunksLoadedToLoadingRatio());
	}
}
