using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerManager s?n?f?, çok oyunculu bilgi yar??mas?nda oyuncular?n puanlar?n? yönetir.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    // Her oyuncunun puanlar?n? tutan bir sözlük
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

    /// <summary>
    /// Oyuncu kat?ld???nda ça?r?l?r.
    /// </summary>
    /// <param name="playerId">Kat?lan oyuncunun kimli?i.</param>
    public void OnPlayerJoined(int playerId)
    {
        // Yeni oyuncunun ba?lang?ç puan?n? 0 olarak ayarla
        playerScores[playerId] = 0;
    }

    /// <summary>
    /// Oyuncunun puan?n? günceller.
    /// </summary>
    /// <param name="playerId">Puan? güncellenecek oyuncunun kimli?i.</param>
    /// <param name="isCorrect">Cevab?n do?rulu?unu belirten bir boolean.</param>
    public void UpdatePlayerScore(int playerId, bool isCorrect)
    {
        if (playerScores.ContainsKey(playerId))
        {
            if (isCorrect)
            {
                playerScores[playerId]++; // Do?ru cevap durumunda puan? art?r
            }
            else
            {
                playerScores[playerId]--; // Yanl?? cevap durumunda puan? azalt
            }

            // Puan güncelleme i?lemini di?er oyunculara senkronize et
            photonView.RPC("SyncPlayerScore", RpcTarget.Others, playerId, playerScores[playerId]);
        }
    }

    /// <summary>
    /// Di?er oyuncular?n puanlar?n? senkronize etmek için RPC metodu.
    /// </summary>
    /// <param name="playerId">Puan güncellenecek oyuncunun kimli?i.</param>
    /// <param name="newScore">Yeni puan de?eri.</param>
    [PunRPC]
    private void SyncPlayerScore(int playerId, int newScore)
    {
        playerScores[playerId] = newScore; // Di?er oyuncunun puan?n? güncelle
    }

    /// <summary>
    /// Belirli bir oyuncunun puan?n? al?r.
    /// </summary>
    /// <param name="playerId">Puan? al?nacak oyuncunun kimli?i.</param>
    /// <returns>Oyuncunun puan?.</returns>
    public int GetPlayerScore(int playerId)
    {
        return playerScores.ContainsKey(playerId) ? playerScores[playerId] : 0;
    }
}
