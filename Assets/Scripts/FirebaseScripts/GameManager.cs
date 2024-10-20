using Photon.Pun;
using Photon.Realtime; // PhotonPlayer yerine kullan?yoruz
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    private IFirestoreService firestoreService; // Firestore servisini referans al?yoruz
    private FirestoreService fService; // Do?rudan FirestoreService tipi kullan?l?yor

    private List<Question> allQuestions; // Tüm sorular
    private Question currentQuestion; // ?u anki soru
    private bool isHost; // Oyuncunun host olup olmad???n? kontrol eder

    public IQuestionUI questionUI; // UI ile etkile?im için arayüz

    // Oyuncu puanlar?
    private Dictionary<int, int> playerScores = new Dictionary<int, int>();

    void Start()
    {
        fService = new FirestoreService(); // Firestore servisini ba?lat?yoruz
        questionUI = FindObjectOfType<QuestionUIController>(); // UI kontrolcüsünü buluyoruz
        isHost = PhotonNetwork.IsMasterClient;  // Host olup olmad???n? kontrol et

        // Tüm oyuncular?n ba?lang?çta puan?n? s?f?rla
        foreach (var player in PhotonNetwork.PlayerList)
        {
            playerScores[player.ActorNumber] = 0;
        }

        if (isHost)
        {
            // Host ise sorular? çek ve di?er oyunculara gönder
            FetchRandomQuestion();
        }
    }

    // Firestore'dan sorular? çekme i?lemi
    private async void FetchRandomQuestion()
    {
        allQuestions = await firestoreService.GetQuestions(); // Sorular? Firestore'dan çek

        if (allQuestions.Count > 0)
        {
            currentQuestion = GetRandomQuestion(); // Rastgele bir soru seç
            // Seçilen soruyu tüm oyunculara gönder
            photonView.RPC("SendQuestionToAllPlayers", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer);
        }
    }

    // Tüm sorular aras?ndan rastgele bir soru seçer
    private Question GetRandomQuestion()
    {
        int randomIndex = Random.Range(0, allQuestions.Count);
        return allQuestions[randomIndex];
    }

    // RPC ile tüm oyunculara soru gönderilir
    [PunRPC]
    public void SendQuestionToAllPlayers(string questionText, string[] options, string correctAnswer)
    {
        // Gelen verilerle yeni bir soru olu?tur
        currentQuestion = new Question(questionText, new List<string>(options), correctAnswer);
        questionUI.SetQuestionUI(currentQuestion); // UI üzerinden soruyu göster
    }

    // Oyuncular?n cevaplar?n? almak için kullan?l?r
    public void OnPlayerAnswered(string selectedAnswer, Player player) // Photon.Realtime.Player kullan?yoruz
    {
        photonView.RPC("SubmitAnswer", RpcTarget.All, selectedAnswer, player.ActorNumber);
    }

    // Cevaplar? kontrol edip sonucu göstermek için kullan?l?r
    [PunRPC]
    public void SubmitAnswer(string selectedAnswer, int playerActorNumber, PhotonMessageInfo info)
    {
        bool isCorrect = selectedAnswer == currentQuestion.CorrectAnswer;

        if (isCorrect)
        {
            playerScores[playerActorNumber]++; // Do?ru cevapsa oyuncunun puan?n? art?r
        }

        // Sonucu tüm oyunculara göster
        photonView.RPC("ShowResults", RpcTarget.All, isCorrect, playerActorNumber);
    }

    // Sonucu tüm oyunculara gösterir
    [PunRPC]
    public void ShowResults(bool isCorrect, int playerActorNumber)
    {
        Player player = PhotonNetwork.CurrentRoom.GetPlayer(playerActorNumber); // Player bilgilerini çekiyoruz
        string playerName = player.NickName;

        if (isCorrect)
        {
            Debug.Log($"{playerName} do?ru cevap verdi!");
            questionUI.ShowCorrectAnswerFeedback(); // UI üzerinden do?ru cevap animasyonu
        }
        else
        {
            Debug.Log($"{playerName} yanl?? cevap verdi.");
            questionUI.ShowIncorrectAnswerFeedback(); // UI üzerinden yanl?? cevap animasyonu
        }

        // Oyuncu puanlar?n? güncelle
        questionUI.UpdatePlayerScores(playerScores);
    }
}
