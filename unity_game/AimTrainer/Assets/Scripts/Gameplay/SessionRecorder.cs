using Newtonsoft.Json;
using System;
using System.IO;
using System.Xml;
using UnityEngine;

public class SessionRecorder : MonoBehaviour
{
    private SessionData currentSession;

    private float sessionStartTime;

    public void StartSession(
        string playerId,
        int maxTargets,
        float targetLifetimeSeconds)
    {
        currentSession = new SessionData();

        currentSession.schemaVersion = "1.0";
        currentSession.sessionId = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
        currentSession.playerId = playerId;
        currentSession.timestamp = DateTime.UtcNow.ToString("o");

        currentSession.maxTargets = maxTargets;
        currentSession.targetLifetimeSeconds = targetLifetimeSeconds;

        sessionStartTime = Time.realtimeSinceStartup;

        Debug.Log(
            $"[SESSION START] " +
            $"ID: {currentSession.sessionId} | " +
            $"Player: {currentSession.playerId} | " +
            $"Targets: {currentSession.maxTargets} | " +
            $"Lifetime: {currentSession.targetLifetimeSeconds}s"
        );
    }

    public void RegisterTarget(int targetId, Vector2 targetPos)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session.");
            return;
        }

        float spawnTimeMs = GetElapsedTimeMs();

        TargetData target = new TargetData(
            targetId,
            spawnTimeMs,
            targetPos.x,
            targetPos.y
        );

        currentSession.targets.Add(target);

        Debug.Log(
            $"[TARGET REGISTERED] " +
            $"ID: {targetId} | " +
            $"Spawn: {spawnTimeMs:F0} ms | " +
            $"Position: ({targetPos.x:F2}, {targetPos.y:F2})"
        );
    }

    public void RegisterClick(
        int? targetId,
        Vector2 pos,
        bool hit)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session.");
            return;
        }

        float timeMs = GetElapsedTimeMs();

        ClickData click = new ClickData(
            timeMs,
            pos.x,
            pos.y,
            hit,
            targetId
        );

        currentSession.clicks.Add(click);

        Debug.Log(
            $"[CLICK] " +
            $"Target: {(targetId.HasValue ? targetId.Value.ToString() : "null")} | " +
            $"Time: {timeMs:F0} ms | " +
            $"Position: ({pos.x:F2}, {pos.y:F2}) | " +
            $"Hit: {hit}"
        );
    }

    public void RegisterHit(int targetId)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session.");
            return;
        }

        TargetData target =
            currentSession.targets.Find(t => t.targetId == targetId);

        if (target == null)
        {
            Debug.LogWarning(
                $"Target {targetId} not found in current session."
            );

            return;
        }

        float despawnTimeMs = GetElapsedTimeMs();

        target.despawnTimeMs = despawnTimeMs;
        target.result = "hit";

        target.reactionTimeMs = despawnTimeMs - target.spawnTimeMs;

        Debug.Log(
            $"[HIT] " +
            $"Target: {targetId} | " +
            $"Reaction time: {target.reactionTimeMs:F0} ms"
        );
    }

    public void RegisterTimeout(int targetId)
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session.");
            return;
        }

        TargetData target =
            currentSession.targets.Find(t => t.targetId == targetId);

        if (target == null)
        {
            Debug.LogWarning(
                $"Target {targetId} not found in current session."
            );

            return;
        }

        target.despawnTimeMs = GetElapsedTimeMs();
        target.result = "timeout";
        target.reactionTimeMs = null;

        Debug.Log(
            $"[TIMEOUT] Target: {targetId}"
        );
    }

    public void EndSession()
    {
        if (currentSession == null)
        {
            Debug.LogWarning("No active session to end.");
            return;
        }

        DebugSession();
        ExportToJson();
    }

    private float GetElapsedTimeMs()
    {
        return (Time.realtimeSinceStartup - sessionStartTime) * 1000f;
    }

    private void ExportToJson()
    {
        string json = JsonConvert.SerializeObject(
            currentSession,
            Newtonsoft.Json.Formatting.Indented
        );

        string directoryPath = Path.Combine(
            Application.persistentDataPath,
            "AimTrainerData"
        );

        Directory.CreateDirectory(directoryPath);

        string fileName =
            $"Session_{currentSession.sessionId}.json";

        string filePath =
            Path.Combine(directoryPath, fileName);

        try
        {
            File.WriteAllText(filePath, json);

            Debug.Log(
                $"[SESSION EXPORTED] {filePath}"
            );
        }
        catch (Exception e)
        {
            Debug.LogError(
                $"Failed to export session data: {e.Message}"
            );
        }
    }

    public void DebugSession()
    {
        if (currentSession == null)
            return;

        Debug.Log(
            $"===== SESSION {currentSession.sessionId} ====="
        );

        Debug.Log(
            $"Player: {currentSession.playerId} | " +
            $"Targets: {currentSession.targets.Count} | " +
            $"Clicks: {currentSession.clicks.Count}"
        );

        foreach (TargetData target in currentSession.targets)
        {
            string reaction =
                target.reactionTimeMs.HasValue
                    ? $"{target.reactionTimeMs.Value:F0} ms"
                    : "null";

            Debug.Log(
                $"Target {target.targetId} | " +
                $"Position: ({target.targetX:F2}, {target.targetY:F2}) | " +
                $"Result: {target.result} | " +
                $"Spawn: {target.spawnTimeMs:F0} ms | " +
                $"Despawn: {target.despawnTimeMs:F0} ms | " +
                $"Reaction: {reaction}"
            );
        }

        foreach (ClickData click in currentSession.clicks)
        {
            Debug.Log(
                $"Click | " +
                $"Time: {click.timeMs:F0} ms | " +
                $"Position: ({click.posX:F2}, {click.posY:F2}) | " +
                $"Hit: {click.hit} | " +
                $"Target: {(click.targetId.HasValue ? click.targetId.Value.ToString() : "null")}"
            );
        }

        Debug.Log("============================");
    }
}