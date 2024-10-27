using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private IFirestoreService firestoreService;
    private List<Question> allQuestions;
    private Question currentQuestion;
    private float timeRemaining = 10f;
    private bool isQuestionActive = false;

    public TextMeshProUGUI questionText;
    public Button[] optionButtons;
    public TextMeshProUGUI timerText;
    public GameObject resultPanel;
    public TextMeshProUGUI resultText;
    public Button closeButton;

    private Coroutine questionTimerCoroutine;
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();
    private int answeredPlayers = 0; // Cevap veren oyuncu sayısını tutan sayaç

    private void Start()
    {
        firestoreService = new FirestoreService();

        if (questionText == null || optionButtons == null || timerText == null || resultPanel == null || resultText == null)
        {
            Debug.LogError("Bir veya daha fazla UI bileşeni atanmadı!");
            return;
        }

        if (PhotonNetwork.IsMasterClient)
        {
            FetchAndShowRandomQuestion();
        }

        closeButton.onClick.AddListener(CloseResultPanel);
    }

    private async void FetchAndShowRandomQuestion()
    {
        try
        {
            allQuestions = await firestoreService.GetQuestions();

            if (allQuestions == null || allQuestions.Count == 0)
            {
                Debug.LogError("Firestore'dan hiç soru alınamadı.");
                return;
            }

            currentQuestion = GetRandomQuestion();
            photonView.RPC("SendQuestionToAllPlayers", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Soru çekme işlemi sırasında hata oluştu: {ex.Message}");
        }
    }

    private Question GetRandomQuestion()
    {
        int randomIndex = Random.Range(0, allQuestions.Count);
        Question selectedQuestion = allQuestions[randomIndex];
        allQuestions.RemoveAt(randomIndex);
        return selectedQuestion;
    }

    [PunRPC]
    private void SendQuestionToAllPlayers(string questionText, string[] options, string correctAnswer)
    {
        this.questionText.text = questionText;

        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i];
            optionButtons[i].gameObject.SetActive(true);

            int index = i;
            optionButtons[i].onClick.RemoveAllListeners();
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index, correctAnswer));
        }

        isQuestionActive = true;
        answeredPlayers = 0; // Yeni soru için sayaç sıfırlanır
        StartQuestionTimer();
    }

    private void StartQuestionTimer()
    {
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine);
        }

        questionTimerCoroutine = StartCoroutine(QuestionTimer());
    }

    private IEnumerator QuestionTimer()
    {
        timeRemaining = 10f;

        while (timeRemaining > 0 && isQuestionActive)
        {
            timerText.text = $"Kalan Süre: {timeRemaining.ToString("F0")}";
            timeRemaining -= Time.deltaTime;
            yield return null;
        }

        if (isQuestionActive)
        {
            isQuestionActive = false;
            DisplayTimeUpMessage();
            photonView.RPC("ShowResultsForAllPlayers", RpcTarget.All, false); // Süre bitiminde yanlış kabul
        }
    }

    public void OnOptionSelected(int index, string correctAnswer)
    {
        if (!isQuestionActive || currentQuestion == null) return;

        string selectedAnswer = currentQuestion.Options[index];
        bool isCorrect = selectedAnswer == correctAnswer;

        photonView.RPC("SubmitAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isCorrect);
        isQuestionActive = false;
    }

    [PunRPC]
    private void SubmitAnswer(int playerId, bool isCorrect)
    {
        if (!playerScores.ContainsKey(playerId))
        {
            playerScores[playerId] = 0;
        }

        if (isCorrect)
        {
            playerScores[playerId] += 1; // Doğru cevaba puan ekle
        }

        // Cevap veren oyuncu sayısını arttır
        answeredPlayers++;

        // Her oyuncu için sonuçları göster
        ShowResultsForPlayer(playerId, isCorrect);

        // Tüm oyuncular cevap verince sonuçları göster
        if (answeredPlayers == PhotonNetwork.CurrentRoom.PlayerCount)
        {
            photonView.RPC("ShowResultsForAllPlayers", RpcTarget.All, isCorrect);
        }
    }

    private void ShowResultsForPlayer(int playerId, bool isCorrect)
    {
        // Sadece sonuçları kendi oyuncusuna göstermek için
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId)
        {
            resultPanel.SetActive(true);
            resultText.text = isCorrect ? "Doğru Cevap!" : "Yanlış Cevap!";
        }
    }

    [PunRPC]
    private void ShowResultsForAllPlayers(bool isCorrect)
    {
        resultPanel.SetActive(true);
        resultText.text = isCorrect ? "Doğru Cevap!" : "Yanlış Cevap!";
    }

    private void DisplayTimeUpMessage()
    {
        resultText.text = "Süre Bitti!";
        resultPanel.SetActive(true);
        timerText.gameObject.SetActive(false);
    }

    private void CloseResultPanel()
    {
        resultPanel.SetActive(false);
        timerText.gameObject.SetActive(true);

        if (PhotonNetwork.IsMasterClient)
        {
            FetchAndShowRandomQuestion();
        }
    }
}
