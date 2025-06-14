// FirebaseInitializer.cs
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using UnityEngine;

public class FirebaseInitializer : MonoBehaviour
{
    public static FirebaseAuth Auth;
    public static FirebaseFirestore DB;

    private async void Awake()
    {
        DontDestroyOnLoad(gameObject);
        var status = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (status != DependencyStatus.Available)
        {
            Debug.LogError($"Could not resolve Firebase dependencies: {status}");
            return;
        }
        Auth = FirebaseAuth.DefaultInstance;
        DB = FirebaseFirestore.DefaultInstance;
    }
}
