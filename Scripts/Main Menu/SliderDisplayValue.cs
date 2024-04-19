using Godot;
using System;
using Range = Godot.Range;

public partial class SliderDisplayValue : Label
{
	[Export] public Range slider;
	[Export] public string suffix;
	public override void _Ready()
	{
		ChangeText(slider.Value);
		slider.ValueChanged += ChangeText;
	}

	void ChangeText(double value)
	{
		Text = value.ToString() + suffix;
	}
}
