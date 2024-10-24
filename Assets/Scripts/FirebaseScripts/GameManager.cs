using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using System.Collections.Generic;

public class GameManager : MonoBehaviourPunCallbacks
{
    public GameObject resultsPanel;
    public Text resultText;
    public Button answerButton; // Cevap butonu
    private Dictionary<int, bool> playerAnswers = new Dictionary<int, bool>(); // Oyuncular?n cevaplar?n? tutar

    void Start()
    {
        resultsPanel.SetActive(false); // Sonuç panelini gizli tut
        answerButton.onClick.AddListener(() => OnAnswerButtonClick(true)); // Buton t?klamas?
    }

    // Oyuncunun cevap vermesi durumunda bu fonksiyon tetiklenir
    public void OnAnswerButtonClick(bool isCorrect)
    {
        Debug.Log("Cevap butonuna t?kland?. Do?ru mu? " + isCorrect);

        // Tüm oyunculara cevab? gönder
        photonView.RPC("SubmitAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isCorrect);
    }

    // Cevap gönderildi?inde bu RPC fonksiyonu tetiklenir
    [PunRPC]
    public void SubmitAnswer(int playerId, bool isCorrect)
    {
        Debug.Log($"Oyuncu {playerId} cevap verdi: {isCorrect}");

        // Cevab? kaydet
        if (!playerAnswers.ContainsKey(playerId))
        {
            playerAnswers.Add(playerId, isCorrect);
        }

        // Tüm oyuncular?n cevap verip vermedi?ini kontrol et
        if (playerAnswers.Count == PhotonNetwork.PlayerList.Length)
        {
            ShowResultsForAllPlayers();
        }
    }

    // Tüm oyuncular?n cevaplar? al?nd???nda sonuçlar? göster
    private void ShowResultsForAllPlayers()
    {
        resultsPanel.SetActive(true); // Sonuç panelini göster

        // Tüm oyuncular?n sonuçlar?n? tek tek göster
        foreach (var answer in playerAnswers)
        {
            Debug.Log($"Oyuncu {answer.Key}, Do?ru mu: {answer.Value}");
        }

        // Örnek: ?lk oyuncunun sonucunu göster
        bool isCorrect = playerAnswers[PhotonNetwork.LocalPlayer.ActorNumber];
        ShowResultsForPlayer(isCorrect);
    }

    // Oyuncu bazl? sonuçlar? göster
    private void ShowResultsForPlayer(bool isCorrect)
    {
        if (isCorrect)
        {
            resultText.text = "Do?ru cevap verdiniz!";
        }
        else
        {
            resultText.text = "Yanl?? cevap verdiniz!";
        }
    }
}
