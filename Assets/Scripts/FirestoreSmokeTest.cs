using System.Collections.Generic;
using Firebase.Firestore;
using UnityEngine;

public class FirestoreSmokeTest : MonoBehaviour
{
    private async void Start()
    {
        var testRef = FirebaseInitializer.DB
                          .Collection("smoke-tests")
                          .Document("unity-link-test");

        // 1) Write a timestamp
        var payload = new Dictionary<string, object>
        {
            { "lastPing", Timestamp.GetCurrentTimestamp() },
            { "message", "Unity → Firestore OK" }
        };
        await testRef.SetAsync(payload);
        Debug.Log("▶️ Written test document to Firestore");

        // 2) Read it back
        var snapshot = await testRef.GetSnapshotAsync();
        if (snapshot.Exists)
        {
            Debug.Log($"✅ Read back: {snapshot.GetValue<Timestamp>("lastPing")} – "
                    + snapshot.GetValue<string>("message"));
        }
        else
        {
            Debug.LogError("❌ Test document not found in Firestore");
        }
    }
}
