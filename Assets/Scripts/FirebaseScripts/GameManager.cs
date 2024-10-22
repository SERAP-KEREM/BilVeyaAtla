using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private IFirestoreService firestoreService; // Firestore servisi
    private List<Question> allQuestions; // T�m sorular
    private Question currentQuestion; // ?u anki soru
    private float timeRemaining = 10f; // 10 saniyelik s�re
    private bool isQuestionActive = false; // Sorunun aktif olup olmad???n? kontrol et

    public TextMeshProUGUI questionText; // UI'daki soru metni
    public Button[] optionButtons; // Se�enek butonlar?
    public TextMeshProUGUI timerText; // S�re metni
    public GameObject resultPanel; // Sonu� paneli
    public TextMeshProUGUI resultText; // Sonu� metni
    public Button closeButton; // Kapatma butonu

    private void Start()
    {
        firestoreService = new FirestoreService(); // Firestore servisini ba?lat

        // UI bile?enlerini kontrol et
        if (questionText == null || optionButtons == null || timerText == null || resultPanel == null || resultText == null)
        {
            Debug.LogError("Bir veya daha fazla UI bile?eni atanmad?!");
            return; // UI bile?enleri atanmad?ysa metodu sonland?r
        }

        if (PhotonNetwork.IsMasterClient)
        {
            FetchAndShowRandomQuestion(); // Host ise soru �ek
        }
        else
        {
            resultPanel.SetActive(false); // Di?er oyuncular i�in sonu� panelini kapat
        }

        closeButton.onClick.AddListener(CloseResultPanel); // Kapatma butonuna dinleyici ekle
    }

    private async void FetchAndShowRandomQuestion()
    {
        try
        {
            allQuestions = await firestoreService.GetQuestions(); // Sorular? Firestore'dan �ek

            // Sorular?n bo? olup olmad???n? kontrol et
            if (allQuestions == null || allQuestions.Count == 0)
            {
                Debug.LogError("Firestore'dan hi� soru al?namad?.");
                return; // E?er bo?sa metodu sonland?r
            }

            currentQuestion = GetRandomQuestion(); // Rastgele bir soru se�
            photonView.RPC("SendQuestionToAllPlayers", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer); // Soruyu t�m oyunculara g�nder
            StartCoroutine(StartQuestionTimer()); // Zamanlay?c?y? ba?lat
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Soru �ekme i?lemi s?ras?nda hata olu?tu: {ex.Message}");
        }
    }

    private Question GetRandomQuestion()
    {
        int randomIndex = Random.Range(0, allQuestions.Count);
        Question selectedQuestion = allQuestions[randomIndex];
        allQuestions.RemoveAt(randomIndex); // Ayn? sorunun tekrar gelmesini engelle
        return selectedQuestion;
    }

    [PunRPC]
    private void SendQuestionToAllPlayers(string questionText, string[] options, string correctAnswer)
    {
        this.questionText.text = questionText; // Soru metnini g�ncelle

        // Se�enekleri g�ncelle
        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i]; // Se�enek metinlerini g�ncelle
            optionButtons[i].gameObject.SetActive(true); // Butonlar? g�r�n�r hale getir

            int index = i; // Local de?i?ken olu?tur
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index, correctAnswer)); // Cevap se�imi
        }

        isQuestionActive = true; // Sorunun aktif oldu?unu belirt
    }

    private IEnumerator StartQuestionTimer()
    {
        timeRemaining = 10f; // S�reyi 10 saniye olarak ayarla

        while (timeRemaining > 0 && isQuestionActive)
        {
            timerText.text = $"Kalan S�re: {timeRemaining.ToString("F0")}"; // S�reyi g�ster
            timeRemaining -= Time.deltaTime; // Zaman? azalt
            yield return null; // Bir frame bekle
        }

        // Zaman doldu?unda sonu�lar? yaln?zca cevap veren oyuncuya g�ster
        if (isQuestionActive)
        {
            ShowResults(false); // Yan?t verilmediyse sonu�lar? g�ster
            DisplayTimeUpMessage(); // S�re doldu mesaj?n? g�ster
        }
    }

    public void OnOptionSelected(int index, string correctAnswer)
    {
        if (!isQuestionActive || currentQuestion == null) return; // E?er soru aktif de?ilse veya currentQuestion null ise geri d�n

        string selectedAnswer = currentQuestion.Options[index]; // Se�ilen cevab? al
        bool isCorrect = selectedAnswer == correctAnswer; // Cevab?n do?ru olup olmad???n? kontrol et

        // Cevab? yaln?zca bu oyuncuya g�nder
        photonView.RPC("SubmitAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isCorrect); // Cevab? di?er oyunculara g�nder
        isQuestionActive = false; // Soruyu pasif hale getir
    }

    [PunRPC]
    private void SubmitAnswer(int playerId, bool isCorrect)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId) // E?er bu cevap veren oyuncu sensen
        {
            ShowResults(isCorrect); // Sonu�lar? yaln?zca cevap veren oyuncuya g�ster
        }
    }

    private void ShowResults(bool isCorrect)
    {
        resultPanel.SetActive(true); // Sonu� panelini a�
        resultText.text = isCorrect ? "Do?ru Cevap!" : "Yanl?? Cevap!"; // Sonu� mesaj?n? g�ncelle

        // Butonlar? kapat
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false); // Se�enek butonlar?n? kapat
        }

        timerText.gameObject.SetActive(false); // Zamanlay?c?y? gizle
    }

    private void DisplayTimeUpMessage()
    {
        resultText.text = "S�re Bitti!"; // S�re bitti mesaj?n? g�ster
        resultPanel.SetActive(true); // Sonu� panelini a�
        timerText.gameObject.SetActive(false); // Zamanlay?c?y? gizle
    }

    private void CloseResultPanel()
    {
        resultPanel.SetActive(false); // Sonu� panelini kapat
        timerText.gameObject.SetActive(true); // Zamanlay?c?y? g�r�n�r yap
    }
}
