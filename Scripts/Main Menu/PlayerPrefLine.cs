using Godot;
using System;

public partial class PlayerPrefLine : LineEdit
{
	[Export] public string playerPref;
	[Export] public string defaultValue;

	public override void _Ready()
	{
		string name = PlayerPrefs.GetString(playerPref);
		if(name == "")
		{
			name = defaultValue;
			PlayerPrefs.SetString(playerPref, defaultValue);
		}
		Text = name;

		TextChanged += NameChanged;

	}

	void NameChanged(string newText)
	{
		PlayerPrefs.SetString(playerPref, newText);
	}
}
