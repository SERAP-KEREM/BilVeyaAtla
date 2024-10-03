using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variables
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    public FirebaseAuth auth;
    public FirebaseUser user;

    // Login Variables
    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;

    // Registration Variables
    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;

    private void Awake()
    {
        // Check for Firebase dependencies
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWith(task =>
        {
            dependencyStatus = task.Result;

            if (dependencyStatus == DependencyStatus.Available)
            {
                InitializeFirebase();
            }
            else
            {
                Debug.LogError("Could not resolve all Firebase dependencies: " + dependencyStatus);
            }
        });
    }

    void InitializeFirebase()
    {
        // Set the default instance of FirebaseAuth
        auth = FirebaseAuth.DefaultInstance;

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }

    // Track state changes of the auth object
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out: " + user.UserId);
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in: " + user.UserId);
            }
        }
    }

    public void Login()
    {
        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            Debug.LogError(loginTask.Exception);
            HandleAuthError(loginTask.Exception);
        }
        else
        {
            var result = loginTask.Result;
            user = result.User;
            Debug.LogFormat("{0} You Are Successfully Logged In", user.DisplayName);

            References.userName = user.DisplayName;
            UnityEngine.SceneManagement.SceneManager.LoadScene("GameScene");
        }
    }

    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        // Check for null input fields
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("User Name is empty");
            yield break;
        }
        if (string.IsNullOrEmpty(email))
        {
            Debug.LogError("Email field is empty");
            yield break;
        }
        if (string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
        {
            Debug.LogError("Password fields are empty");
            yield break;
        }
        if (password != confirmPassword)
        {
            Debug.LogError("Passwords do not match");
            yield break;
        }

        // Attempt to create a user
        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            Debug.LogError(registerTask.Exception);
            HandleAuthError(registerTask.Exception);
        }
        else
        {
            var result = registerTask.Result;
            user = result.User;

            // Ensure user is not null before updating profile
            if (user != null)
            {
                UserProfile userProfile = new UserProfile { DisplayName = name };
                var updateProfileTask = user.UpdateUserProfileAsync(userProfile);
                yield return new WaitUntil(() => updateProfileTask.IsCompleted);

                if (updateProfileTask.Exception != null)
                {
                    // Delete the user if the profile update fails
                    user.DeleteAsync();
                    Debug.LogError(updateProfileTask.Exception);
                    HandleAuthError(updateProfileTask.Exception);
                }
                else
                {
                    Debug.Log("Registration Successful! Welcome " + user.DisplayName);
                    UIManager.Instance.OpenLoginPanel();
                }
            }
            else
            {
                Debug.LogError("User is null after registration.");
            }
        }
    }


    private void HandleAuthError(AggregateException exception)
    {
        FirebaseException firebaseException = exception.GetBaseException() as FirebaseException;
        AuthError authError = (AuthError)firebaseException.ErrorCode;

        string errorMessage = "Operation failed! Reason: ";

        switch (authError)
        {
            case AuthError.InvalidEmail:
                errorMessage += "Email is invalid.";
                break;
            case AuthError.WrongPassword:
                errorMessage += "Wrong password.";
                break;
            case AuthError.MissingEmail:
                errorMessage += "Email is missing.";
                break;
            case AuthError.MissingPassword:
                errorMessage += "Password is missing.";
                break;
            default:
                errorMessage += "An unknown error occurred.";
                break;
        }

        Debug.Log(errorMessage);
    }
}
