using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Firebase.Auth;
using Firebase.Extensions;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

/// <summary>
/// Manages user authentication and transitions to Main Menu on success.
/// </summary>
public class AuthManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_InputField emailInput;
    public TMP_InputField passwordInput;
    public TMP_Text feedbackText;
    public Button registerButton;
    public Button loginButton;
    public Button anonymousButton;

    [Header("Scene Names")]
    [Tooltip("Name of the Main Menu scene to load upon successful login")]
    public string mainMenuScene = "MainMenu";

    private FirebaseAuth auth;

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        registerButton.onClick.AddListener(RegisterUser);
        loginButton.onClick.AddListener(LoginUser);
        anonymousButton.onClick.AddListener(SignInAnonymously);
    }

    private void RegisterUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and password cannot be empty.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    feedbackText.text = "Registration canceled.";
                    return;
                }
                if (task.IsFaulted)
                {
                    feedbackText.text = "Registration failed: " + task.Exception?.GetBaseException().Message;
                    return;
                }

                // Success: load main menu
                feedbackText.text = "Registration successful! Redirecting...";
                LoadMainMenu();
            });
    }

    private void LoginUser()
    {
        string email = emailInput.text.Trim();
        string password = passwordInput.text;
        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            feedbackText.text = "Email and password cannot be empty.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    feedbackText.text = "Login canceled.";
                    return;
                }
                if (task.IsFaulted)
                {
                    feedbackText.text = "Login failed: " + task.Exception?.GetBaseException().Message;
                    return;
                }

                // Success: load main menu
                feedbackText.text = "Login successful! Redirecting...";
                LoadMainMenu();
            });
    }

    private void SignInAnonymously()
    {
        auth.SignInAnonymouslyAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsCanceled)
                {
                    feedbackText.text = "Anonymous sign-in canceled.";
                    return;
                }
                if (task.IsFaulted)
                {
                    feedbackText.text = "Anonymous sign-in failed: " + task.Exception?.GetBaseException().Message;
                    return;
                }

                // Success: load main menu
                feedbackText.text = "Signed in anonymously! Redirecting...";
                LoadMainMenu();
            });
    }

    /// <summary>
    /// Loads the Main Menu scene.
    /// </summary>
    private void LoadMainMenu()
    {
        SceneManager.LoadScene(mainMenuScene);
    }

    private void OnDestroy()
    {
        registerButton.onClick.RemoveAllListeners();
        loginButton.onClick.RemoveAllListeners();
        anonymousButton.onClick.RemoveAllListeners();
    }
}
