using UnityEngine;
using TMPro;
using System.Collections;

public class RhythmFeedbackDisplay : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShootingSystem shootingSystem;
    [SerializeField] private TextMeshProUGUI feedbackText;

    [Header("Settings")]
    [SerializeField] private Color perfectColor = new Color(1f, 0.8f, 0f); // Gold
    [SerializeField] private Color goodColor = new Color(0f, 0.8f, 0f);    // Green
    [SerializeField] private Color okColor = new Color(0f, 0.6f, 1f);      // Blue
    [SerializeField] private Color missColor = new Color(1f, 0.3f, 0.3f);  // Red

    [Header("Timing Thresholds")]
    [SerializeField] private float perfectThreshold = 0.05f; // Within 50ms
    [SerializeField] private float goodThreshold = 0.1f;     // Within 100ms
    [SerializeField] private float okThreshold = 0.2f;       // Within 200ms

    [Header("Animation")]
    [SerializeField] private float displayDuration = 1f;
    [SerializeField] private float fadeStartTime = 0.7f;
    [SerializeField] private float scaleUpAmount = 1.2f;
    [SerializeField] private float scaleUpDuration = 0.1f;
    [SerializeField] private float scaleDownDuration = 0.2f;

    private RhythmManager rhythmManager;
    private Coroutine currentAnimation;
    private bool isListening = false;

    private void Start()
    {
        if (shootingSystem == null)
        {
            shootingSystem = FindObjectOfType<ShootingSystem>();
            if (shootingSystem == null)
            {
                Debug.LogError("RhythmFeedbackDisplay: No ShootingSystem found in the scene!");
                enabled = false;
                return;
            }
        }

        if (feedbackText == null)
        {
            feedbackText = GetComponent<TextMeshProUGUI>();
            if (feedbackText == null)
            {
                Debug.LogError("RhythmFeedbackDisplay: No TextMeshProUGUI component found!");
                enabled = false;
                return;
            }
        }

        rhythmManager = shootingSystem.rhythmManager;

        // Hide text initially
        feedbackText.text = "";
        feedbackText.alpha = 0;

        StartListening();
    }

    private void OnEnable()
    {
        if (isListening == false && shootingSystem != null)
        {
            StartListening();
        }
    }

    private void OnDisable()
    {
        StopListening();
    }

    private void StartListening()
    {
        isListening = true;
        // Start monitoring input
        StartCoroutine(MonitorInput());
    }

    private void StopListening()
    {
        isListening = false;
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
            currentAnimation = null;
        }
    }

    private IEnumerator MonitorInput()
    {
        while (isListening)
        {
            // Check for the same input conditions as in ShootingSystem.ShouldFire()
            bool inputDetected = false;

            if (Input.anyKeyDown)
            {
                inputDetected = true;
            }

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began && touch.position.x > Screen.width / 2)
                {
                    inputDetected = true;
                }
            }

            if (inputDetected)
            {
                ShowFeedback();
            }

            yield return null;
        }
    }

    private void ShowFeedback()
    {
        float timeOffset = rhythmManager.GetDistanceToNearestBeat(Time.time);

        string feedbackMessage;
        Color feedbackColor;

        // Determine feedback category based on how close to the beat
        if (timeOffset <= perfectThreshold)
        {
            feedbackMessage = "Perfect!";
            feedbackColor = perfectColor;
        }
        else if (timeOffset <= goodThreshold)
        {
            feedbackMessage = "Good!";
            feedbackColor = goodColor;
        }
        else if (timeOffset <= okThreshold)
        {
            feedbackMessage = "OK";
            feedbackColor = okColor;
        }
        else
        {
            feedbackMessage = "Miss";
            feedbackColor = missColor;
        }

        // Stop any current animation and start a new one
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }

        currentAnimation = StartCoroutine(AnimateFeedback(feedbackMessage, feedbackColor));
    }

    private IEnumerator AnimateFeedback(string message, Color color)
    {
        // Set up text
        feedbackText.text = message;
        feedbackText.color = color;
        feedbackText.alpha = 1f;

        // Initial scale
        Vector3 originalScale = new Vector3(1,1,1);

        // Scale up
        float elapsedTime = 0f;
        while (elapsedTime < scaleUpDuration)
        {
            float t = elapsedTime / scaleUpDuration;
            float scale = Mathf.Lerp(1f, scaleUpAmount, t);
            feedbackText.transform.localScale = originalScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Scale down
        elapsedTime = 0f;
        while (elapsedTime < scaleDownDuration)
        {
            float t = elapsedTime / scaleDownDuration;
            float scale = Mathf.Lerp(scaleUpAmount, 1f, t);
            feedbackText.transform.localScale = originalScale * scale;

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Reset scale
        feedbackText.transform.localScale = originalScale;

        // Wait before starting to fade
        yield return new WaitForSeconds(fadeStartTime);

        // Fade out
        elapsedTime = 0f;
        float fadeDuration = displayDuration - fadeStartTime;
        while (elapsedTime < fadeDuration)
        {
            float t = elapsedTime / fadeDuration;
            feedbackText.alpha = Mathf.Lerp(1f, 0f, t);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure fully transparent
        feedbackText.alpha = 0f;

        currentAnimation = null;
    }
}