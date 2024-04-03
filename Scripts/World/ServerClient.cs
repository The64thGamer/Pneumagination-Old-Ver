using Godot;
using System;

public partial class ServerClient : Node
{

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

    private void ConnectionFailed()
    {
		GD.Print("CONNECTION FAILED");
    }

    private void ConnectedToServer()
    {
        GD.Print("Connected To Server");
    }

    private void PeerDisconnected(long id)
    {
        GD.Print("Player Disconnected: " + id.ToString());
    }


    private void PeerConnected(long id)
    {
        GD.Print("Player Connected! " + id.ToString());
    }


}
