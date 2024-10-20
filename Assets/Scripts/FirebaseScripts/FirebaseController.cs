using System.Collections;
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
    public string userName;
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

        if (signupPassword.text != signupCPassword.text)
        {
            ShowNotificationMessage("Error", "Passwords do not match!");
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

            // Giriş yap
            SignInUser(email, password);
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

            // Kullanıcı adını sakla
            userName = newUser.DisplayName; // Giriş yaptıktan sonra kullanıcı adını al

            // Kullanıcı verilerini Firestore'a kaydet
            SaveUserToFirestore(newUser.UserId, newUser.DisplayName, newUser.Email);

            // Profil bilgilerini güncelle
            profileUserNameText.text = newUser.DisplayName;
            profileUserEmailText.text = newUser.Email;
            OnUserNameReceived(newUser.DisplayName);

            // Profil panelini aç
            OpenProfilePanel();
        });
    }

    public void OnUserNameReceived(string userName)
    {
        if (userName != null)
        {
            Debug.Log($"Kullanıcı adı: {userName}");
            PhotonNetwork.NickName = userName;
        }
        else
        {
            Debug.Log("Kullanıcı bulunamadı veya bir hata oluştu.");
        }
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
        Debug.LogError("Auth operation failed with error: " + task.Exception);
        ShowNotificationMessage("Error", "Authentication failed: " + task.Exception.Flatten().InnerExceptions[0].Message);
    }

    void ForgetPasswordSubmit(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled || task.IsFaulted)
            {
                HandleAuthTaskError(task);
                return;
            }

            ShowNotificationMessage("Success", "Password reset email sent. Please check your inbox.");
        });
    }

    public void SetRememberMe(bool value)
    {
        if (value)
        {
            PlayerPrefs.SetString("UserEmail", loginEmail.text);
            PlayerPrefs.SetString("UserPassword", loginPassword.text);
        }
        else
        {
            PlayerPrefs.DeleteKey("UserEmail");
            PlayerPrefs.DeleteKey("UserPassword");
        }
        PlayerPrefs.Save();
    }
}
