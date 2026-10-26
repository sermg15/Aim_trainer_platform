using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public int misses = 0;
    public int hits = 0;

    private void Awake()
    {
        hits = 0;
        misses = 0;
    }

    public void RegisterHit()
    {
        hits++;
        Debug.Log($"Hits: {hits}");
    }

    public void RegisterMiss()
    {
        misses++;
        Debug.Log($"Misses: {misses}");
    }

    public void PrintResults()
    {
        Debug.Log($"Final Results - Hits: {hits}, Misses: {misses}");
    }

    public void resetScores()
    {
        hits = 0;
        misses = 0;
        Debug.Log("Scores reset.");
    }
}
