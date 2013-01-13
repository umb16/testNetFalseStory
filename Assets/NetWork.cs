using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
public class NetWork : MonoBehaviour {
	
	//NetworkPlayer Player;
	bool connecting=false;
	string login="";
	string password="";
	
	Dictionary<NetworkPlayer, string> onlinePlayerListServer = new Dictionary<NetworkPlayer,string>();
	string[] onlinePlayerListClient = new string[0];
	
	public class player{
		public player(){
			
		}
		public player(string setLogin,NetworkPlayer setPlayer, string setPassword)
		{
			networkPlayer=setPlayer;
			password=setPassword;
			login=setLogin;
		}
		public NetworkPlayer networkPlayer;
		public string password;
		public string login;
	}
	
	public class players
	{
		public bool checkLoginPassword(string login,string password)
		{
			for(int i=0;i<dict.Count;i++)
				if(dict[i].login==login)
					if(dict[i].password==password)
					return true;
			return false;
		}
		public List<player> dict = new List<player>();
	}
	
	players playersInMemory = new players();
	
	// Use this for initialization
	void Start () {
	
		
		//print(Network.HavePublicAddress());
	}
	
	void prepareToSendOnlinePlayerList()
	{
	/*	onlinePlayerListClient = new string[onlinePlayerListServer.Count];
		for(int i=0; i<onlinePlayerListServer.Count;i++)
			onlinePlayerListClient=onlinePlayerListServer [i];*/
	}
	
	void sendOnlineToClients()
	{
		
	}
	
	void fSave (string path, players saving_players)
	{
		
		Stream writer;
		XmlSerializer serialWrite = new XmlSerializer (typeof(players));
		if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor||Application.platform == RuntimePlatform.WindowsPlayer)
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
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor||Application.platform == RuntimePlatform.WindowsPlayer)
				reader = new FileStream (path, FileMode.Open, FileAccess.Read);
			else
				reader = new FileStream (Application.persistentDataPath + "/" + path, FileMode.Open, FileAccess.Read);
		} catch (System.Exception ex) {
			fSave (path, level);
			if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor||Application.platform == RuntimePlatform.WindowsPlayer)
				reader = new FileStream (path, FileMode.Open, FileAccess.Read);
			else
				reader = new FileStream (Application.persistentDataPath + "/" + path, FileMode.Open, FileAccess.Read);
		}
		level = (players)preferences.Deserialize (reader);
		reader.Close ();
		return level;
	}
	
	[RPC]
	void clientLogin(NetworkPlayer player)
	{
		print("connect to server succes");
		Debug.Log("Player " + player.ToString() + " connected from " + player.ipAddress + ":" + player.port);
		networkView.RPC("serverLogin", RPCMode.Server,player,login,password);
	}
	[RPC]
	void serverLogin(NetworkPlayer player,string login_login, string login_password)
	{
		//print("login = "+login+"\npssword = "+password);
		if(playersInMemory.checkLoginPassword(login_login,login_password))
		{
				onlinePlayerListServer.Add(player,login_login);
				networkView.RPC("loginOK", player);
		}
		else
		{
			playersInMemory.dict.Add(new player(login_login,player,login_password));
			fSave("111.txt",playersInMemory);
		}
		
	}
	[RPC]
	void loginOK()
	{
		
		connecting=false;
		print("login ok");
	}
	
	void OnGUI()
	{
		if(!Network.isClient&&!Network.isServer&&!connecting)
		{
			if(GUI.Button(new Rect(10,10,100,100),"server"))
			{
				playersInMemory=fOpen("111.txt");
				Network.InitializeServer(100,25565);
				MasterServer.RegisterHost("TEstGame","MyfirstHost");
			}
			GUI.Label(new Rect(Screen.width/2-70,Screen.height/3.8f,140,25),"Login:");
			login=GUI.TextField(new Rect(Screen.width/2-70,Screen.height/3,140,25),login,20);
			GUI.Label(new Rect(Screen.width/2-70,Screen.height/2.3f,140,25),"Password:");
			password=GUI.PasswordField(new Rect(Screen.width/2-70,Screen.height/2f,140,25),password,'*',20);
			if(GUI.Button(new Rect(Screen.width/2-50,Screen.height/1.5f,100,30),"login"))
			{
				if(password!=""&&login!="")
					preLoginFunc();
				//refresh();
				//Network.
			}
			
/*			for(int i=0;i<hostData.Length;i++)
			{
				if(GUI.Button(new Rect(220,10+i*110,100,100),hostData[i].gameName))
				{
					Network.Connect("213.222.243.25",25565);
				}
			}*/
		}
		if(connecting)
		{
			GUI.Label(new Rect(Screen.width/2-70,Screen.height/2,140,25),"CONNECTING");
		}
	}
	
	void preLoginFunc()
	{
		Network.Connect("213.222.243.25",25565);
		connecting=true;
		
	}
	
	 void OnMasterServerEvent(MasterServerEvent msEvent) {
        if (msEvent == MasterServerEvent.RegistrationSucceeded)
            Debug.Log("Server registered");
        
    }
	
    void OnPlayerConnected(NetworkPlayer player) {
        Debug.Log("Player " + player.ToString() + " connected from " + player.ipAddress + ":" + player.port);
        networkView.RPC("clientLogin", player,player);

    }
	   void OnPlayerDisconnected(NetworkPlayer player) {
       /* Debug.Log("Clean up after player " + player);*/
        Network.RemoveRPCs(player);
       /* Network.DestroyPlayerObjects(player);*/
		onlinePlayerListServer.Remove(player);
    }
	void OnServerInitialized() {
        Debug.Log("Server initialized and ready");
    }
	 void OnConnectedToServer() {
        Debug.Log("Connected to server");
    }
	  void OnFailedToConnect(NetworkConnectionError error) {
        Debug.Log("Could not connect to server: " + error);
    }
	// Update is called once per frame
	void Update () {
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
}
