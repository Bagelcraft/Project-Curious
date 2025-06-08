using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Controls the main menu buttons, enabling scene transitions and disabling other buttons when one is activated.
/// Attach this script to your Main Menu Canvas GameObject.
/// </summary>
public class MainMenuScript : MonoBehaviour
{
    [Header("Menu Buttons")]
    [Tooltip("List of all menu buttons to manage")]
    public Button[] menuButtons;
    public Button BattleButton;
    public Button[] GameSelection;
    public GameObject Gamecategory;

    /// <summary>
    /// Loads the specified scene and disables all other buttons.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    /// 

    private void Start()
    {
        Gamecategory.SetActive(false);
    }

    public void BattleButtonClick()
    {
        Gamecategory.SetActive(true);
        BattleButton.gameObject.SetActive(true);

    }    

    public void OnMenuButtonClicked(string sceneName)
    {
        // Disable all menu buttons to prevent multiple clicks
        SetButtonsInteractable(false);

        // Optionally play click sound or animation here

        // Load the scene
        SceneManager.LoadScene(sceneName);
    }

    /// <summary>
    /// Enables or disables interactivity on all menu buttons.
    /// </summary>
    /// <param name="state">True to enable; false to disable.</param>
    private void SetButtonsInteractable(bool state)
    {
        foreach (Button btn in menuButtons)
        {
            btn.interactable = state;
        }
    }

    /// <summary>
    /// Example methods for specific buttons; these can be linked in the Inspector.
    /// </summary>
    public void PlayGame()
    {
        OnMenuButtonClicked("GameScene");
    }

    public void OpenLeaderboard()
    {
        OnMenuButtonClicked("LeaderboardScene");
    }

    public void OpenSettings()
    {
        OnMenuButtonClicked("SettingsScene");
    }

    public void QuitGame()
    {
        // Disable buttons during quit
        SetButtonsInteractable(false);

        Debug.Log("Quitting game...");

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
