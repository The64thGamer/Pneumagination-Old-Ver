using Godot;
using System;
using Range = Godot.Range;
public partial class PlayerPrefSlider : Range
{
	[Export] public string playerPref;
	[Export] public float defaultValue;

	public override void _Ready()
	{
		bool check = false;
		foreach(string key in PlayerPrefs.ListKeys)
		{
			if(key == playerPref)
			{
				check = true;
			}
		}
		if(!check)
		{
			PlayerPrefs.SetFloat(playerPref, defaultValue);
		}

		Value = PlayerPrefs.GetFloat(playerPref);
		ValueChanged += SliderChanged;
	}

	void SliderChanged(double value)
	{
		PlayerPrefs.SetFloat(playerPref, (float)value);
	}
}
