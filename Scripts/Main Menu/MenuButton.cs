using Godot;
using System;
using Console = media.Laura.SofiaConsole.Console;

public partial class MenuButton : BaseButton
{
	[Export] public ButtonFunctionType buttonFunction;
	public enum ButtonFunctionType
	{
		openWiki,
		openTerminal,
		openOptions,
		saveFiles,
		joinGame
	}
	public override void _Ready()
	{
		Pressed += ButtonPress;
	}

	void ButtonPress()
	{
		switch(buttonFunction)
		{
			case ButtonFunctionType.openTerminal:
				Console.Instance.ToggleConsole();
				break;
			case ButtonFunctionType.openWiki:
				GetTree().Root.GetNode<WikiStart>("Menu/Wiki").ToggleWiki();
				break;
			case ButtonFunctionType.openOptions:
				GetTree().Root.GetNode<OptionsStart>("Menu/Options").ToggleOptions();
				break;
			case ButtonFunctionType.saveFiles:
				GetTree().Root.GetNode<MenuSelector>("Menu/CanvasLayer/VBoxContainer/Menu Container/Panel").SetVisible("Save Files");
				break;
			default:
				break;
		}
	}
}
