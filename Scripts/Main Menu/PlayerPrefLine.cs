using Godot;
using System;

public partial class PlayerPrefLine : LineEdit
{
	[Export] public string playerPref;
	[Export] public string defaultValue;
	[Export] public bool defaultIsTranslationKey;

	public override void _Ready()
	{
		string name = PlayerPrefs.GetString(playerPref);
		if(name == "")
		{
			if(defaultIsTranslationKey)
			{
				name = Tr(defaultValue);
			}
			else
			{
				name = defaultValue;
			}
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
