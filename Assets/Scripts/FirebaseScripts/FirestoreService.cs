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

public class FirestoreService
{

    private FirebaseFirestore db;

    public FirestoreService()
    {
        db = FirebaseFirestore.DefaultInstance;
    }


    void Start()
    {
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
