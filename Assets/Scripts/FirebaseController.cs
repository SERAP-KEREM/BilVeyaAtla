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
                LoadSavedCredentials(); // Kullan?c? bilgilerini yükle
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

    public void OpenSignupPanel() // Kay?t paneli açma metodu
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

    public void OpenForgetPasswordPanel() // ?ifre unutma paneli açma metodu
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
        SceneManager.LoadScene(1);
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
        PlayerPrefs.DeleteKey("UserEmail"); // Kullan?c? e-posta bilgilerini sil
        PlayerPrefs.DeleteKey("UserPassword"); // Kullan?c? ?ifre bilgilerini sil
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

            profileUserNameText.text = newUser.DisplayName;
            profileUserEmailText.text = newUser.Email;

            // Kullan?c? bilgilerini hat?rlamak için kaydet
            if (rememberMe.isOn)
            {
                PlayerPrefs.SetString("UserName", newUser.DisplayName);
                PlayerPrefs.SetString("UserEmail", newUser.Email);
                PlayerPrefs.SetString("UserPassword", password); // ?ifreyi de kaydet
                PlayerPrefs.Save();
            }
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

    void ForgetPasswordSubmit(string forgetPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task => {
            if (task.IsCanceled)
            {
                Debug.LogError("SendPasswordResetEmailAsync was canceled");
                return;
            }
            if (task.IsFaulted)
            {
                HandleAuthTaskError(task);
                return;
            }

            ShowNotificationMessage("Alert", "Successfully Sent Email For Reset Password");
        });
    }
}
