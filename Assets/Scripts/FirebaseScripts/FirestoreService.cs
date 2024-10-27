using Firebase;
using Firebase.Extensions;
using Firebase.Firestore;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public interface IFirestoreService
{
    // Sorular? Firestore'dan asenkron olarak çeker
    Task<List<Question>> GetQuestions();
}

public class FirestoreService : IFirestoreService
{
    private FirebaseFirestore db;

    public FirestoreService()
    {
        db = FirebaseFirestore.DefaultInstance;

        // Firebase'in düzgün ba?lat?ld???ndan emin oluyoruz
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("Firebase initialized successfully!");
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {task.Result}");
            }
        });
    }

    // Firestore'dan sorular? çekme i?lemi
    public async Task<List<Question>> GetQuestions()
    {
        List<Question> questions = new List<Question>();

        try
        {
            QuerySnapshot snapshot = await db.Collection("questions").GetSnapshotAsync();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    Dictionary<string, object> data = document.ToDictionary();

                    // 'questionText' ve 'correctAnswer' alanlar?n? string olarak al?yoruz
                    string questionText = data["questionText"].ToString();
                    string correctAnswer = data["correctAnswer"].ToString();

                    // 'options' alan?n? List<string> olarak dönü?türüyoruz
                    List<string> options = new List<string>();
                    foreach (var option in (List<object>)data["options"])
                    {
                        options.Add(option.ToString());
                    }

                    // Question nesnesini constructor kullanarak olu?turuyoruz
                    Question question = new Question(questionText, options, correctAnswer);

                    questions.Add(question); // Listeye ekliyoruz
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Sorular çekilirken bir hata olu?tu: {e.Message}");
        }

        return questions;
    }



    // Soru ekleme i?lemi
    public async Task AddQuestion(Question question)
    {
        try
        {
            Dictionary<string, object> questionData = new Dictionary<string, object>
            {
                { "questionText", question.QuestionText },
                { "options", question.Options },
                { "correctAnswer", question.CorrectAnswer }
            };

            await db.Collection("questions").AddAsync(questionData);
            Debug.Log("Soru ba?ar?yla Firestore'a kaydedildi.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Soru kaydedilirken bir hata olu?tu: {e.Message}");
        }
    }
}
