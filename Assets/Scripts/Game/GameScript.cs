using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GameScript : MonoBehaviour
{
    [Header("Match Manager")]
    public MatchManager matchManager;

    [Header("UI References")]
    public TMP_Text questionText;
    public Image mapPlaceholder;
    public Button[] optionButtons;
    public TMP_Text[] optionLabels;
    public TMP_Text timerText;      // <— Add this reference in the Inspector

    private MapPinpointQuestion currentQuestion;

    private void Awake()
    {
        // Subscribe to events from MatchManager
        matchManager.OnNewQuestion += DisplayQuestion;
        matchManager.OnTimerTick += UpdateTimerUI;
        matchManager.OnMatchEnd += ShowResults;
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        matchManager.OnNewQuestion -= DisplayQuestion;
        matchManager.OnTimerTick -= UpdateTimerUI;
        matchManager.OnMatchEnd -= ShowResults;
    }

    private void DisplayQuestion(MapPinpointQuestion q)
    {
        currentQuestion = q;
        questionText.text =
          $"What station is between {q.previousStation.name} and {q.nextStation.name}?";
        mapPlaceholder.color = Color.white;

        // Wire up the four Kahoot‑style buttons
        for (int i = 0; i < optionButtons.Length; i++)
        {
            if (i < q.options.Count)
            {
                var station = q.options[i];
                optionLabels[i].text = station.name;

                // Clear old listeners, then call our OnOptionClicked
                optionButtons[i].onClick.RemoveAllListeners();
                int idx = i;
                optionButtons[i].onClick.AddListener(() => OnOptionClicked(idx));
                optionButtons[i].gameObject.SetActive(true);
            }
            else
            {
                optionButtons[i].gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Called when the player taps one of the four options.
    /// Delegates correctness handling back to MatchManager.
    /// </summary>
    private void OnOptionClicked(int idx)
    {
        var chosen = currentQuestion.options[idx];
        if (chosen.code == currentQuestion.correctStation.code)
        {
            // Correct → increment local score + next question
            matchManager.RegisterCorrectAnswer();
        }
        else
        {
            // Wrong → just ask next question
            matchManager.AskNextQuestion();
        }
    }

    private void UpdateTimerUI(int secondsLeft)
    {
        timerText.text = $"{secondsLeft}s";
    }

    private void ShowResults(int myScore, int oppScore)
    {
        // TODO: Navigate to your result screen,
        // display myScore vs oppScore
        Debug.Log($"Match ended. You: {myScore}, Opponent: {oppScore}");
    }
}
