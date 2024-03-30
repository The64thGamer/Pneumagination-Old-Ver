using Godot;
using System;

public partial class PlayerName : LineEdit
{
	public override void _Ready()
	{
		string name = PlayerPrefs.GetString("Name");
		if(name == "")
		{
			name = "I Forgot To Change Name";
		}
        Text = name;

        TextChanged += NameChanged;

    }

	void NameChanged(string newText)
	{
        PlayerPrefs.SetString("Name", newText);
    }
}
