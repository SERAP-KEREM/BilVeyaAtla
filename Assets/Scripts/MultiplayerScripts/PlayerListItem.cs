using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Realtime;
using Photon.Pun;
using TMPro;

public class PlayerListItem : MonoBehaviourPunCallbacks
{

    public TextMeshProUGUI playerUsername;

    Player player;

    public void SetUp(Player _player)
    {
        player = _player;
        playerUsername.text = PlayerPrefs.GetString("UserName");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        if (player == otherPlayer)
        {
            Destroy(gameObject);
        }
    }

    public override void OnLeftRoom()
    {
        Destroy(gameObject);
    }
}
   