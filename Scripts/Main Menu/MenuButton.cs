using Godot;
using System;
using Console = media.Laura.SofiaConsole.Console;

public partial class MenuButton : BaseButton
{
	[Export] public ButtonFunctionType buttonFunction;
	[Export] public WorldEnum keepIfIn;

	string currentScene;

	public enum WorldEnum
	{
		none,
		world,
		menu,
	}
	public enum ButtonFunctionType
	{
		openWiki,
		openTerminal,
		openOptions,
		saveFiles,
		joinGame,
		quit,
		exitToTitle,
	}
	public override void _Ready()
	{
		currentScene = GetTree().CurrentScene.Name;
		Pressed += ButtonPress;

		switch(keepIfIn)
		{
			case WorldEnum.menu:
				if(currentScene != "Menu")
				{
					QueueFree();
					return;
				}   
				break;
			case WorldEnum.world:
			if(currentScene != "World")
			{
				QueueFree();
				return;
			}   
			break;
		}
	}

	void ButtonPress()
	{
		switch(buttonFunction)
		{
			case ButtonFunctionType.openTerminal:
				Console.Instance.ToggleConsole();
				break;
			case ButtonFunctionType.openWiki:
				GetTree().Root.GetNode<WikiStart>(currentScene + "/Wiki").ToggleWiki();
				break;
			case ButtonFunctionType.openOptions:
				GetTree().Root.GetNode<OptionsStart>(currentScene + "/Options").ToggleOptions();
				break;
			case ButtonFunctionType.saveFiles:
				GetTree().Root.GetNode<MenuSelector>(currentScene + "/Menu/VBoxContainer/Menu Container/Panel").SetVisible("Save Files");
				break;
			case ButtonFunctionType.quit:
				GetTree().Root.PropagateNotification((int)NotificationWMCloseRequest);
				GetTree().Quit();
				break;
			case ButtonFunctionType.exitToTitle:
				GetTree().ChangeSceneToFile("res://Scenes/Menu.tscn");
				break;
			default:
				break;
		}
	}
}
