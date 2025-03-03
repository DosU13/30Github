using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ScoreManager : MonoBehaviour
{
    public int currentScore = 0;
    public TMP_Text HighScore;

    private void Start()
    {
        PlayerPrefs.SetInt("Score", 0);
        // Reset the score at the start of a new game
        currentScore = 0;
    }

    public void Update()
    {
        int score = PlayerPrefs.GetInt("Score", 0);
        currentScore = score;
        HighScore.text = "Score: " + currentScore.ToString();
    }

    public void ExitToMainMenu()
    {
        // Save High Score if it's greater than the previous one
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (currentScore > highScore)
        {
            PlayerPrefs.SetInt("HighScore", currentScore);
            PlayerPrefs.Save();
        }

        // Load Main Menu
        SceneManager.LoadScene("MainMenu"); // Ensure the Main Menu scene name is correct
    }

    public void Restart()
    {
        SceneManager.LoadScene("SampleScene");
    }
}
