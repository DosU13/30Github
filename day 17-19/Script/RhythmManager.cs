using System.Collections.Generic;
using UnityEngine;

public class RhythmManager : MonoBehaviour
{
    public float bpm = 120f; // Beats per minute
    private float beatInterval; // Time between beats
    private float startTime;

    void Start()
    {
        beatInterval = 60f / bpm; // Convert BPM to time per beat
        startTime = Time.time;    // Record when the rhythm started
    }

    // Returns how far away the given time is from the nearest beat
    public float GetDistanceToNearestBeat(float time)
    {
        float elapsedTime = time - startTime;
        float beatProgress = elapsedTime % beatInterval; // Time since last beat
        float distanceToNext = beatInterval - beatProgress;
        return Mathf.Min(beatProgress, distanceToNext); // Return closest distance
    }

    // Returns a list of upcoming beats in the next 'lookaheadTime' seconds
    public List<float> GetUpcomingBeats(float lookaheadTime = 2)
    {
        List<float> upcomingBeats = new List<float>();
        float currentTime = Time.time;
        float nextBeatTime = startTime + Mathf.Ceil((currentTime - startTime) / beatInterval) * beatInterval;

        while (nextBeatTime <= currentTime + lookaheadTime)
        {
            upcomingBeats.Add(nextBeatTime);
            nextBeatTime += beatInterval;
        }

        return upcomingBeats;
    }
}
