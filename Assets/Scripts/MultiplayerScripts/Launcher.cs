using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;
using Photon.Realtime;
using System.Linq;
using Firebase.Extensions;
using System;
using System.Threading.Tasks;
using Firebase.Firestore;
using Firebase.Auth;

public class Launcher : MonoBehaviourPunCallbacks
{
    public static Launcher Instance;

    [SerializeField] TMP_InputField roomNameInputField;

    [SerializeField] TextMeshProUGUI roomNameText;

    [SerializeField] TextMeshProUGUI errorText;

    [SerializeField] Transform roomListContent;

    [SerializeField] GameObject roomListItemPrefab;

    [SerializeField] GameObject playerListItemPrefab;

    [SerializeField] Transform playerListContent;

    public GameObject startButton;


    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Debug.Log("Connecting to Master...");
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        PhotonNetwork.JoinLobby();
        PhotonNetwork.AutomaticallySyncScene = true;


    }

    public override void OnJoinedLobby()
    {
        MenuManager.instance.OpenMenu("TitleMenu");
        Debug.Log("Joined Lobby");

        string email = Display.text;

        FetchUserDataByEmail(email, (userData) =>
        {
            if (userData != null)
            {
                // Kullan?c? ad? de?erini al
                if (userData.TryGetValue("userName", out object userName))
                {
                    PhotonNetwork.NickName = userName as string; // Kullan?c? ad? ile g�ncelle
                    Debug.Log("User NickName set to: " + PhotonNetwork.NickName);
                }
                else
                {
                    Debug.Log("User name is empty or not found.");
                }
            }
            else
            {
                Debug.Log("Failed to fetch user data.");
            }
        });



       
    }



    public void CreateRoom()
    {
        if (string.IsNullOrEmpty(roomNameInputField.text))
        {
            return;
        }
        PhotonNetwork.CreateRoom(roomNameInputField.text);
        MenuManager.instance.OpenMenu("LoadingMenu");


    }

    public override void OnJoinedRoom()
    {
        MenuManager.instance.OpenMenu("RoomMenu");
        roomNameText.text = PhotonNetwork.CurrentRoom.Name;

        Player[] players = PhotonNetwork.PlayerList;

        foreach (Transform child in playerListContent)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < players.Count(); i++)
        {
            Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(players[i]);

        }

        startButton.SetActive(PhotonNetwork.IsMasterClient);

    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        startButton.SetActive(PhotonNetwork.IsMasterClient);
    }
    public override void OnCreateRoomFailed(short returnCode, string errormessage)
    {
        errorText.text = "Room Generation Unsuccesfull" + errormessage;
        MenuManager.instance.OpenMenu("ErrorMenu");

    }

    public void JoinRoom(RoomInfo info)
    {
        PhotonNetwork.JoinRoom(info.Name);
        MenuManager.instance.OpenMenu("LoadingMenu");
    }
    public void StartGame()
    {
        PhotonNetwork.LoadLevel(1);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
        MenuManager.instance.OpenMenu("LoadingMenu");
    }
    public override void OnLeftRoom()
    {
        MenuManager.instance.OpenMenu("TitleMenu");
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        foreach (Transform trans in roomListContent)
        {
            Destroy(trans.gameObject);
        }
        for (int i = 0; i < roomList.Count; i++)
        {
            if (roomList[i].RemovedFromList)
                continue;
            Instantiate(roomListItemPrefab, roomListContent).GetComponent<RoomListItem>().SetUp(roomList[i]);
        }
    }

    public override void OnPlayerEnteredRoom(Player newplayer)
    {
        Instantiate(playerListItemPrefab, playerListContent).GetComponent<PlayerListItem>().SetUp(newplayer);
    }


    void FetchUserDataByEmail(string email, Action<Dictionary<string, object>> callback)
    {
        var db = Firebase.Firestore.FirebaseFirestore.DefaultInstance;

        db.Collection("users")
            .WhereEqualTo("email", email)  // E-posta ile e?le?en belgeleri al
            .GetSnapshotAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("Failed to retrieve user data: " + task.Exception);
                    callback(null); // Hata durumunda geri �a?r?y? tetikle
                    return;
                }

                var snapshot = task.Result;
                if (snapshot.Count > 0)
                {
                    foreach (var document in snapshot.Documents)
                    {
                        // Kullan?c? verilerini almak i�in Dictionary olu?tur
                        var userData = new Dictionary<string, object>
                        {
                        { "userName", document.GetValue<string>("userName") },
                        { "email", document.GetValue<string>("email") }
                        };

                        // Geri �a?r?y? tetikle
                        callback(userData);
                        return;
                    }
                }
                else
                {
                    Debug.Log("No user found with the provided email.");
                    callback(null); // Kullan?c? bulunamad???nda geri �a?r?y? tetikle
                }
            });
    }
}