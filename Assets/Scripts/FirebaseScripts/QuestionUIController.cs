
using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestionUIController : MonoBehaviourPunCallbacks, IQuestionUI
{
    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    public TextMeshProUGUI[] playerScoreTexts; // Oyuncu skorlar?n? g�sterecek Text bile?enleri

    private string correctAnswer;

    // UI �zerinde soruyu ve ??klar? g�sterir
    public void SetQuestionUI(Question question)
    {
        questionText.text = question.QuestionText;
        correctAnswer = question.CorrectAnswer;

        for (int i = 0; i < question.Options.Count; i++)
        {
            optionButtons[i].GetComponentInChildren<Text>().text = question.Options[i];
            int index = i; // Lambda closure sorunu i�in
            optionButtons[i].onClick.RemoveAllListeners(); // �nceki event'leri temizle
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(question.Options[index]));
        }
    }

    // Bir oyuncu bir se�enek se�ti?inde �a?r?l?r
    private void OnOptionSelected(string selectedAnswer)
    {
        photonView.RPC("SubmitAnswer", RpcTarget.All, selectedAnswer);
    }

    // Do?ru cevab? g�steren bir animasyon veya geri bildirim
    public void ShowCorrectAnswerFeedback()
    {
        Debug.Log("Do?ru cevap verildi!");
        // Burada do?ru cevap UI'de g�sterilebilir (�rne?in, ye?il animasyon)
    }

    // Yanl?? cevab? g�steren bir animasyon veya geri bildirim
    public void ShowIncorrectAnswerFeedback()
    {
        Debug.Log("Yanl?? cevap verildi!");
        // Yanl?? cevap i�in k?rm?z? bir animasyon veya ba?ka bir geri bildirim
    }

    // Oyuncular?n skorlar?n? g�nceller
    public void UpdatePlayerScores(Dictionary<int, int> playerScores)
    {
        foreach (var playerScore in playerScores)
        {
            string playerName = PhotonNetwork.CurrentRoom.GetPlayer(playerScore.Key).NickName;
            playerScoreTexts[playerScore.Key].text = $"{playerName}: {playerScore.Value}";
        }
    }
}