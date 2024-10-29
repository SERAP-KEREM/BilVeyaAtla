using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// PlayerManager s?n?f?, �ok oyunculu bilgi yar??mas?nda oyuncular?n puanlar?n? y�netir.
/// </summary>
public class PlayerManager : MonoBehaviourPunCallbacks
{
    // Her oyuncunun puanlar?n? tutan bir s�zl�k
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

    /// <summary>
    /// Oyuncu kat?ld???nda �a?r?l?r.
    /// </summary>
    /// <param name="playerId">Kat?lan oyuncunun kimli?i.</param>
    public void OnPlayerJoined(int playerId)
    {
        // Yeni oyuncunun ba?lang?� puan?n? 0 olarak ayarla
        playerScores[playerId] = 0;
    }

    /// <summary>
    /// Oyuncunun puan?n? g�nceller.
    /// </summary>
    /// <param name="playerId">Puan? g�ncellenecek oyuncunun kimli?i.</param>
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

            // Puan g�ncelleme i?lemini di?er oyunculara senkronize et
            photonView.RPC("SyncPlayerScore", RpcTarget.Others, playerId, playerScores[playerId]);
        }
    }

    /// <summary>
    /// Di?er oyuncular?n puanlar?n? senkronize etmek i�in RPC metodu.
    /// </summary>
    /// <param name="playerId">Puan g�ncellenecek oyuncunun kimli?i.</param>
    /// <param name="newScore">Yeni puan de?eri.</param>
    [PunRPC]
    private void SyncPlayerScore(int playerId, int newScore)
    {
        playerScores[playerId] = newScore; // Di?er oyuncunun puan?n? g�ncelle
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
