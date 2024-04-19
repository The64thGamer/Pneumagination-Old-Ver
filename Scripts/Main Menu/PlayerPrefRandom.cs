using Godot;
using System;

public partial class PlayerPrefRandom : LineEdit
{
    [Export] public string playerPref;
    [Export] public bool randomizeEveryTime;
    [Export] public bool deterministicHashFinalValue;

    public override void _Ready()
    {
        string name; 

        if (randomizeEveryTime)
        {
            Random rnd = new Random();
            name = rnd.Next().ToString();
        }
        else
        {
            name = PlayerPrefs.GetString(playerPref);
        }
		if(name == "")
		{
            Random rnd = new Random();
            name = rnd.Next().ToString();
        }
        PlayerPrefs.SetString(playerPref, name);
        Text = name;

        TextChanged += NameChanged;

    }

	void NameChanged(string newText)
	{
        if(deterministicHashFinalValue)
        {
            PlayerPrefs.SetString(playerPref, GetDeterministicHashCode(newText));
        }
        else
        {
            PlayerPrefs.SetString(playerPref, newText);
        }
        GetTree().Root.PropagateNotification(64646464);
    }

    string GetDeterministicHashCode(string str)
    {
        int hash1 = (5381 << 16) + 5381;
        int hash2 = hash1;

        for (int i = 0; i < str.Length; i += 2)
        {
            hash1 = ((hash1 << 5) + hash1) ^ str[i];
            if (i == str.Length - 1)
                break;
            hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
        }

        return (hash1 + (hash2 * 1566083941)).ToString();
    }
}
