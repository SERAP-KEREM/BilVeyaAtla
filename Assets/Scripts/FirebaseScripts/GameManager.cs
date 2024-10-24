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
        resultsPanel.SetActive(false); // Sonu� panelini gizli tut
        answerButton.onClick.AddListener(() => OnAnswerButtonClick(true)); // Buton t?klamas?
    }

    // Oyuncunun cevap vermesi durumunda bu fonksiyon tetiklenir
    public void OnAnswerButtonClick(bool isCorrect)
    {
        Debug.Log("Cevap butonuna t?kland?. Do?ru mu? " + isCorrect);

        // T�m oyunculara cevab? g�nder
        photonView.RPC("SubmitAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isCorrect);
    }

    // Cevap g�nderildi?inde bu RPC fonksiyonu tetiklenir
    [PunRPC]
    public void SubmitAnswer(int playerId, bool isCorrect)
    {
        Debug.Log($"Oyuncu {playerId} cevap verdi: {isCorrect}");

        // Cevab? kaydet
        if (!playerAnswers.ContainsKey(playerId))
        {
            playerAnswers.Add(playerId, isCorrect);
        }

        // T�m oyuncular?n cevap verip vermedi?ini kontrol et
        if (playerAnswers.Count == PhotonNetwork.PlayerList.Length)
        {
            ShowResultsForAllPlayers();
        }
    }

    // T�m oyuncular?n cevaplar? al?nd???nda sonu�lar? g�ster
    private void ShowResultsForAllPlayers()
    {
        resultsPanel.SetActive(true); // Sonu� panelini g�ster

        // T�m oyuncular?n sonu�lar?n? tek tek g�ster
        foreach (var answer in playerAnswers)
        {
            Debug.Log($"Oyuncu {answer.Key}, Do?ru mu: {answer.Value}");
        }

        // �rnek: ?lk oyuncunun sonucunu g�ster
        bool isCorrect = playerAnswers[PhotonNetwork.LocalPlayer.ActorNumber];
        ShowResultsForPlayer(isCorrect);
    }

    // Oyuncu bazl? sonu�lar? g�ster
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
