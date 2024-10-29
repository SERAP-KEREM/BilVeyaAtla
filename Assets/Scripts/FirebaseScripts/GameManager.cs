using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

/// <summary>
/// GameManager sınıfı, çok oyunculu bilgi yarışmasında oyunun akışını yönetir.
/// </summary>
public class GameManager : MonoBehaviourPunCallbacks
{
    private PlayerManager playerManager; // PlayerManager referansı

    private void Awake()
    {
        playerManager = GetComponent<PlayerManager>(); // PlayerManager bileşenini al
    }

    /// <summary>
    /// Oyun başladığında çağrılır.
    /// </summary>
    public void StartGame()
    {
        // Oyun başlangıç işlemleri
        // Her oyuncunun katılımını kontrol et
        foreach (Player player in PhotonNetwork.PlayerList)
        {
            playerManager.OnPlayerJoined(player.ActorNumber); // Her oyuncu için PlayerManager'da kaydet
        }

        // Oyunun geri kalan başlangıç mantığını buraya ekleyebilirsiniz.
    }

    /// <summary>
    /// Oyuncunun cevabını kontrol eder.
    /// </summary>
    /// <param name="playerId">Cevap veren oyuncunun kimliği.</param>
    /// <param name="isCorrect">Cevabın doğruluğunu belirten bir boolean.</param>
    public void CheckAnswer(int playerId, bool isCorrect)
    {
        playerManager.UpdatePlayerScore(playerId, isCorrect); // Oyuncunun puanını güncelle

        // Sonuçları diğer oyunculara bildirin
        photonView.RPC("NotifyOthers", RpcTarget.Others, playerId, isCorrect);
    }

    /// <summary>
    /// Diğer oyunculara cevabın sonucunu bildirir.
    /// </summary>
    /// <param name="playerId">Cevap veren oyuncunun kimliği.</param>
    /// <param name="isCorrect">Cevabın doğruluğu.</param>
    [PunRPC]
    private void NotifyOthers(int playerId, bool isCorrect)
    {
        // Diğer oyunculara cevabın sonucunu bildirin (UI güncellemeleri vb. için kullanılabilir)
        Debug.Log($"Oyuncu {playerId} cevabını {(isCorrect ? "doğru" : "yanlış")} verdi.");
    }

    /// <summary>
    /// Oyuncu çıkış yaptığında çağrılır.
    /// </summary>
    /// <param name="playerId">Çıkan oyuncunun kimliği.</param>
    //public void OnPlayerLeft(int playerId)
    //{
    //    // Çıkan oyuncunun puan bilgilerini kaldır
    //    playerManager.RemovePlayer(playerId);
    //}
}
