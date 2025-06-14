using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Firestore;
using Firebase.Extensions;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    [Header("Match Settings")]
    public string matchId;              // assign or generate
    public int durationSeconds = 60;    // total play time

    private bool hasStartedJoining = false;

    private FirebaseAuth auth;
    private FirebaseFirestore db;
    private DocumentReference matchDoc;
    private ListenerRegistration listener;

    private QuestionGenerator generator;
    private int localScore = 0;
    private bool isPlayerA;
    private string myUid, otherUid;
    private bool countdownStarted = false;


    // Events you can hook into:
    public event Action<MapPinpointQuestion> OnNewQuestion;
    public event Action<int, int> OnMatchEnd; // (myScore, otherScore)
    public event Action<int> OnTimerTick;    // remaining seconds

    private void Awake()
    {
        auth = FirebaseAuth.DefaultInstance;
        db = FirebaseFirestore.DefaultInstance;
        generator = GetComponent<QuestionGenerator>()
                    ?? gameObject.AddComponent<QuestionGenerator>();

        myUid = auth.CurrentUser.UserId;
        matchDoc = db.Collection("matches").Document(matchId);
    }

    private void Start()
    {
        //ListenToMatch();
        //TryJoinMatch();
    }

    public void StartMatchmaking()
    {
        if (hasStartedJoining) return;
        hasStartedJoining = true;

        ListenToMatch();
        TryJoinMatch();
    }

    private void ListenToMatch()
    {
        listener = matchDoc.Listen(snapshot =>
        {
            if (!snapshot.Exists) return;

            var data = snapshot.ToDictionary();
            string state = snapshot.GetValue<string>("state");

            if (state == "inProgress")
            {
                // Determine A/B
                string a = snapshot.GetValue<string>("playerA");
                string b = snapshot.GetValue<string>("playerB");
                isPlayerA = (myUid == a);
                otherUid = isPlayerA ? b : a;

                // Only start once:
                if (!countdownStarted)
                {
                    countdownStarted = true;
                    Timestamp ts = snapshot.GetValue<Timestamp>("startTime");
                    var start = ts.ToDateTime();
                    float elapsed = (float)(DateTime.UtcNow - start).TotalSeconds;
                    StartCoroutine(Countdown(durationSeconds - (int)elapsed));
                }
            }
            else if (state == "finished")
            {
                // Both scores should be set
                var scores = data["scores"] as Dictionary<string, object>;
                int aScore = Convert.ToInt32(scores[data["playerA"] as string]);
                int bScore = Convert.ToInt32(scores[data["playerB"] as string]);
                int myScore = isPlayerA ? aScore : bScore;
                int opScore = isPlayerA ? bScore : aScore;
                listener.Stop();  // no more updates
                OnMatchEnd?.Invoke(myScore, opScore);
            }
        });
    }

    private void TryJoinMatch()
    {
        // Transactionally set playerB if empty, else if doc missing create as playerA
        db.RunTransactionAsync(async tx =>
        {
            var snap = await tx.GetSnapshotAsync(matchDoc);
            if (!snap.Exists)
            {
                // Create new match
                tx.Set(matchDoc, new Dictionary<string, object> {
                    { "playerA", myUid },
                    { "playerB", "" },
                    { "state", "waiting" }
                });
            }
            else
            {
                var data = snap.ToDictionary();
                if (data["state"] as string == "waiting" &&
                    (data["playerA"] as string) != myUid)
                {
                    // Join as B and start the match
                    Dictionary<string, object> update = new()
                    {
                        { "playerB", myUid },
                        { "state", "inProgress" },
                        { "startTime", Timestamp.GetCurrentTimestamp() },
                        { "duration", durationSeconds },
                        {
                            "scores",
                            new Dictionary<string, object> {
                            { data["playerA"] as string, 0 },
                            { myUid, 0 }
                        }
                        }
                    };
                    tx.Update(matchDoc, update);
                }
            }
        });
    }

    private IEnumerator Countdown(int remaining)
    {
        localScore = 0;
        // Fire the first question immediately
        AskNextQuestion();

        int r = remaining;
        while (r > 0)
        {
            OnTimerTick?.Invoke(r);
            yield return new WaitForSeconds(1f);
            r--;
        }

        // Time's up → submit score & finish
        SubmitMyScore();
    }

    public void AskNextQuestion()
    {
        var q = generator.GenerateQuestion();
        OnNewQuestion?.Invoke(q);
    }

    /// <summary>
    /// Call this when the player answers correctly.
    /// </summary>
    public void RegisterCorrectAnswer()
    {
        localScore++;
        // Immediately queue up next question
        AskNextQuestion();
    }

    private void SubmitMyScore()
    {
        string field = $"scores.{myUid}";
        // Write my score, then check in a transaction if both scores exist
        db.RunTransactionAsync(async tx =>
        {
            tx.Update(matchDoc, new Dictionary<string, object> {
                { field, localScore }
            });

            var snap = await tx.GetSnapshotAsync(matchDoc);
            var data = snap.ToDictionary();
            var scores = data["scores"] as Dictionary<string, object>;
            // Both scores non-null → finish the match
            if (scores.ContainsKey(data["playerA"] as string) &&
                scores.ContainsKey(data["playerB"] as string))
            {
                tx.Update(matchDoc, new Dictionary<string, object> {
                    { "state", "finished" }
                });
            }
        });
    }
}
