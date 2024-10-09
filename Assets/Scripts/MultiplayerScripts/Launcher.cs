using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
public class Launcher : MonoBehaviourPunCallbacks
{
    
    void Start()
    {
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        OnJoinedLobby();

    }

   public override void OnJoinedLobby()
    {

        MenuManager.instance.OpenMenu("TitleMenu");
        Debug.Log("Joined Lobby");
    }


}
