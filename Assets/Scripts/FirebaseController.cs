﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Firebase;
using Firebase.Auth;
using System;
using System.Threading.Tasks;
using Firebase.Extensions;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Firebase.Firestore;

public class FirebaseController : MonoBehaviour
{
    public GameObject loginPanel, signupPanel, profilePanel, forgetPasswordPanel, notificationPanel;
    public TMP_InputField loginEmail, loginPassword, signupEmail, signupPassword, signupCPassword, signupUserName, forgetPassEmail;

    public TextMeshProUGUI notifTitleText, notifMessageText, profileUserNameText, profileUserEmailText;

    public Toggle rememberMe;

    Firebase.Auth.FirebaseAuth auth;
    Firebase.Auth.FirebaseUser user;

    bool isSignIn = false;

    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
                LoadSavedCredentials(); // Kullanıcı bilgilerini yükle
            }
            else
            {
                Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
            }
        });
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenSignupPanel() // Kayıt paneli açma metodu
    {
        signupPanel.SetActive(true);
        loginPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenProfilePanel() // Profil paneli açma metodu
    {
        profilePanel.SetActive(true);
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPasswordPanel() // Şifre unutma paneli açma metodu
    {
        forgetPasswordPanel.SetActive(true);
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
    }

    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");
            return;
        }

        SignInUser(loginEmail.text, loginPassword.text);
    }

    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text) || string.IsNullOrEmpty(signupCPassword.text) || string.IsNullOrEmpty(signupUserName.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");
            return;
        }

        CreateUser(signupEmail.text, signupPassword.text, signupUserName.text);
    }

    public void ForgetPass()
    {
        if (string.IsNullOrEmpty(forgetPassEmail.text))
        {
            ShowNotificationMessage("Error", "Fields Empty Please Input All Details");
            return;
        }
        ForgetPasswordSubmit(forgetPassEmail.text);
    }

    private void ShowNotificationMessage(string title, string message)
    {
        notifTitleText.text = title;
        notifMessageText.text = message;

        notificationPanel.SetActive(true);
        StartCoroutine(HideNotificationAfterDelay(5f)); // 5 saniye sonra kapat
    }

    private IEnumerator HideNotificationAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        CloseNotifPanel();
    }

    public void CloseNotifPanel()
    {
        notifTitleText.text = "";
        notifMessageText.text = "";
        notificationPanel.SetActive(false);
    }

    public void LogOut()
    {
        auth.SignOut();
        profileUserNameText.text = "";
        profileUserEmailText.text = "";
        PlayerPrefs.DeleteKey("UserEmail"); // Kullanıcı e-posta bilgilerini sil
        PlayerPrefs.DeleteKey("UserPassword"); // Kullanıcı şifre bilgilerini sil
        OpenLoginPanel();
    }

    void CreateUser(string email, string password, string userName)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleAuthTaskError(task);
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            UpdateUserProfile(userName);

            // Kullanıcı bilgilerini Firestore'a kaydedin
            SaveUserToFirestore(result.User.UserId, userName, email);
        });

    }

    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleAuthTaskError(task);
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);


            // Profil bilgilerini güncelleyin
            profileUserNameText.text = newUser.DisplayName;
            profileUserEmailText.text = newUser.Email;

            // Kullanıcı verilerini Firestore'a kaydet
            SaveUserToFirestore(newUser.UserId, newUser.DisplayName, newUser.Email);

            OpenProfilePanel();
        });
    }


    void InitializeFirebase()
    {
        auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            user = auth.CurrentUser;
            if (user != null)
            {
                Debug.Log("Signed in " + user.UserId);
                isSignIn = true;
            }
            else
            {
                Debug.Log("Signed out");
            }
        }
    }

    public void StarGame()
    {
        SceneManager.LoadScene(1);
    }

    void OnDestroy()
    {
        auth.StateChanged -= AuthStateChanged;
        auth = null;
    }

    void UpdateUserProfile(string userName)
    {
        Firebase.Auth.FirebaseUser user = auth.CurrentUser;
        if (user != null)
        {
            Firebase.Auth.UserProfile profile = new Firebase.Auth.UserProfile
            {
                DisplayName = userName,
                PhotoUrl = new System.Uri("https://via.placeholder.com/150C/0%20https:/placeholder.com/"),
            };
            user.UpdateUserProfileAsync(profile).ContinueWith(task => {
                if (task.IsCanceled || task.IsFaulted)
                {
                    Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                    return;
                }

                Debug.Log("User profile updated successfully.");
                ShowNotificationMessage("Alert", "Account Successfully Created");
            });
        }
    }

    void SaveUserToFirestore(string userId, string userName, string email)
    {
        // Firestore bağlantısı oluşturun ve kullanıcı verilerini kaydedin
        var db = Firebase.Firestore.FirebaseFirestore.DefaultInstance;
        var userDoc = db.Collection("users").Document(userId);

        var userData = new { userName = userName, email = email };

        userDoc.SetAsync(userData).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("Failed to save user to Firestore: " + task.Exception);
            }
            else
            {
                Debug.Log("User saved to Firestore successfully.");
            }
        });
    }

    void LoadSavedCredentials()
    {
        if (PlayerPrefs.HasKey("UserEmail"))
        {
            loginEmail.text = PlayerPrefs.GetString("UserEmail");
        }
        if (PlayerPrefs.HasKey("UserPassword"))
        {
            loginPassword.text = PlayerPrefs.GetString("UserPassword");
        }
    }

    private void HandleAuthTaskError(Task task)
    {
        Debug.LogError("Auth task encountered an error: " + task.Exception);
        foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
        {
            if (exception is Firebase.FirebaseException firebaseEx)
            {
                var errorCode = (AuthError)firebaseEx.ErrorCode;
                ShowNotificationMessage("Error", GetErrorMessage(errorCode));
            }
        }
    }

    private static string GetErrorMessage(AuthError errorCode)
    {
        return errorCode switch
        {
            AuthError.AccountExistsWithDifferentCredentials => "An account already exists with different credentials.",
            AuthError.MissingPassword => "Password is missing.",
            AuthError.WeakPassword => "The password is too weak.",
            AuthError.WrongPassword => "The password is incorrect.",
            AuthError.EmailAlreadyInUse => "An account already exists with this email address.",
            AuthError.InvalidEmail => "Invalid email address.",
            AuthError.MissingEmail => "Email address is missing.",
            _ => "An error occurred."
        };
    }

    void ForgetPasswordSubmit(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleAuthTaskError(task);
                return;
            }

            Debug.Log("Password reset email sent successfully.");
            ShowNotificationMessage("Success", "Password reset email sent. Please check your inbox.");
        });
    }
}
