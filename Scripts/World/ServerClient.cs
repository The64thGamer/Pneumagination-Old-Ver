using Godot;
using media.Laura.SofiaConsole;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Console = media.Laura.SofiaConsole.Console;

public partial class ServerClient : Node
{
	public static List<PlayerInfo> playerList = new List<PlayerInfo>();

	ENetMultiplayerPeer peer;

	const string address = "127.0.0.1";
	const int maxPlayers = 256;

	public override void _Ready()
	{
		if(PlayerPrefs.GetBool("Joining"))
		{
			JoinServer();
		}
		else
		{
			CreateServer();
		}

		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

	void CreateServer()
	{
		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(Convert.ToInt32(PlayerPrefs.GetString("Hosting Port")),maxPlayers);
		if(error != Error.Ok)
		{
			GD.Print("Hosting Failed: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.Zlib);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Hosting Started");
	}

	void JoinServer()
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address,Convert.ToInt32(PlayerPrefs.GetString("Joining Port")));

		peer.Host.Compress(ENetConnection.CompressionMode.Zlib);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Joining Started");
	}

     void ConnectionFailed()
    {
		GD.Print("CONNECTION FAILED");
    }

     void ConnectedToServer()
    {
        GD.Print("Connected To Server");
    }

     void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
    }


     void PeerConnected(long id)
    {
        GD.Print("Player Connected! " + id.ToString());

		if(!Multiplayer.IsServer())
		{
			return;
		}
		playerList.Add(new PlayerInfo(){id = id,});
    }

	public string ListPlayerNames()
	{
		if(playerList.Count == 0)
		{
			return "No Players Connected";
		}
		string finalresult = "";
		for(int i = 0; i < playerList.Count; i++)
		{
			finalresult += playerList[index: i].id.ToString() + '\n';
		}
		return finalresult;
	}

}

public class PlayerInfo
{
	public string name;
	public long id;
	public CharacterBody3D playerObject;
}