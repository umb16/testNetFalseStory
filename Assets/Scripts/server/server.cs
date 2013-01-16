using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

public class server : MonoBehaviour
{
	float secTimer = 0;
	float minTimer = 0;
	public class c_battle
	{
		public c_battle ()
		{
			fight = false;
			timer = 200;
		}

		public c_battle (int setBattleId, NetworkPlayer[] setPlayers, int[] setIds)
		{
			players = setPlayers;
			battleId = setBattleId;
			ids = setIds;
			fight = false;
			timer = 200;
			redy = new bool[]{false,false};
		}

		public int battleId;
		public NetworkPlayer[] players;
		public bool fight;
		public int[] ids;
		public bool[] redy;
		public float timer;
	}
	
	public class c_player
	{
		public c_player ()
		{
			//playerIntParams = new int[]{0,0,100,100,0};
		}

		public c_player (string setLogin, string setPassword, int setId)
		{
			password = setPassword;
			login = setLogin;
			id = setId;
			hp = 100;
			maxHp = 100;
		}

		public int id;
		public int exp;
		public float winratio;
		public float hp;
		public float maxHp;
		public string password;
		public string login;
	}
	
	public class c_onlinePlayers
	{
		public c_onlinePlayers ()
		{
			
		}

		public c_onlinePlayers (int setId, NetworkPlayer setNetworkPlayer)
		{
			id = setId;
			networkPlayer = setNetworkPlayer;
		}

		public int id;
		public NetworkPlayer networkPlayer;
	}
	
	public class c_clientsOnlineList
	{
		public List<c_player> list = new List<c_player> ();
	}
	
