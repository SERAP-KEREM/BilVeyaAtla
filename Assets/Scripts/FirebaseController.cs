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
using System.Diagnostics;


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
                // Create and hold a reference to your FirebaseApp,
                // where app is a Firebase.FirebaseApp property of your application class.
                InitializeFirebase();

                // Set a flag here to indicate whether Firebase is ready to use by your app.
            }
            else
            {
                UnityEngine.Debug.LogError(System.String.Format(
                  "Could not resolve all Firebase dependencies: {0}", dependencyStatus));
                // Firebase Unity SDK is not safe to use here.
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
    public void OpenSignUpPanel()
    {

        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(false);

    }
    public void OpenProfilePanel()
    {

        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(true);
        forgetPasswordPanel.SetActive(false);

    }
    public void OpenPorgetPassPanel()
    {

        loginPanel.SetActive(false);
        signupPanel.SetActive(false);
        profilePanel.SetActive(false);
        forgetPasswordPanel.SetActive(true);

    }


    public void LoginUser()
    {
        if (string.IsNullOrEmpty(loginEmail.text) && string.IsNullOrEmpty(loginPassword.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");

            return;
        }
        // Do Login  
        SignInUser(loginEmail.text, loginPassword.text);
    }


    public void SignUpUser()
    {
        if (string.IsNullOrEmpty(signupEmail.text) && string.IsNullOrEmpty(signupPassword.text) && string.IsNullOrEmpty(signupCPassword.text) && string.IsNullOrEmpty(signupUserName.text))
        {
            ShowNotificationMessage("Error", "Fields Empty! Please Input All Details");

            return;
        }

        //Do SignUp

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
        notifTitleText.text = "" + title;
        notifMessageText.text = "" + message;

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

            // Firebase user has been created.
            Firebase.Auth.AuthResult result = task.Result;
            UnityEngine.Debug.LogFormat("Firebase user created successfully: {0} ({1})",
                result.User.DisplayName, result.User.UserId);

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

            UnityEngine.Debug.LogFormat("User signed in successfully: {0} ({1})",
                newUser.DisplayName, newUser.UserId);

            profileUserNameText.text = "" + newUser.DisplayName;
            profileUserEmailText.text = "" + newUser.Email;
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
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null
                && auth.CurrentUser.IsValid();
            if (!signedIn && user != null)
            {
                UnityEngine.Debug.Log("Signed out " + user.UserId);
            }
            user = auth.CurrentUser;
            if (signedIn)
            {
                UnityEngine.Debug.Log("Signed in " + user.UserId);

                isSignIn = true;


            }
        }
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

                ShowNotificationMessage("Alert", "Account Successfully Created");
            });
        }
    }


    bool isSigned = false;
    void Update()
    {
        if (isSignIn)
        {
            if (!isSigned)
            {
                isSigned = true;
                profileUserNameText.text = "" + user.DisplayName;
                profileUserEmailText.text = "" + user.Email;
                OpenProfilePanel();
            }

        }
    }


    private static string GetErrorMessage(AuthError errorCode)
    {
        var message = "";
        switch (errorCode)
        {
            case AuthError.AccountExistsWithDifferentCredentials:
                message = "An account already exists with different credentials.";
                break;
            case AuthError.MissingPassword:
                message = "Password is missing.";
                break;
            case AuthError.WeakPassword:
                message = "The password is too weak.";
                break;
            case AuthError.WrongPassword:
                message = "The password is incorrect.";
                break;
            case AuthError.EmailAlreadyInUse:
                message = "An account already exists with this email address.";
                break;
            case AuthError.InvalidEmail:
                message = "Invalid email address.";
                break;
            case AuthError.MissingEmail:
                message = "Email address is missing.";
                break;
            default:
                message = "An error occurred.";
                break;
        }
        return message;
    }

    void ForgetPasswordSubmit(string forgetPasswordEmail)
    {
        auth.SendPasswordResetEmailAsync(forgetPasswordEmail).ContinueWithOnMainThread(task => {

            if (task.IsCanceled)
            {
                UnityEngine.Debug.LogError("SendPasswordResetEmailAsync was canceled");
            }

            if (task.IsFaulted)
            {
                foreach (Exception exception in task.Exception.Flatten().InnerExceptions)
                {


                    Firebase.FirebaseException firebaseEx = exception as Firebase.FirebaseException;
                    if (firebaseEx != null)
                    {
                        var errorCode = (AuthError)firebaseEx.ErrorCode;
                        ShowNotificationMessage("Error", GetErrorMessage(errorCode));
                    }
                }
            }

            ShowNotificationMessage("Alert", "Successfully Send Email For Reset Password");
        }
);
    }

}


