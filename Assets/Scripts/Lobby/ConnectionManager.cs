using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine.SceneManagement;
using System;

public class ConnectionManager : MonoBehaviourPunCallbacks//,IConnectionCallbacks,IMatchmakingCallbacks
{
    private static ConnectionManager instance;

    public static ConnectionManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<ConnectionManager>();
            }

            return instance;

        }
    }
    const string KEY_GAME_START_TIME = "gst";
    public static double GameStartTime;

    public event Action OnSingleButtonClicked;
    public event Action OnMultiplayerButtonClicked;
    public event Action OnConnectionReadyInvoked;
    public event Action OnCreatedRoomInvoked;
    public event Action OnRoomJoinedInvoked;
    public event Action OnLeftRoomInvoked;
    public event Action OnConnectedInvoked;
    public event Action OnConnectedToMasterInvoked;
    public event Action OnGameToLoad;

    public void StartSinglePlayerMode()
    {
        OnSingleButtonClicked?.Invoke();
    }

    public void StartRandomPlayerMode()
    {
        OnMultiplayerButtonClicked?.Invoke();
    }

    public void LoadSinglePlayer()
    {
        PlayerPrefs.SetInt("MODE", 0); // Set Game  Single Player

        SceneManager.LoadScene("Game");
    }
    public void OnButtonClick()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            OnConnectionReadyInvoked?.Invoke();
            Debug.Log("ConnectionManager :: Start :: IsConnectedAndReady : True");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("ConnectionManager :: Start :: IsConnectedAndReady : False");
            if (PhotonNetwork.ConnectUsingSettings())
            {
                Debug.Log("ConnectionManager :: Start :: ConnectUsingSettings : True");
            }
            else
            {
                Debug.Log("ConnectionManager :: Start :: ConnectUsingSettings : False");
                //TODO: Handle this case
            }
        }

        PlayerPrefs.SetInt("MODE", 1); // Set Game Multiplayer
    }

    public override void OnConnected()
    {
        OnConnectedInvoked?.Invoke();
        Debug.Log("ConnectionManager :: OnConnected");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("ConnectionManager :: OnConnectedToMaster");
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.JoinRandomRoom();
        OnConnectedToMasterInvoked?.Invoke();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("ConnectionManager :: OnCreatedRoom");
        OnCreatedRoomInvoked?.Invoke();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.Log("ConnectionManager :: OnCreateRoomFailed");
    }

    public override void OnCustomAuthenticationFailed(string debugMessage)
    {
        Debug.Log("ConnectionManager :: OnCustomAuthenticationFailed");
    }

    public override void OnCustomAuthenticationResponse(Dictionary<string, object> data)
    {
        Debug.Log("ConnectionManager :: OnCustomAuthenticationResponse");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("ConnectionManager :: OnDisconnected");
    }

    public override void OnFriendListUpdate(List<FriendInfo> friendList)
    {
        Debug.Log("ConnectionManager :: OnFriendListUpdate");
    }

    public override void OnJoinedRoom()
    {
        OnRoomJoinedInvoked?.Invoke();
        Debug.Log("ConnectionManager :: OnJoinedRoom :: " + PhotonNetwork.CurrentRoom.Name);
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log("ConnectionManager :: OnJoinRandomFailed");

        Debug.Log("Creating Room");
        CreateRoom();
    }

    void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions() { MaxPlayers = 2 };
        PhotonNetwork.CreateRoom(null, roomOptions);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("ConnectionManager :: OnJoinRoomFailed");
    }

    public override void OnLeftRoom()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Lobby");
    }

    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("ConnectionManager :: OnRegionListReceived");
    }

    public void LeaveRoom()
    {
        Debug.Log("ConnectionManager :: LeaveRoom");
        PhotonNetwork.LeaveRoom();
    }

    void LoadGameScene()
    {
        OnGameToLoad?.Invoke();
        if(!PhotonNetwork.IsMasterClient)
        {
            Debug.Log("ConnectionManager :: LoadGameScene :: Not MasterClient. Returning from LoadGame method.");
            return;
        }

        GameStartTime = PhotonNetwork.Time;
        Debug.LogFormat("ConnectionManager :: LoadGameScene :: {0}", GameStartTime);

        ExitGames.Client.Photon.Hashtable hashtable = PhotonNetwork.CurrentRoom.CustomProperties;
        hashtable.Add(KEY_GAME_START_TIME, GameStartTime);

        PhotonNetwork.CurrentRoom.SetCustomProperties(hashtable);
        PhotonNetwork.SendAllOutgoingCommands();

        PhotonNetwork.LoadLevel("Game");
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.LogFormat("ConnectionManager :: OnPlayerEnteredRoom :: {0} entered room", newPlayer.NickName);

        Room room = PhotonNetwork.CurrentRoom;
        if (room.PlayerCount == room.MaxPlayers)
        {
            LoadGameScene();
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogFormat("ConnectionManager :: OnPlayerLeftRoom :: {0} left room", otherPlayer.NickName);

        Room room = PhotonNetwork.CurrentRoom;
        if (room.PlayerCount <= 1)
        {
            //TODO: Show win screen/ some UI to denote as last player in room
            LeaveRoom();
        }
    }

    public override void OnRoomPropertiesUpdate(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.LogFormat("ConnectionManager :: OnRoomPropertiesUpdate");
        if (propertiesThatChanged.ContainsKey(KEY_GAME_START_TIME))
        {
            GameStartTime = (double)propertiesThatChanged[KEY_GAME_START_TIME];
            Debug.LogFormat("ConnectionManager :: OnRoomPropertiesUpdate :: {0}", GameStartTime);
        }
    }
}
