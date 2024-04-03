using Godot;
using System;

public partial class PlayerPrefToggle : CheckBox
{
	[Export] public string playerPref;
	[Export] LineEdit buttonTogglesLineEdit;

	public override void _Ready()
	{
		ButtonPressed = PlayerPrefs.GetBool(playerPref);
		if(buttonTogglesLineEdit != null)
		{
			buttonTogglesLineEdit.Editable = ButtonPressed;
		}

		Toggled += BoolChanged;
	}

	void BoolChanged(bool toggle)
	{
		PlayerPrefs.SetBool(playerPref, toggle);
		if(buttonTogglesLineEdit != null)
		{
			buttonTogglesLineEdit.Editable = toggle;
		}
	}
}
