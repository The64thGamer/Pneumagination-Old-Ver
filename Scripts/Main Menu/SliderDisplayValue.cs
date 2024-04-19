using Godot;
using System;

public partial class SliderDisplayValue : Label
{
	[Export] public Slider slider;
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
