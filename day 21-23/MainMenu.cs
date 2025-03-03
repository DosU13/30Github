using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public TMP_Text highScoreText; // Assign in Inspector

    private void Start()
    {
        // Load and display the high score
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        highScoreText.text = "High Score: " + highScore;
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("SampleScene"); // Change "GameScene" to your game scene name
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
