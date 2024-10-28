using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviourPunCallbacks
{
    private IFirestoreService firestoreService;  // Firestore hizmeti için arayüz
    private List<Question> allQuestions;          // Tüm soruların listesi
    private Question currentQuestion;             // Şu anki soru
    private float timeRemaining = 10f;            // Kalan süre
    private bool isQuestionActive = false;        // Soru aktif mi?

    public TextMeshProUGUI questionText;         // Soru metni için TextMeshPro
    public Button[] optionButtons;                // Seçenek butonları
    public TextMeshProUGUI timerText;             // Zamanlayıcı metni için TextMeshPro
    public GameObject resultPanel;                // Sonuç paneli
    public TextMeshProUGUI resultText;            // Sonuç metni için TextMeshPro
    public Button closeButton;                     // Kapat butonu

    private Coroutine questionTimerCoroutine;     // Soru zamanlayıcısı
    private Dictionary<int, int> playerScores = new Dictionary<int, int>(); // Oyuncu skorları

    void Start()
    {
        firestoreService = new FirestoreService(); // Firestore hizmetini başlat
        if (PhotonNetwork.IsMasterClient) // Sadece host bu işlemi yapar
        {
            LoadNewQuestion(); // Yeni soruyu yükle
        }
    }

    // Yeni bir soru yüklemek için çağrılır
    private async void LoadNewQuestion()
    {
        allQuestions = await firestoreService.GetQuestions(); // Soruları Firestore'dan çek
        if (allQuestions.Count > 0)
        {
            currentQuestion = GetRandomQuestion(); // Rastgele bir soru al
            // Sadece host RPC'yi çağırır
            photonView.RPC("DisplayQuestionRPC", RpcTarget.All, currentQuestion.QuestionText, currentQuestion.Options.ToArray(), currentQuestion.CorrectAnswer);
        }
        else
        {
            Debug.LogError("No questions available.");
        }
    }

    // Rastgele bir soru seçer
    private Question GetRandomQuestion()
    {
        int randomIndex = Random.Range(0, allQuestions.Count);
        return allQuestions[randomIndex];
    }

    // Soruyu ekrana getirir
    [PunRPC]
    private void DisplayQuestionRPC(string questionText, string[] options, string correctAnswer)
    {
        // Question nesnesini oluştururken gerekli tüm parametreleri sağlayın
        this.currentQuestion = new Question(questionText, new List<string>(options), correctAnswer);
        DisplayQuestion(this.currentQuestion); // Soru göster
        StartQuestionTimer(); // Zamanlayıcıyı başlat
    }


    private void DisplayQuestion(Question question)
    {
        questionText.text = question.QuestionText; // Soru metnini ayarla

        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < question.Options.Count)
            {
                optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = question.Options[i];
                optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false); // Fazla butonları gizle
            }
        }

        isQuestionActive = true; // Soru aktif
    }

    // Soru zamanlayıcısını başlatır
    private void StartQuestionTimer()
    {
        timeRemaining = 10f; // Süreyi sıfırla
        timerText.text = timeRemaining.ToString("F0");
        if (questionTimerCoroutine != null)
        {
            StopCoroutine(questionTimerCoroutine); // Önceki zamanlayıcıyı durdur
        }
        questionTimerCoroutine = StartCoroutine(QuestionTimer()); // Yeni zamanlayıcı başlat
    }

    // Sorunun zamanlayıcısı
    private IEnumerator QuestionTimer()
    {
        while (timeRemaining > 0)
        {
            yield return new WaitForSeconds(1);
            timeRemaining--;
            timerText.text = timeRemaining.ToString("F0"); // Kalan süreyi güncelle
        }

        // Süre dolduğunda sonuç göster
        ShowResult(false);
    }

    // Seçeneğe tıklama işlemi
    public void OnOptionSelected(int optionIndex)
    {
        // Buton indeksinin geçerli olup olmadığını kontrol edin
        if (optionIndex < 0 || optionIndex >= optionButtons.Length)
        {
            Debug.LogError($"Geçersiz buton indeksi: {optionIndex}. Toplam buton sayısı: {optionButtons.Length}");
            return; // Hata durumunda metodu sonlandır
        }

        Debug.Log($"Butona tıklandı: Seçenek İndeksi {optionIndex}");
        if (isQuestionActive)
        {
            string selectedAnswer = optionButtons[optionIndex].GetComponentInChildren<TextMeshProUGUI>().text; // Seçilen cevabı al
            bool isCorrect = selectedAnswer == currentQuestion.CorrectAnswer; // Cevabın doğruluğunu kontrol et

            ShowResult(isCorrect); // Sonucu göster
        }
    }

    // Sonuç panelini göster
    private void ShowResult(bool isCorrect)
    {
        isQuestionActive = false; // Soru artık aktif değil
        if (isCorrect)
        {
            resultText.text = "Doğru cevap!";
            UpdatePlayerScore(); // Skoru güncelle
        }
        else
        {
            resultText.text = "Yanlış cevap! Doğru cevap: " + currentQuestion.CorrectAnswer;
        }

        resultPanel.SetActive(true); // Sonuç panelini aç
        StopCoroutine(questionTimerCoroutine); // Zamanlayıcıyı durdur
    }

    // Oyuncunun skorunu günceller
    private void UpdatePlayerScore()
    {
        // Burada, oyuncunun skorunu güncelleme mantığını yazabilirsiniz
        // playerScores içindeki ilgili oyuncunun puanını artırın
    }

    // Kapat butonuna tıklandığında
    public void CloseResultPanel()
    {
        resultPanel.SetActive(false); // Sonuç panelini kapat
        LoadNewQuestion(); // Yeni soru yükle
    }
}
