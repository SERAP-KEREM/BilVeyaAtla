using UnityEngine;
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

      
        playerUsername.text = player.IsLocal ? PhotonNetwork.LocalPlayer.NickName : player.NickName;
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
