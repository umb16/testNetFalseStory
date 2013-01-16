using UnityEngine;
using System.Collections;

public class client : MonoBehaviour
{
	
	public GameObject battle;
	
	int loginstatus = constants.zero;
	string login = "";
	string password = "";
	server.c_clientsOnlineList onlinePlayersList = new server.c_clientsOnlineList();
	//int[] onlinePlayersIdList;
	int battleId=-1;
	string stringMsg="";
	int order=0;
	server.c_player clientPlayer = new server.c_player();
	NetworkPlayer clientNetPlayer;
	
	// Use this for initialization
	void Start ()
	{
	
	}

	[RPC]
	void clientLogin (NetworkPlayer player)
	{
		print ("connect to server succes");
		Debug.Log ("Player " + player.ToString () + " connected from " + player.ipAddress + ":" + player.port);
		clientNetPlayer=player;
		networkView.RPC ("serverLogin", RPCMode.Server, player, login, password);
	}

	[RPC]
	void loginEnd (int loginCode)
	{
		loginstatus = loginCode;
		print ("loginEnd"+loginstatus);
	}
	[RPC]
	void takePlayerInfo(bool self,string playerInfo)
	{
		if(self)
		{
			clientPlayer = (server.c_player) serialize.deserialization(clientPlayer,playerInfo);
			
		}
	}
	
	[RPC]
	void takeInvite(int tbattleId,string inviter,int torder)
	{
		if(loginstatus!=constants.waitingForReply)
		{
			order=torder;
			battleId=tbattleId;
			loginstatus= constants.inviteToGame;
			stringMsg=inviter+" invite you to play";
		}
	}
	
	[RPC]
	void takeOnlineList(string o)
	{
		onlinePlayersList=(server.c_clientsOnlineList)serialize.deserialization(onlinePlayersList,o);
	}

	[RPC]
	void receiver(int type, string o)
	{
		if(type == constants.redy)
		{
			loginstatus=constants.redy;
			battle.SetActive(true);
		}
	}
	
	void OnGUI ()
	{
		//print(loginstatus);
		GUI.color = Color.black;
		if (!Network.isServer && loginstatus==constants.zero) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 3.8f, 140, 25), "Login:");
			login = GUI.TextField (new Rect (Screen.width / 2 - 70, Screen.height / 3, 140, 25), login, 20);
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2.3f, 140, 25), "Password:");
			password = GUI.PasswordField (new Rect (Screen.width / 2 - 70, Screen.height / 2f, 140, 25), password, '*', 20);
			if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 1.5f, 100, 30), "login")) {
				if (password != "" && login != "")
					preLoginFunc ();
			}
		}
		if (loginstatus == constants.logining) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "CONNECTING");
		}
		if (loginstatus == constants.errorPassword) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "Password error");
		}
		if (loginstatus == constants.connectionError) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "Connection error");
		}
		if (loginstatus == constants.loginNotFound) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "Account is created");
		}
		if (loginstatus == constants.dobleLogining) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "DOBLE LOGINING");
		}
		if (loginstatus == constants.disconnectedFromSrver) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "Diconnected from the server");
		}
		if (loginstatus == constants.waitingForReply) {
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), "WAITING OPPONENT");
		}
		if (loginstatus == constants.inviteToGame) {
			if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 1.5f, 100, 30), "OK")) {
				loginstatus= constants.waitingForReply;
				networkView.RPC ("battleReceiver",RPCMode.Server,battleId,order,constants.redy);
			}
			GUI.Label (new Rect (Screen.width / 2 - 100, Screen.height / 2, 200, 100), stringMsg);
		}
		if (loginstatus == constants.disconnectedFromSrver||loginstatus == constants.errorPassword||loginstatus == constants.connectionError||loginstatus == constants.loginNotFound||loginstatus == constants.dobleLogining) 
		if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 1.5f, 100, 30), "OK")) {
			Network.Disconnect();
				loginstatus= constants.zero;
			}
		if (loginstatus == constants.ok) {
			GUI.Label (new Rect (10, 10, 200, 25), login);
		if (onlinePlayersList != null)
				GUI.Label (new Rect (Screen.width*.5f-50, 10, 100, 25), "Players online:");
			for (int i=0; i< onlinePlayersList.list.Count; i++) {
				if( onlinePlayersList.list[i].login!=login)
				if (GUI.Button (new Rect (Screen.width*.15f, 100 + i * 22, Screen.width/1.5f, 20), onlinePlayersList.list[i].login)) {
					//Network.Connect("213.222.243.25",25565);
					//waitingPlayer=onlinePlayersIdList[i];
					networkView.RPC ("sendInvite", RPCMode.Server, onlinePlayersList.list[i].id, clientPlayer.id);
					loginstatus= constants.waitingForReply;
					order=0;
				}
			}
			if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 1.5f, 100, 30), "LOGOUT")) {
			Network.Disconnect();
				loginstatus= constants.zero;
			}
		}
	}

	void preLoginFunc ()
	{
		Network.Connect ("213.222.243.25", 25565);
		loginstatus = constants.logining;
		
	}

	void OnConnectedToServer ()
	{
		Debug.Log ("Connected to server");
	}
	
	void OnDisconnectedFromServer(NetworkDisconnection info) {
        if (Network.isServer)
            Debug.Log("Local server connection disconnected");
        else
            if (info == NetworkDisconnection.LostConnection)
                Debug.Log("Lost connection to the server");
            else
		{
				loginstatus = constants.disconnectedFromSrver;
                Debug.Log("Successfully diconnected from the server");
		}
    }
	
	void OnFailedToConnect (NetworkConnectionError error)
	{
		loginstatus = constants.connectionError;
		Debug.Log ("Could not connect to server: " + error);
	}
	// Update is called once per frame
	void Update ()
	{
	
	}
}
