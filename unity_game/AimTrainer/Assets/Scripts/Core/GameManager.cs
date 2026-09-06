using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private Button buttonStart;

    private void Awake()
    {
        targetManager.OnGameFinished += EndGame;
    }

    public void StartGame()
    {
        buttonStart.gameObject.SetActive(false);
        scoreManager.resetScores();
        targetManager.StartGame();
    }

    public void EndGame()
    {
        scoreManager.PrintResults();
        buttonStart.gameObject.SetActive(true);
    }
}
