using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;
using Firebase.Storage;
using System.Threading.Tasks;
using System;
using UnityEditor.VersionControl;

public class DBManager : Singleton<DBManager>
{
    public UserData user;
    public AuthManager auth;
    public FirebaseUser newUser;
    public DatabaseReference usersReference;
    public DatabaseReference soruRef;
    public StorageReference imagesReference;
    public string DatabaseUrl = "https://king-of-quiz.firebaseio.com/";
    public string storageUrl = "gs://king-of-quiz.appspot.com/";
    public string userName;
    public bool isDone;

    private GamePlayManager gamePlayManager;
    void Start()
    {
        user = UserData.Instance;
        auth = AuthManager.Instance;
        StartCoroutine(Initilization());
    }
    private IEnumerator Initilization()
    {
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl(DatabaseUrl);
        var task = FirebaseApp.CheckAndFixDependenciesAsync();

        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsFaulted || task.IsCanceled)
        {
            Debug.LogError(task.Exception);
            yield break;
        }

        var dependencyStatus = task.Result;
        if (dependencyStatus == DependencyStatus.Available)
        {
            usersReference = FirebaseDatabase.DefaultInstance.GetReference("Users");
            soruRef = FirebaseDatabase.DefaultInstance.GetReference("Sorular");
            isDone = true;
        }
        else
        {
            Debug.LogError("Database Error");

        }
    }

    public void CreateUser(string username, string password)
    {

        newUser = auth.auth.CurrentUser;
        Dictionary<string, object> user = new Dictionary<string, object>();
        user["username"] = username;
        user["email"] = newUser.Email;
        user["password"] = password;
        user[" randevu"] = "";
        userName = username;
        usersReference.Child(newUser.UserId).UpdateChildrenAsync(user);
        StartCoroutine(auth.SendEmailVerification());
    }


    public IEnumerator GetQuestions()
    {
        gamePlayManager = GamePlayManager.Instance;

        var task = soruRef.GetValueAsync();
        while (!task.IsCompleted)
        {
            yield return null;
        }
        if (task.IsCanceled || task.IsFaulted)
        {
        }
        Debug.LogError("Database Error");
        yield break;
        DataSnapshot snapshot = task.Result;
        Debug.Log(snapshot.ChildrenCount);
        Debug.Log("random: " + Convert.ToInt32(UnityEngine.Random.Range(0, snapshot.ChildrenCount)));
        int random = Convert.ToInt32(UnityEngine.Random.Range(1, snapshot.ChildrenCount));

        Debug.Log("ge.itk");
        foreach (var quest in snapshot.Child(random.ToString()).Children)
        {
            if (quest.Key == "resim")
            {
                StartCoroutine(gamePlayManager.LoadQuestionImage(quest.Value.ToString()));
            }
            if (quest.Key == "soru")
            {
                gamePlayManager.GetQuestion(quest.Value.ToString());
            }
            
            if (quest.Key == "??klar")
            {
            }
            gamePlayManager.GetOptions(quest.Child("A").Value.ToString(), quest.Child("B").Value.ToString()
            , quest.Child("C").Value.ToString(), quest.Child("D").Value.ToString(), quest.Child("dogru").Value.ToString());
        }
    }
   
}