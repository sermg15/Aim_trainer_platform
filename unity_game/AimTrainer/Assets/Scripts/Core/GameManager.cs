using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private GameObject settingsMenu;

    public bool inGame;

    private void Awake()
    {
        targetManager.OnGameFinished += EndGame;
        inGame = false;
    }

    public void StartGame(GameSettings settings)
    {
        inGame = true;
        targetManager.maxTargets = settings.maxTargets;
        settingsMenu.SetActive(false);
        scoreManager.resetScores();
        targetManager.StartGame();

        Debug.Log("Targets set to: " + targetManager.maxTargets);
    }

    public void EndGame()
    {
        inGame = false;
        scoreManager.PrintResults();
        settingsMenu.SetActive(true);
    }
}
