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
    private List<Question> allQuestions; // Tüm sorular
    private Question currentQuestion; // ?u anki soru
    private float timeRemaining = 10f; // 10 saniyelik süre
    private bool isQuestionActive = false; // Sorunun aktif olup olmad???n? kontrol et

    public TextMeshProUGUI questionText; // UI'daki soru metni
    public Button[] optionButtons; // Seçenek butonlar?
    public TextMeshProUGUI timerText; // Süre metni
    public GameObject resultPanel; // Sonuç paneli
    public TextMeshProUGUI resultText; // Sonuç metni
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
            FetchAndShowRandomQuestion(); // Host ise soru çek
        }
        else
        {
            resultPanel.SetActive(false); // Di?er oyuncular için sonuç panelini kapat
        }

        closeButton.onClick.AddListener(CloseResultPanel); // Kapatma butonuna dinleyici ekle
    }

    private async void FetchAndShowRandomQuestion()
    {
        try
        {
            allQuestions = await firestoreService.GetQuestions(); // Sorular? Firestore'dan çek

            // Sorular?n bo? olup olmad???n? kontrol et
            if (allQuestions == null || allQuestions.Count == 0)
            {
                Debug.LogError("Firestore'dan hiç soru al?namad?.");
                return; // E?er bo?sa metodu sonland?r
            }

            currentQuestion = GetRandomQuestion(); // Rastgele bir soru seç
            photonView.RPC("SendQuestionToAllPlayers", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer); // Soruyu tüm oyunculara gönder
            StartCoroutine(StartQuestionTimer()); // Zamanlay?c?y? ba?lat
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Soru çekme i?lemi s?ras?nda hata olu?tu: {ex.Message}");
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
        this.questionText.text = questionText; // Soru metnini güncelle

        // Seçenekleri güncelle
        for (int i = 0; i < options.Length; i++)
        {
            optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = options[i]; // Seçenek metinlerini güncelle
            optionButtons[i].gameObject.SetActive(true); // Butonlar? görünür hale getir

            int index = i; // Local de?i?ken olu?tur
            optionButtons[i].onClick.AddListener(() => OnOptionSelected(index, correctAnswer)); // Cevap seçimi
        }

        isQuestionActive = true; // Sorunun aktif oldu?unu belirt
    }

    private IEnumerator StartQuestionTimer()
    {
        timeRemaining = 10f; // Süreyi 10 saniye olarak ayarla

        while (timeRemaining > 0 && isQuestionActive)
        {
            timerText.text = $"Kalan Süre: {timeRemaining.ToString("F0")}"; // Süreyi göster
            timeRemaining -= Time.deltaTime; // Zaman? azalt
            yield return null; // Bir frame bekle
        }

        // Zaman doldu?unda sonuçlar? yaln?zca cevap veren oyuncuya göster
        if (isQuestionActive)
        {
            ShowResults(false); // Yan?t verilmediyse sonuçlar? göster
            DisplayTimeUpMessage(); // Süre doldu mesaj?n? göster
        }
    }

    public void OnOptionSelected(int index, string correctAnswer)
    {
        if (!isQuestionActive || currentQuestion == null) return; // E?er soru aktif de?ilse veya currentQuestion null ise geri dön

        string selectedAnswer = currentQuestion.Options[index]; // Seçilen cevab? al
        bool isCorrect = selectedAnswer == correctAnswer; // Cevab?n do?ru olup olmad???n? kontrol et

        // Cevab? yaln?zca bu oyuncuya gönder
        photonView.RPC("SubmitAnswer", RpcTarget.All, PhotonNetwork.LocalPlayer.ActorNumber, isCorrect); // Cevab? di?er oyunculara gönder
        isQuestionActive = false; // Soruyu pasif hale getir
    }

    [PunRPC]
    private void SubmitAnswer(int playerId, bool isCorrect)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == playerId) // E?er bu cevap veren oyuncu sensen
        {
            ShowResults(isCorrect); // Sonuçlar? yaln?zca cevap veren oyuncuya göster
        }
    }

    private void ShowResults(bool isCorrect)
    {
        resultPanel.SetActive(true); // Sonuç panelini aç
        resultText.text = isCorrect ? "Do?ru Cevap!" : "Yanl?? Cevap!"; // Sonuç mesaj?n? güncelle

        // Butonlar? kapat
        foreach (var button in optionButtons)
        {
            button.gameObject.SetActive(false); // Seçenek butonlar?n? kapat
        }

        timerText.gameObject.SetActive(false); // Zamanlay?c?y? gizle
    }

    private void DisplayTimeUpMessage()
    {
        resultText.text = "Süre Bitti!"; // Süre bitti mesaj?n? göster
        resultPanel.SetActive(true); // Sonuç panelini aç
        timerText.gameObject.SetActive(false); // Zamanlay?c?y? gizle
    }

    private void CloseResultPanel()
    {
        resultPanel.SetActive(false); // Sonuç panelini kapat
        timerText.gameObject.SetActive(true); // Zamanlay?c?y? görünür yap
    }
}
