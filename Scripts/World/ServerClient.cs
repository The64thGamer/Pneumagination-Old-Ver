using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using Console = media.Laura.SofiaConsole.Console;
using media.Laura.SofiaConsole;

public partial class ServerClient : Node
{
	public static List<PlayerInfo> playerList = new List<PlayerInfo>();

	ENetMultiplayerPeer peer;

	const string address = "127.0.0.1";
	const int maxPlayers = 256;
	const long hostID = 1;

	public override void _Ready()
	{		
		if(!IsInsideTree())
		{
			return;
		}

		if(PlayerPrefs.GetBool("Joining"))
		{		
			JoinServer(Convert.ToInt32(PlayerPrefs.GetString("Joining Port")));
		}
		else
		{		
			CreateServer(Convert.ToInt32(PlayerPrefs.GetString("Hosting Port")));
		}

		Multiplayer.PeerConnected += PeerConnected;
		Multiplayer.PeerDisconnected += PeerDisconnected;
		Multiplayer.ConnectedToServer += ConnectedToServer;
		Multiplayer.ConnectionFailed += ConnectionFailed;
	}

	void CreateServer(int port)
	{
		peer = new ENetMultiplayerPeer();
		Error error = peer.CreateServer(port,PlayerPrefs.GetBool("Hosting Online") ? maxPlayers : 1);
		if(error != Error.Ok)
		{
			GD.Print("Hosting Failed: " + error.ToString());
			return;
		}
		peer.Host.Compress(ENetConnection.CompressionMode.Zlib);
		Multiplayer.MultiplayerPeer = peer;

		GD.Print("Hosting Started");


		Rpc(nameof(SendPlayerInfo),PlayerPrefs.GetString("Name"),hostID);
	}

	void JoinServer(int port)
	{
		peer = new ENetMultiplayerPeer();
		peer.CreateClient(address,port);

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

		RpcId(hostID, nameof(SendPlayerInfo), PlayerPrefs.GetString("Name"),Multiplayer.GetUniqueId());
    }

     void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
    }


     void PeerConnected(long id)
    {
        GD.Print("Player Connected! " + id.ToString());
    }

	[Rpc(MultiplayerApi.RpcMode.AnyPeer)]
	void SendPlayerInfo(long id, string name)
	{	
		//Anyone can call server including server, only server can call client
		if(!Multiplayer.IsServer() && Multiplayer.GetRemoteSenderId() != hostID)
		{
			return;
		}

		//Check for duplicate call
		for(int i = 0; i < playerList.Count; i++)
		{
			if(playerList[i].id == id)
			{
				return;
			}
		}


		playerList.Add(new PlayerInfo(){id = id, name = name});

		if(Multiplayer.IsServer())
		{				
			RpcCallOnlyClientPlayerIDs(nameof(SendPlayerInfo), id, name);

			foreach (PlayerInfo item in playerList)
			{			
				RpcId(id, nameof(SendPlayerInfo), item.id, item.name);
			}
		}
	}

	void RpcCallOnlyClientPlayerIDs(StringName method, params Variant[] args)
	{
		for(int i = 0; i < playerList.Count; i++)
		{
			if(playerList[i].id != hostID)
			{
				RpcId(playerList[i].id, method, args);
			}
		}
	}

	/*
			//Method can be called only from Client to Server and Server to Client
		if((Multiplayer.IsServer() && Multiplayer.GetRemoteSenderId() == hostID) || 
			(!Multiplayer.IsServer() && Multiplayer.GetRemoteSenderId() != hostID))
		{
			return;
		}
	*/

	[ConsoleCommand("listplayers", Description = "Prints IDs and names of all currently connected players.")]
	void ListPlayers()
	{
		if(!IsInsideTree())
		{
			Console.Instance.Print("Not currently in game");
			return;
		}

		if(playerList.Count == 0)
		{
			Console.Instance.Print("No Players Connected");
		}
		string finalresult = "";
		for(int i = 0; i < playerList.Count; i++)
		{
			finalresult += playerList[index: i].name + " (ID " + playerList[index: i].id.ToString() + ")\n";
		}
		Console.Instance.Print(finalresult);
	}

	[ConsoleCommand("listmaxplayercount", Description = "Prints max players that can join. Singleplayer will be 1.")]
	void ListMaxPlayerCount()
	{
		Console.Instance.Print(PlayerPrefs.GetBool("Hosting Online") ? maxPlayers.ToString() : "1");
	}
}

public partial class PlayerInfo : GodotObject
{
	public string name;
	public long id;
	public CharacterBody3D playerObject;
}