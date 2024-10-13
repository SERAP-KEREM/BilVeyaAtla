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

    private const string EMAIL_PREF_KEY = "email";
    private const string PASSWORD_PREF_KEY = "password";
    private const string REMEMBER_ME_PREF_KEY = "rememberMe";


    private AuthManager authManager; // AuthManager referans?


 
    FirebaseFirestore db;




    private void Start()
    {
        Firebase.FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task => {
            var dependencyStatus = task.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                InitializeFirebase();
                LoadSavedCredentials(); // Kullan?c? bilgilerini yükle
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
            }
        });

        authManager = FindObjectOfType<AuthManager>();
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
    }

    public void OpenLoginPanel()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
        ClearLoginFields(); // Giri? alanlar?n? temizle
    }

    private void ClearLoginFields()
    {
        loginEmail.text = "";
        loginPassword.text = ""; // Giri? alanlar?n? temizleme
    }

    public void OpenSignUpPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);
        ClearSignupFields(); // Kay?t alanlar?n? temizle
    }

    public void OpenProfilePanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);
    }

    public void OpenForgetPassPanel()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);
    }

    public async void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) || string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");
            return;
        }

       
        SceneManager.LoadScene(1);
    }

    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) || string.IsNullOrEmpty(signupPassword.text) ||
            string.IsNullOrEmpty(signupCPassword.text) || string.IsNullOrEmpty(signupUserName.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");
            return;
        }

        // Do SignUp
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
        PlayerPrefs.DeleteKey(EMAIL_PREF_KEY);
        PlayerPrefs.DeleteKey(PASSWORD_PREF_KEY);
        PlayerPrefs.DeleteKey(REMEMBER_ME_PREF_KEY); // Logout s?ras?nda bilgileri sil
        OpenLoginPanel();
    }

    void CreateUser(string email, string password, string userName)
    {
        auth.CreateUserWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("CreateUserWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("CreateUserWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
                return;
            }

            Firebase.Auth.AuthResult result = task.Result;
            UnityEngine.Debug.LogFormat("Firebase user created successfully: {0} ({1})", result.User.DisplayName, result.User.UserId);
            UpdateUserProfile(userName);
        });
    }

    public void SignInUser(string email, string password)
    {
        auth.SignInWithEmailAndPasswordAsync(email, password).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("SignInWithEmailAndPasswordAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("SignInWithEmailAndPasswordAsync encountered an error: " + task.Exception);
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {
                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
                return;
            }

            Firebase.Auth.FirebaseUser newUser = task.Result.User;
            UnityEngine.Debug.LogFormat("User signed in successfully: {0} ({1})", newUser.DisplayName, newUser.UserId);

            profileUserNameText.text = newUser.DisplayName; // Kullan?c? ad? burada ayarlan?r
            profileUserEmailText.text = newUser.Email;
            OpenProfilePanel();

            // Remember Me kontrolü
            if (rememberMe.isOn)
            {
                PlayerPrefs.SetString(EMAIL_PREF_KEY, newUser.Email);
                PlayerPrefs.SetString(PASSWORD_PREF_KEY, loginPassword.text);
                PlayerPrefs.SetInt(REMEMBER_ME_PREF_KEY, 1); // 1: true
            }
            else
            {
                PlayerPrefs.DeleteKey(EMAIL_PREF_KEY);
                PlayerPrefs.DeleteKey(PASSWORD_PREF_KEY);
                PlayerPrefs.DeleteKey(REMEMBER_ME_PREF_KEY); // 0: false
            }
            PlayerPrefs.Save(); // De?i?iklikleri kaydet
        });
    }

    private void LoadSavedCredentials()
    {
        if (PlayerPrefs.GetInt(REMEMBER_ME_PREF_KEY, 0) == 1)
        {
            // Kullan?c? bilgilerini yükle
            string savedEmail = PlayerPrefs.GetString(EMAIL_PREF_KEY);
            string savedPassword = PlayerPrefs.GetString(PASSWORD_PREF_KEY);
            loginEmail.text = savedEmail;
            loginPassword.text = savedPassword;

            // Giri? yap
            SignInUser(savedEmail, savedPassword);
            rememberMe.isOn = true; // Toggle durumunu ayarla
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
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                UnityEngine.Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                UnityEngine.Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    void UpdateUserProfile(string userName)
    {
        user = auth.CurrentUser;
        var profile = new Firebase.Auth.UserProfile
        {
            DisplayName = userName,
            PhotoUrl = null // ?sterseniz foto?raf URL'si ekleyebilirsiniz
        };

        user.UpdateUserProfileAsync(profile).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("UpdateUserProfileAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("UpdateUserProfileAsync encountered an error: " + task.Exception);
                return;
            }

            UnityEngine.Debug.Log("User profile updated successfully.");
            profileUserNameText.text = user.DisplayName; // Kullan?c? ad?n? profil sayfas?na güncelle
            profileUserEmailText.text = user.Email;
            OpenProfilePanel(); // Profil sayfas?n? aç
        });
    }

    private string GetErrorMessage(AuthError errorCode)
    {
        string message;
        switch (errorCode)
        {
            case AuthError.InvalidEmail:
                message = "The email address is badly formatted.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "The email address is already in use by another account.";
                break;
            case AuthError.WrongPassword:
                message = "The password is invalid or the user does not have a password.";
                break;
            case AuthError.UserNotFound:
                message = "There is no user corresponding to the email address.";
                break;
            case AuthError.OperationNotAllowed:
                message = "Operation is not allowed. Please enable email/password accounts.";
                break;
            default:
                message = "An unknown error occurred.";
                break;
        }
        return message;
    }

    private void ForgetPasswordSubmit(string email)
    {
        auth.SendPasswordResetEmailAsync(email).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("SendPasswordResetEmailAsync was canceled.");
                return;
            }
            if (task.IsFaulted)
            {
                UnityEngine.Debug.LogError("SendPasswordResetEmailAsync encountered an error: " + task.Exception);
                ShowNotificationMessage("Error", "Could not send password reset email.");
                return;
            }
            ShowNotificationMessage("Alert", "Password reset email sent successfully.");
        });
    }

    private void ClearSignupFields()
    {
        signupEmail.text = "";
        signupPassword.text = "";
        signupCPassword.text = "";
        signupUserName.text = ""; // Kullan?c? ad? alan?n? temizle
    }


    public void OnPlayerJoinedRoom()
    {
        string userId = auth.CurrentUser.UserId; // Oturum açan kullan?c?n?n ID'sini al
        FirebaseController firebaseController = FindObjectOfType<FirebaseController>();

        firebaseController.GetUserName(userId, (userName) =>
        {
            // Oda oyuncu listesine ekle
            AddPlayerToRoomList(userName);
        });
    }

    private void AddPlayerToRoomList(string playerName)
    {
        // Odaya kat?lan oyuncu ismini listeye ekleyin
        Debug.Log($"Player joined: {playerName}");
        // Bu noktada Photon'un oyuncu listesini güncelleyebilirsin
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
                    string userName = snapshot.GetValue<string>("username"); // "username" alan? Firestore'da tan?mlanmal?
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
