using UnityEngine;
using System.Collections;

public class client : MonoBehaviour
{
	
	int loginstatus = constants.zero;
	string login = "";
	string password = "";
	string[] onlinePlayersList = new string[0];
	NetWork.c_player clientPlayer;
	// Use this for initialization
	void Start ()
	{
	
	}

	[RPC]
	void clientLogin (NetworkPlayer player)
	{
		print ("connect to server succes");
		Debug.Log ("Player " + player.ToString () + " connected from " + player.ipAddress + ":" + player.port);
		networkView.RPC ("serverLogin", RPCMode.Server, player, login, password);
	}

	[RPC]
	void loginEnd (int loginCode)
	{
		loginstatus = loginCode;
		print (loginstatus);
	}
	
	[RPC]
	void takeOnlineList(int onlineNumber, string onlineList)
	{
		onlinePlayersList = new string[onlineNumber];
		int j=0;
		for(int i=0;i<onlineList.Length;i++)
		{
			if(onlineList[i]!='\n')
			onlinePlayersList[j]+=onlineList[i];
			else
			{
				j++;
			}
		}
	}
	void OnGUI ()
	{
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
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "CONNECTING");
		}
		if (loginstatus == constants.errorPassword) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "Password error");
		}
		if (loginstatus == constants.connectionError) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "Connection error");
		}
		if (loginstatus == constants.loginNotFound) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "Account is created");
		}
		if (loginstatus == constants.dobleLogining) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "DOBLE LOGINING");
		}
		if (loginstatus == constants.disconnectedFromSrver) {
			GUI.Label (new Rect (Screen.width / 2 - 70, Screen.height / 2, 140, 25), "Diconnected from the server");
		}
		if (loginstatus == constants.disconnectedFromSrver||loginstatus == constants.errorPassword||loginstatus == constants.connectionError||loginstatus == constants.loginNotFound||loginstatus == constants.dobleLogining) 
		if (GUI.Button (new Rect (Screen.width / 2 - 50, Screen.height / 1.5f, 100, 30), "OK")) {
			Network.Disconnect();
				loginstatus= constants.zero;
			}
		if (loginstatus == constants.ok) {
		if (onlinePlayersList != null)
			for (int i=0; i< onlinePlayersList.Length; i++) {
				if (GUI.Button (new Rect (220, 10 + i * 22, 100, 20), onlinePlayersList[i])) {
					//Network.Connect("213.222.243.25",25565);
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
