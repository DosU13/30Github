using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DebuffUI : MonoBehaviour
{
    public TMP_Text debuffText;
    public Image screen;

    public void Debuff(EnemyController.EnemyType debuffType)
    {
        StartCoroutine(ShowDebuff(debuffType));
    }

    private IEnumerator ShowDebuff(EnemyController.EnemyType debuffType)
    {
        Color debuffColor = GetDebuffColor(debuffType);
        debuffText.text = debuffType.ToString();
        debuffText.gameObject.SetActive(true);
        screen.color = new Color(debuffColor.r, debuffColor.g, debuffColor.b, 0);
        screen.gameObject.SetActive(true);

        float duration = 2f;
        float elapsedTime = 0f;
        float flickerSpeed = 0.1f;

        while (elapsedTime < duration)
        {
            float alpha = Mathf.PingPong(Time.time * 5, 1);
            debuffText.color = new Color(debuffText.color.r, debuffText.color.g, debuffText.color.b, alpha);
            screen.color = new Color(debuffColor.r, debuffColor.g, debuffColor.b, alpha * 0.5f);

            elapsedTime += flickerSpeed;
            yield return new WaitForSeconds(flickerSpeed);
        }

        debuffText.gameObject.SetActive(false);
        screen.gameObject.SetActive(false);
    }

    private Color GetDebuffColor(EnemyController.EnemyType debuffType)
    {
        switch (debuffType)
        {
            case EnemyController.EnemyType.Sadness:
                return Color.blue;
            case EnemyController.EnemyType.Fear:
                return new Color(0.5f, 0, 0.5f); // Violet
            case EnemyController.EnemyType.Anxiety:
                return new Color(0.86f, 0.08f, 0.24f); // Crimson
            default:
                return Color.white;
        }
    }
}
