using Photon.Pun;
using Photon.Realtime; // PhotonPlayer yerine kullan?yoruz
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private IFirestoreService firestoreService; // Firestore servisini referans al?yoruz
    private FirestoreService fService; // Do?rudan FirestoreService tipi kullan?l?yor

    private List<Question> allQuestions; // T�m sorular
    private Question currentQuestion; // ?u anki soru
    private bool isHost; // Oyuncunun host olup olmad???n? kontrol eder

    public IQuestionUI questionUI; // UI ile etkile?im i�in aray�z

    // Oyuncu puanlar?
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

    void Start()
    {
        fService = new FirestoreService(); // Firestore servisini ba?lat?yoruz
        questionUI = FindObjectOfType<QuestionUIController>(); // UI kontrolc�s�n� buluyoruz
        isHost = PhotonNetwork.IsMasterClient;  // Host olup olmad???n? kontrol et

        // T�m oyuncular?n ba?lang?�ta puan?n? s?f?rla
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerScores[player.ActorNumber] = 0;
        }

        if (isHost)
        {
            // Host ise sorular? �ek ve di?er oyunculara g�nder
            FetchRandomQuestion();
        }
    }

    // Firestore'dan sorular? �ekme i?lemi
    private async void FetchRandomQuestion()
    {
        allQuestions = await firestoreService.GetQuestions(); // Sorular? Firestore'dan �ek

        if (allQuestions.Count > 0)
        {
            currentQuestion = GetRandomQuestion(); // Rastgele bir soru se�
            // Se�ilen soruyu t�m oyunculara g�nder
            photonView.RPC("SendQuestionToAllPlayers", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer);
        }
    }

    // T�m sorular aras?ndan rastgele bir soru se�er
    private Question GetRandomQuestion()
    {
        int randomIndex = Random.Range(0, allQuestions.Count);
        return allQuestions[randomIndex];
    }

    // RPC ile t�m oyunculara soru g�nderilir
    [PunRPC]
    public void SendQuestionToAllPlayers(string questionText, string[] options, string correctAnswer)
    {
        // Gelen verilerle yeni bir soru olu?tur
        currentQuestion = new Question(questionText, new List<string>(options), correctAnswer);
        questionUI.SetQuestionUI(currentQuestion); // UI �zerinden soruyu g�ster
    }

    // Oyuncular?n cevaplar?n? almak i�in kullan?l?r
    public void OnPlayerAnswered(string selectedAnswer, Player player) // Photon.Realtime.Player kullan?yoruz
    {
        photonView.RPC("SubmitAnswer", RpcTarget.All, selectedAnswer, player.ActorNumber);
    }

    // Cevaplar? kontrol edip sonucu g�stermek i�in kullan?l?r
    [PunRPC]
    public void SubmitAnswer(string selectedAnswer, int playerActorNumber, PhotonMessageInfo info)
    {
        bool isCorrect = selectedAnswer == currentQuestion.CorrectAnswer;

        if (isCorrect)
        {
            playerScores[playerActorNumber]++; // Do?ru cevapsa oyuncunun puan?n? art?r
        }

        // Sonucu t�m oyunculara g�ster
        photonView.RPC("ShowResults", RpcTarget.All, isCorrect, playerActorNumber);
    }

    // Sonucu t�m oyunculara g�sterir
    [PunRPC]
    public void ShowResults(bool isCorrect, int playerActorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber); // Player bilgilerini �ekiyoruz
        string playerName = player.NickName;

        if (isCorrect)
        {
            Debug.Log($"{playerName} do?ru cevap verdi!");
            questionUI.ShowCorrectAnswerFeedback(); // UI �zerinden do?ru cevap animasyonu
        }
        else
        {
            Debug.Log($"{playerName} yanl?? cevap verdi.");
            questionUI.ShowIncorrectAnswerFeedback(); // UI �zerinden yanl?? cevap animasyonu
        }

        // Oyuncu puanlar?n? g�ncelle
        questionUI.UpdatePlayerScores(playerScores);
    }
}
