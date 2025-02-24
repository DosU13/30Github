using UnityEngine;
using TMPro; 

public class GameTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText; 
    private float timeLeft = 10f;

    void Update()
    {
        timeLeft -= Time.deltaTime;
        timerText.text = "Time: " + Mathf.Ceil(timeLeft);

        if (timeLeft <= 0)
        {
            timerText.text = "Game Over!";
            Time.timeScale = 0;
        }
    }
}
