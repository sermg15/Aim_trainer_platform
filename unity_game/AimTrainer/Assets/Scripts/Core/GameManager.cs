using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private GameObject settingsMenu;
    [SerializeField] private SessionRecorder sessionRecorder;

    public bool inGame;

    private const string playerId = "TestPlayer";

    private void Awake()
    {
        targetManager.OnGameFinished += EndGame;
        inGame = false;
    }

    public void StartGame(GameSettings settings)
    {
        inGame = true;

        targetManager.maxTargets = settings.maxTargets;

        sessionRecorder.StartSession(playerId, targetManager.maxTargets, targetManager.maxLifetime);

        settingsMenu.SetActive(false);
        scoreManager.resetScores();
        targetManager.StartGame();

        Debug.Log("Targets set to: " + targetManager.maxTargets);
    }

    public void EndGame()
    {
        inGame = false;
        //scoreManager.PrintResults();
        sessionRecorder.EndSession();
        settingsMenu.SetActive(true);
    }
}
