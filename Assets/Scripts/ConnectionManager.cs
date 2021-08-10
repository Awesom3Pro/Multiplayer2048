using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
public class ConnectionManager : MonoBehaviourPunCallbacks//,IConnectionCallbacks,IMatchmakingCallbacks
{
    void Start()
    {
        if (PhotonNetwork.IsConnectedAndReady)
        {
            Debug.Log("ConnectionManager :: Start :: IsConnectedAndReady : True");
            PhotonNetwork.JoinRandomRoom();
        }
        else
        {
            Debug.Log("ConnectionManager :: Start :: IsConnectedAndReady : False");
            if(PhotonNetwork.ConnectUsingSettings())
            {
                Debug.Log("ConnectionManager :: Start :: ConnectUsingSettings : True");
            }
            else
            {
                Debug.Log("ConnectionManager :: Start :: ConnectUsingSettings : False");
            }
        }
    }

    public override void OnConnected()
    {
        Debug.Log("ConnectionManager :: OnConnected");
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("ConnectionManager :: OnConnectedToMaster");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("ConnectionManager :: OnCreatedRoom");
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
        PhotonNetwork.CreateRoom(null, new RoomOptions());
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        Debug.Log("ConnectionManager :: OnJoinRoomFailed");
    }

    public override void OnLeftRoom()
    {
        Debug.Log("ConnectionManager :: OnLeftRoom");
    }

    public override void OnRegionListReceived(RegionHandler regionHandler)
    {
        Debug.Log("ConnectionManager :: OnRegionListReceived");
    }
}