	public class players
	{
		public int checkLoginPassword (string login, string password, NetworkPlayer networkPlayer)
		{
			for (int i=0; i<dict.Count; i++)
				if (dict [i].login == login)
				if (dict [i].password == password) {
					
					for (int j=0; j<onlineList.Count; j++)
						if (onlineList [j].id == i) {
							Network.CloseConnection (onlineList [j].networkPlayer, true);
							return constants.dobleLogining;
						}
					onlineList.Add (new c_onlinePlayers (i, networkPlayer));
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
		
		public c_player getOnlinePlayer (int index)
		{
			if (onlineList [index] == null)
				return null;
			return dict [onlineList [index].id];
		}
		
		//public List<c_player> onlineList = new List<c_player> ();
		public List<c_onlinePlayers> onlineList = new List<c_onlinePlayers> ();
		public List<c_player> dict = new List<c_player> ();
	}
	
	int battleCount = 0;
	players playersInMemory = new players ();
	List <c_battle> battles = new List<c_battle> ();
	
	// Use this for initialization
	void Start ()
	{
		//clearDb();
		//print(Network.HavePublicAddress());
	}
	
	c_battle getBattle (int id)
	{
		for (int i=0; i<battles.Count; i++)
			if (battles [i].battleId == id)
				return battles [i];
		return null;
	}
	
	void prepareAndSendOnlinePlayerList ()
	{
		c_clientsOnlineList temp = new c_clientsOnlineList();
		for(int i=0;i<playersInMemory.onlineList.Count;i++)
			temp.list.Add(playersInMemory.dict[playersInMemory.onlineList[i].id]);
		networkView.RPC ("takeOnlineList", RPCMode.Others, serialize.serialization(temp));
	}
	
	[RPC]
	void serverLogin (NetworkPlayer player, string login_login, string login_password)
	{
		//print("login = "+login+"\npssword = "+password);
		int loginstatus = playersInMemory.checkLoginPassword (login_login, login_password, player);
		networkView.RPC ("loginEnd", player, loginstatus);
		if (loginstatus == constants.ok) {
			
			networkView.RPC ("takePlayerInfo", player, true, serialize.serialization (playersInMemory.getOnlinePlayer (playersInMemory.onlineList.Count - 1)));
			prepareAndSendOnlinePlayerList ();
		}
		if (loginstatus == constants.loginNotFound) {
			playersInMemory.dict.Add (new c_player (login_login, login_password, playersInMemory.dict.Count));
			serialize.serialization ("111.txt", playersInMemory);
		}
		
	}
	
	[RPC]
	void getPlayerInfo (int id, NetworkPlayer sender)
	{
		//networkView.RPC ("takePlayerInfo", sender, false, playersInMemory.dict [id].playerIntParams);
	}
	
	[RPC]
	void sendInvite (int recipientId, int senderId)
	{
		NetworkPlayer[] temp = new NetworkPlayer[2];
		for (int i=0; i<playersInMemory.onlineList.Count; i++) {
			if (playersInMemory.onlineList [i].id == recipientId)
				temp [1] = playersInMemory.onlineList [i].networkPlayer;
			if (playersInMemory.onlineList [i].id == senderId)
				temp [0] = playersInMemory.onlineList [i].networkPlayer;
		}
		if (temp [0] != null && temp [1] != null) {
			battles.Add (new c_battle (battleCount, temp, new int[]{senderId,recipientId}));
			battles [battles.Count - 1].redy [0] = true;
			networkView.RPC ("takeInvite", temp [1], battleCount, playersInMemory.dict [senderId].login, 1);
			battleCount++;
		}
	}
	
	[RPC]
	void battleReceiver (int battleId, int order, int command)
	{
		print("battleReceiver"+command);
		if (command == constants.redy)
		{
			getBattle (battleId).redy [order] = true;
			print(playersInMemory.dict[getBattle (battleId).ids[order]].login+" - redy");
		}
	}
	
	void menageBattles ()
	{
		int readyCount = 0;
		for (int i=0; i<battles.Count; i++) {
			readyCount = 0;
			battles [i].timer -= secTimer;
			for (int j=0; j<battles[i].redy.Length; j++)
				if (battles [i].redy [j])
					readyCount++;
			if (readyCount == battles [i].redy.Length) {
				if (!battles [i].fight) {
					for (int j = 0; j< battles[i].players.Length; j++) {
						battles [i].fight=true;
						networkView.RPC ("receiver", battles [i].players [j], constants.redy,"null");
					}
				}
			}
		}
	}
	
	void clearDb()
	{
		playersInMemory.dict.Clear();
		serialize.serialization("111.txt",playersInMemory);
	}
	
	void onesOf500ms ()
	{
		serialize.serialization ("111.txt", playersInMemory);
	}
	
	void Update ()
	{
		minTimer += Time.deltaTime;
		if (minTimer > 500) {
			onesOf500ms ();
			minTimer = 0;
		}
		
		secTimer += Time.deltaTime;
		if (secTimer > 1) {
			menageBattles ();
			secTimer = 0;
			//print(serialize.serialization(playersInMemory));
		}
	}
	
	void OnGUI ()
	{
		GUI.color = Color.black;
		if (!Network.isClient && !Network.isServer) {
			if (GUI.Button (new Rect (10, 10, 100, 100), "server")) {
				playersInMemory = (players)serialize.deserialization ("111.txt", playersInMemory);
				playersInMemory.onlineList.Clear ();
				Network.InitializeServer (100, 25565);
				MasterServer.RegisterHost ("TEstGame", "MyfirstHost");
			}
			
		}
		if (playersInMemory.onlineList != null)
			for (int i=0; i< playersInMemory.onlineList.Count; i++) {
				if (GUI.Button (new Rect (Screen.width * .15f, 10 + i * 22, Screen.width / 1.5f, 20), playersInMemory.getOnlinePlayer (i).login.ToString ())) {
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
		prepareAndSendOnlinePlayerList ();
		/* Network.DestroyPlayerObjects(player);*/
		//onlinePlayerListServer.Remove (player);
	}

	void OnServerInitialized ()
	{
		Debug.Log ("Server initialized and ready");
	}
	
	// Update is called once per frame
	
	
	/*void fSave (string path, players saving_players)
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
	}*/
}
