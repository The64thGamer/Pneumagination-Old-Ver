using Godot;
using media.Laura.SofiaConsole;

public partial class WorldCommands : Node
{
	[ConsoleCommand("ListAllPlayers", Description = "Prints names of all currently connected players.")]
	void ListAllPlayers()
	{
		Node serverNode = (Engine.GetMainLoop() as SceneTree).Root.FindChild("Server");
		if(serverNode == null)
		{
			Console.Instance.Print("Not currently in game");
			return;
		}
		Console.Instance.Print((serverNode as ServerClient).ListPlayerNames());
	}
}
