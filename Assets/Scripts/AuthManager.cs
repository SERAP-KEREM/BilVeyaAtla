using UnityEngine;
using Firebase.Auth;
using Firebase.Firestore;

public class AuthManager : MonoBehaviour
{
    FirebaseAuth auth;
    FirebaseFirestore db;

    private void Start()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void GetUserName(string userId, System.Action<string> callback)
    {
        DocumentReference docRef = db.Collection("users").Document(userId);
        docRef.GetSnapshotAsync().ContinueWith(task =>
        {
            if (task.IsCompleted)
            {
                DocumentSnapshot snapshot = task.Result;
                if (snapshot.Exists)
                {
                    string userName = snapshot.GetValue<string>("username"); // "username" alanı Firestore'da tanımlanmalı
                    callback(userName);
                }
                else
                {
                    Debug.LogError("No such user!");
                }
            }
        });
    }
}