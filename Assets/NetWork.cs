using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class NetWork : MonoBehaviour
{
	public class c_player
	{
		public c_player ()
		{
			
		}

		public c_player (string setLogin, string setPassword)
		{
			password = setPassword;
			login = setLogin;
			winRatio = 0;
			exp = 0;
		}
		
		public NetworkPlayer networkPlayer;
		public int exp;
		public float winRatio;
		public string password;
		public string login;
	}
	
	public class players
	{
		public int checkLoginPassword (string login, string password, NetworkPlayer networkPlayer)
		{
			for (int i=0; i<dict.Count; i++)
				if (dict [i].login == login)
				if (dict [i].password == password) {
					
					for (int j=0; j<onlineList.Count; j++)
						if (onlineList [j].login == login) {
							Network.CloseConnection (onlineList [j].networkPlayer, true);
							return constants.dobleLogining;
						}
					onlineList.Add (dict [i]);
					onlineList [i].networkPlayer = networkPlayer;
					return constants.ok;
				} else
					return constants.errorPassword;
			return constants.loginNotFound;
		}
		
		public void removeOnlinePlayer (NetworkPlayer player)
		{
			for (int i=0; i<onlineList.Count; i++)
				if (onlineList [i].networkPlayer == player) {
					onlineList.RemoveAt (i);
					break;
				}
		}
		
		public List<c_player> onlineList = new List<c_player> ();
		public List<c_player> dict = new List<c_player> ();
	}
	
	players playersInMemory = new players ();
	
	// Use this for initialization
	void Start ()
	{
		//print(Network.HavePublicAddress());
	}
	
	void prepareAndSendOnlinePlayerList ()
	{
		string onlinePlayersList = playersInMemory.onlineList [0].login;
		for (int i=1; i<playersInMemory.onlineList.Count; i++) {
			onlinePlayersList += "\n" + playersInMemory.onlineList [i].login;
		}
		networkView.RPC ("takeOnlineList", RPCMode.Others, playersInMemory.onlineList.Count, onlinePlayersList);
	}
	
	[RPC]
	void serverLogin (NetworkPlayer player, string login_login, string login_password)
	{
		//print("login = "+login+"\npssword = "+password);
		int loginstatus = playersInMemory.checkLoginPassword (login_login, login_password, player);
		networkView.RPC ("loginEnd", player, loginstatus);
		if (loginstatus == constants.ok) 
			prepareAndSendOnlinePlayerList ();
		if (loginstatus == constants.loginNotFound) {
			playersInMemory.dict.Add (new c_player (login_login, login_password));
			fSave ("111.txt", playersInMemory);
		}
		
	}
	
	void OnGUI ()
	{
		if (!Network.isClient && !Network.isServer) {
			if (GUI.Button (new Rect (10, 10, 100, 100), "server")) {
				playersInMemory = fOpen ("111.txt");
				playersInMemory.onlineList.Clear();
				Network.InitializeServer (100, 25565);
				MasterServer.RegisterHost ("TEstGame", "MyfirstHost");
			}
			
		}
		if (playersInMemory.onlineList != null)
			for (int i=0; i< playersInMemory.onlineList.Count; i++) {
				if (GUI.Button (new Rect (220, 10 + i * 110, 100, 20), playersInMemory.onlineList [i].login)) {
					//Network.Connect("213.222.243.25",25565);
				}
			}
	}
	
	void OnMasterServerEvent (MasterServerEvent msEvent)
	{
		if (msEvent == MasterServerEvent.RegistrationSucceeded)
			Debug.Log ("Server registered");
        
	}
	
	void OnPlayerConnected (NetworkPlayer player)
	{
		Debug.Log ("Player " + player.ToString () + " connected from " + player.ipAddress + ":" + player.port);
		networkView.RPC ("clientLogin", player, player);

	}

	void OnPlayerDisconnected (NetworkPlayer player)
	{
		/* Debug.Log("Clean up after player " + player);*/
		Network.RemoveRPCs (player);
		playersInMemory.removeOnlinePlayer (player);
		prepareAndSendOnlinePlayerList();
		/* Network.DestroyPlayerObjects(player);*/
		//onlinePlayerListServer.Remove (player);
	}

	void OnServerInitialized ()
	{
		Debug.Log ("Server initialized and ready");
	}
	
	// Update is called once per frame
	void Update ()
	{
		/*if(connecting)
		{
			if(MasterServer.PollHostList().Length>0)
			{
				connecting=false;
				print("servers = "+MasterServer.PollHostList().Length);
				hostData=MasterServer.PollHostList();
			}
		}*/
		
	}
	
	void fSave (string path, players saving_players)
	{
		
		Stream writer;
		XmlSerializer serialWrite = new XmlSerializer (typeof(players));
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
			writer = new FileStream (path, FileMode.Create, FileAccess.Write);
		else
			writer = new FileStream (Application.persistentDataPath + "/" + path, FileMode.Create, FileAccess.Write);
		serialWrite.Serialize (writer, saving_players);
		writer.Close ();
	}
	
	players fOpen (string path)
	{
		players level = new players ();
		XmlSerializer preferences = new XmlSerializer (typeof(players));
		Stream reader;
		try {
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
				reader = new FileStream (path, FileMode.Open, FileAccess.Read);
			else
				reader = new FileStream (Application.persistentDataPath + "/" + path, FileMode.Open, FileAccess.Read);
		} catch (System.Exception ex) {
			fSave (path, level);
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor || Application.platform == RuntimePlatform.WindowsPlayer)
				reader = new FileStream (path, FileMode.Open, FileAccess.Read);
			else
				reader = new FileStream (Application.persistentDataPath + "/" + path, FileMode.Open, FileAccess.Read);
		}
		level = (players)preferences.Deserialize (reader);
		reader.Close ();
		return level;
	}
}
