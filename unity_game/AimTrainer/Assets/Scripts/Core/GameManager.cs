using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private Button buttonStart;
    [SerializeField] private SessionRecorder sessionRecorder;

    private void Awake()
    {
        targetManager.OnGameFinished += EndGame;
    }

    public void StartGame()
    {
        buttonStart.gameObject.SetActive(false);
        sessionRecorder.StartSession(targetManager.MaxTargets, targetManager.TargetLifetimeSeconds);
        targetManager.StartGame();
    }

    public void EndGame()
    {
        sessionRecorder.DebugSession();
        sessionRecorder.EndSession();
        buttonStart.gameObject.SetActive(true);
    }
}
