using UnityEngine;
using TMPro; 

public class ClickCube : MonoBehaviour
{
    public TextMeshProUGUI scoreText; 
    private int score = 0;

    void Start()
    {
        MoveCube();
    }

    void OnMouseDown()
    {
        score++;
        scoreText.text = "Score: " + score;
        MoveCube();
    }

    void MoveCube()
    {
        float x = Random.Range(-4f, 4f);
        float y = Random.Range(-4f, 4f);
        transform.position = new Vector3(x, y, 0);
    }
}
