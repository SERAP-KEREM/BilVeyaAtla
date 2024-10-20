
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionUIController : MonoBehaviourPunCallbacks, IQuestionUI
{
    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    public TextMeshProUGUI[] playerScoreTexts; // Oyuncu skorlar?n? gösterecek Text bile?enleri

    private string correctAnswer;

    // UI üzerinde soruyu ve ??klar? gösterir
    public void SetQuestionUI(Question question)
    {
        questionText.text = question.QuestionText;
        correctAnswer = question.CorrectAnswer;

        for (int i = 0; i < question.Options.Count; i++)
        {
            optionButtons[i].GetComponentInChildren<Text>().text = question.Options[i];
            int index = i; // Lambda closure sorunu için
            optionButtons[i].onClick.RemoveAllListeners(); // Önceki event'leri temizle
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(question.Options[index]));
        }
    }

    // Bir oyuncu bir seçenek seçti?inde ça?r?l?r
    private void OnOptionSelected(string selectedAnswer)
    {
        photonView.RPC("SubmitAnswer", RpcTarget.All, selectedAnswer);
    }

    // Do?ru cevab? gösteren bir animasyon veya geri bildirim
    public void ShowCorrectAnswerFeedback()
    {
        Debug.Log("Do?ru cevap verildi!");
        // Burada do?ru cevap UI'de gösterilebilir (örne?in, ye?il animasyon)
    }

    // Yanl?? cevab? gösteren bir animasyon veya geri bildirim
    public void ShowIncorrectAnswerFeedback()
    {
        Debug.Log("Yanl?? cevap verildi!");
        // Yanl?? cevap için k?rm?z? bir animasyon veya ba?ka bir geri bildirim
    }

    // Oyuncular?n skorlar?n? günceller
    public void UpdatePlayerScores(Dictionary<int, int> playerScores)
    {
        foreach (var playerScore in playerScores)
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(playerScore.Key).NickName;
            playerScoreTexts[playerScore.Key].text = $"{playerName}: {playerScore.Value}";
        }
    }
}