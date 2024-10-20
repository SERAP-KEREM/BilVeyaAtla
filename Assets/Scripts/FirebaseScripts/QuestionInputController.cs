using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class QuestionInputController : MonoBehaviour
{
    public TMP_InputField questionInputField; // Soru metni için
    public TMP_InputField[] optionInputFields; // Cevaplar için
    public TMP_InputField correctAnswerInputField; // Do?ru cevap

    private FirestoreService firestoreService;

    private void Start()
    {
        firestoreService = new FirestoreService();
    }

    // Butona bas?ld???nda ça?r?lacak fonksiyon
    public async void OnSubmitQuestion()
    {
        string questionText = questionInputField.text;
        List<string> options = new List<string>();
        foreach (var inputField in optionInputFields)
        {
            options.Add(inputField.text);
        }
        string correctAnswer = correctAnswerInputField.text;

        // Yeni soru nesnesini olu?tur
        Question newQuestion = new Question(questionText, options, correctAnswer);

        // Firebase'e soruyu kaydet
        await firestoreService.AddQuestion(newQuestion);

        Debug.Log("Soru eklendi.");
    }
}
