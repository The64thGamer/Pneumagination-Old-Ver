using Godot;
using System;

public partial class PF_Element : Control
{
    [Export] Label keyLabel;
    [Export] Label valueLabel;
    [Export] Button mainButton;
    [Export] Button deleteButton;

    public void SetElement (string key, string value)
    {
        keyLabel.Text = key;
        valueLabel.Text = value;
    }

    public void AssignMainButton (Action<string> action)
    {
        mainButton.Pressed += () => action?.Invoke(keyLabel.Text);
    }

    public void AssignDeleteAction (Action action)
    {
        deleteButton.Pressed += () => action?.Invoke();
    }
}
