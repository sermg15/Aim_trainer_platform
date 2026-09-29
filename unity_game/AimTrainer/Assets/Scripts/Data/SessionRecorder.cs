using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SessionRecorder : MonoBehaviour
{
    private SessionData currentSession;

    public void StartSession(int maxTargets, float targetLifetimeSeconds)
    {
        currentSession = new SessionData();

        currentSession.sessionId = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        currentSession.date = DateTime.UtcNow.ToString("o");

        currentSession.maxTargets = maxTargets;
        currentSession.targetLifetimeMs = targetLifetimeSeconds * 1000f;

        // ************************************
        // LOGS TO DEBUG CONSOLE 

        Debug.Log(
            $"[SESSION START] " +
            $"ID: {currentSession.sessionId} | " +
            $"Targets: {currentSession.maxTargets} | " +
            $"Lifetime: {currentSession.targetLifetimeMs} ms"
        );

        // *************************************
    }

    public void RegisterTarget (int targetId, Vector2 targetPos)
    {
        currentSession.targets.Add(new TargetData(targetId, targetPos.x, targetPos.y));

        // ************************************
        // LOGS TO DEBUG CONSOLE 

        Debug.Log($"[TARGET REGISTERED] ID: {targetId} | Position: ({targetPos.x}, {targetPos.y})");

        // *************************************
    }

    public void RegisterClick(int targetId, Vector2 pos, float timeMs, bool hit)
    {
        TargetData target = currentSession.targets.Find(t => t.targetId == targetId);

        if (target == null)
        {
            Debug.LogWarning($"Target {targetId} not found in current session.");
            return;
        }

        ClickData click = new ClickData(timeMs, pos.x, pos.y, hit);

        target.clicks.Add(click);

        // *************************************
        // LOGS TO DEBUG CONSOLE 

        Debug.Log(
            $"[CLICK] Target: {targetId} | " +
            $"Time: {timeMs:F0} ms | " +
            $"Position: ({pos.x:F2}, {pos.y:F2}) | " +
            $"Hit: {hit}"
        );

        // *************************************
    }

    public void RegisterHit (int targetId, float reactionTimeMs)
    {
        TargetData target = currentSession.targets.Find(t => t.targetId == targetId);

        if (target == null)
        {
            Debug.LogWarning($"Target {targetId} not found in current session.");
            return;
        }

        target.result = "hit";
        target.reactionTimeMs = reactionTimeMs;

        // *************************************
        // LOGS TO DEBUG CONSOLE 

        Debug.Log(
            $"[HIT] Target: {targetId} | " +
            $"Reaction time: {reactionTimeMs:F0} ms"
        );

        // *************************************
    }

    public void RegisterTimeout(int targetId)
    {
        TargetData target = currentSession.targets.Find(t => t.targetId == targetId);

        if (target == null)
        {
            Debug.LogWarning($"Target {targetId} not found in current session.");
            return;
        }

        target.result = "timeout";
        target.reactionTimeMs = -1f;

        // *************************************
        // LOGS TO DEBUG CONSOLE 

        Debug.Log(
            $"[TIMEOUT] Target: {targetId}"
        );

        // *************************************
    }

    public void DebugSession()
    {
        Debug.Log(
            $"===== SESSION {currentSession.sessionId} ====="
        );

        foreach (TargetData target in currentSession.targets)
        {
            Debug.Log(
                $"Target {target.targetId} | " +
                $"Result: {target.result} | " +
                $"Reaction: {target.reactionTimeMs:F0} ms | " +
                $"Clicks: {target.clicks.Count}"
            );

            foreach (ClickData click in target.clicks)
            {
                Debug.Log(
                    $"    Click | " +
                    $"Time: {click.timeMs:F0} ms | " +
                    $"Position: ({click.x:F2}, {click.y:F2}) | " +
                    $"Hit: {click.hit}"
                );
            }
        }

        Debug.Log("============================");
    }

    public void EndSession()
    {
        ExportToJson();
    }

    private void ExportToJson()
    {
        string json = JsonUtility.ToJson(currentSession, true);
        string fileName = $"Session_{currentSession.sessionId}.json";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        try
        {
            File.WriteAllText(filePath, json);
            Debug.Log($"Session data exported to: {filePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to export session data: {e.Message}");
        }
    }
}