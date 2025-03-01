using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class BeatVisualizer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RhythmManager rhythmManager;

    [Header("Visualization Settings")]
    [SerializeField] private float lookaheadTime = 2f;
    [SerializeField] private float lineThickness = 2f;
    [SerializeField] private float beatSegmentHeight = 20f;
    [SerializeField] private Color lineColor = Color.white;
    [SerializeField] private Color beatColor = Color.red;

    private RectTransform rectTransform;
    private Image horizontalLine;
    private List<Image> leftBeatMarkers = new List<Image>();
    private List<Image> rightBeatMarkers = new List<Image>();
    private float lineWidth;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        // Create horizontal line
        GameObject lineObj = new GameObject("HorizontalLine");
        lineObj.transform.SetParent(transform, false);
        horizontalLine = lineObj.AddComponent<Image>();
        horizontalLine.color = lineColor;

        RectTransform lineRect = lineObj.GetComponent<RectTransform>();
        lineRect.anchorMin = new Vector2(0f, 0.1f);
        lineRect.anchorMax = new Vector2(1f, 0.1f);
        lineRect.sizeDelta = new Vector2(0f, lineThickness);

        // Create center line
        GameObject centerLineObj = new GameObject("CenterLine");
        centerLineObj.transform.SetParent(transform, false);

        Image centerLineImage = centerLineObj.AddComponent<Image>();
        centerLineImage.color = lineColor;

        RectTransform centerLineRect = centerLineObj.GetComponent<RectTransform>();
        centerLineRect.anchorMin = new Vector2(0.5f, 0.1f);
        centerLineRect.anchorMax = new Vector2(0.5f, 0.1f);
        centerLineRect.sizeDelta = new Vector2(lineThickness, beatSegmentHeight);
    }

    private void Start()
    {
        if (rhythmManager == null)
        {
            rhythmManager = FindObjectOfType<RhythmManager>();
            if (rhythmManager == null)
            {
                Debug.LogError("BeatVisualizer: No RhythmManager found in the scene!");
                enabled = false;
                return;
            }
        }
    }

    private void Update()
    {
        UpdateBeatVisualization();
    }

    private void UpdateBeatVisualization()
    {
        // Get upcoming beats from RhythmManager
        List<float> upcomingBeats = rhythmManager.GetUpcomingBeats(lookaheadTime);

        // Ensure we have enough beat markers (need two per beat - left and right)
        while (leftBeatMarkers.Count < upcomingBeats.Count)
        {
            CreateNewBeatMarker(true);  // Left side
            CreateNewBeatMarker(false); // Right side
        }

        // Hide unused markers
        for (int i = 0; i < leftBeatMarkers.Count; i++)
        {
            bool isActive = i < upcomingBeats.Count;
            leftBeatMarkers[i].gameObject.SetActive(isActive);
            rightBeatMarkers[i].gameObject.SetActive(isActive);
        }

        // Update position of visible markers
        lineWidth = rectTransform.rect.width;
        float centerX = lineWidth / 2;

        for (int i = 0; i < upcomingBeats.Count; i++)
        {
            float beatTime = upcomingBeats[i] - Time.time;
            // Time ratio (0 to 1) where 0 is now and 1 is at the end of lookahead time
            float timeRatio = beatTime / lookaheadTime;
            Debug.Log(beatTime +" "+ timeRatio);

            // Invert the ratio so beats move from outside towards center
            float positionRatio = timeRatio;

            // Position from edge (0) to center (1)
            float leftX = centerX - (positionRatio * centerX);
            float rightX = centerX + (positionRatio * centerX);

            // Position the left marker
            RectTransform leftMarkerRect = leftBeatMarkers[i].GetComponent<RectTransform>();
            Vector2 leftAnchoredPosition = leftMarkerRect.anchoredPosition;
            leftAnchoredPosition.x = leftX - (lineThickness / 2);
            leftMarkerRect.anchoredPosition = leftAnchoredPosition;

            // Position the right marker
            RectTransform rightMarkerRect = rightBeatMarkers[i].GetComponent<RectTransform>();
            Vector2 rightAnchoredPosition = rightMarkerRect.anchoredPosition;
            rightAnchoredPosition.x = rightX - (lineThickness / 2);
            rightMarkerRect.anchoredPosition = rightAnchoredPosition;
        }
    }

    private void CreateNewBeatMarker(bool isLeft)
    {
        string side = isLeft ? "Left" : "Right";
        GameObject markerObj = new GameObject("BeatMarker_" + side + "_" + (isLeft ? leftBeatMarkers.Count : rightBeatMarkers.Count));
        markerObj.transform.SetParent(transform, false);

        Image markerImage = markerObj.AddComponent<Image>();
        markerImage.color = beatColor;

        RectTransform markerRect = markerObj.GetComponent<RectTransform>();
        markerRect.anchorMin = new Vector2(0, 0.1f);
        markerRect.anchorMax = new Vector2(0, 0.1f);
        markerRect.sizeDelta = new Vector2(lineThickness, beatSegmentHeight);

        if (isLeft)
            leftBeatMarkers.Add(markerImage);
        else
            rightBeatMarkers.Add(markerImage);
    }

    // Optional: Clear all beat markers when disabled or destroyed
    private void OnDisable()
    {
        foreach (Image marker in leftBeatMarkers)
        {
            if (marker != null && marker.gameObject != null)
            {
                Destroy(marker.gameObject);
            }
        }

        foreach (Image marker in rightBeatMarkers)
        {
            if (marker != null && marker.gameObject != null)
            {
                Destroy(marker.gameObject);
            }
        }

        leftBeatMarkers.Clear();
        rightBeatMarkers.Clear();
    }
}