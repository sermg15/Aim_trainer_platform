using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class TargetManager : MonoBehaviour
{
    [SerializeField] private GameObject targetPrefab;
    private List<GameObject> activeTargets = new List<GameObject>();
    private float maxLifetime = 1.5f; // Maximum lifetime of a target in seconds
    //[SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameObject leftUp;
    [SerializeField] private GameObject rightDown;
    public int tagetsSpawned = 0;
    private int targetId = 0;
    private Target currentTarget;

    [SerializeField] private int maxTargets = 30;
    [SerializeField] private float targetLifetimeSeconds = 1.5f;

    public int MaxTargets => maxTargets;
    public float TargetLifetimeSeconds => targetLifetimeSeconds;
    public Target CurrentTarget => currentTarget;

    public event Action OnGameFinished;

    [SerializeField] private SessionRecorder sessionRecorder;

    public Vector3 calculateTargetPosition()
    {
        Vector3 leftUpPos = leftUp.transform.position;
        Vector3 rightDownPos = rightDown.transform.position;

        float minX = Mathf.Min(leftUpPos.x, rightDownPos.x);
        float maxX = Mathf.Max(leftUpPos.x, rightDownPos.x);
        float minY = Mathf.Min(leftUpPos.y, rightDownPos.y);
        float maxY = Mathf.Max(leftUpPos.y, rightDownPos.y);
        float minZ = Mathf.Min(leftUpPos.z, rightDownPos.z);
        float maxZ = Mathf.Max(leftUpPos.z, rightDownPos.z);

        float x = Random.Range(minX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(minZ, maxZ);

        return new Vector3(x, y, z);
    }

    public void SpawnTarget(Vector3 position)
    {
        GameObject instance = Instantiate(targetPrefab, position, Quaternion.identity);
        activeTargets.Add(instance);

        currentTarget = instance.GetComponent<Target>();
        if (currentTarget != null)
        {
            // Suscribimos con métodos nombrados para poder desuscribirlos después
            currentTarget.OnExpired += HandleTargetExpired;
            currentTarget.OnHit += HandleTargetClicked;
            currentTarget.InitializeTarget(targetId, targetLifetimeSeconds);
            sessionRecorder.RegisterTarget(targetId, position);
            targetId++;
        }
        else
        {
            Debug.LogWarning("El prefab no tiene el componente Target.");
        }
    }

    public void SpawnNextTarget()
    {
        if (tagetsSpawned >= maxTargets)
        {
            Debug.Log("Max targets reached. Ending game.");
            OnGameFinished?.Invoke();
            return;
        }

        SpawnTarget(calculateTargetPosition());
        tagetsSpawned++;
    }

    private void HandleTargetClicked(Target t)
    {
        sessionRecorder.RegisterHit(t.TargetId, t.ElapsedTimeMs);

        RemoveTarget(t.gameObject);

        SpawnNextTarget();
    }

    private void HandleTargetExpired(Target t)
    {
        sessionRecorder.RegisterTimeout(t.TargetId);

        RemoveTarget(t.gameObject);

        SpawnNextTarget();
    }

    public void RemoveTarget(GameObject target)
    {
        if (target == null) return;

        var targetComp = target.GetComponent<Target>();
        if (targetComp != null)
        {
            // Desuscribimos para evitar fugas de memoria
            targetComp.OnExpired -= HandleTargetExpired;
            targetComp.OnHit -= HandleTargetClicked;

            if (currentTarget == targetComp)
            {
                currentTarget = null;
            }
        }

        if (activeTargets.Contains(target))
        {
            activeTargets.Remove(target);
        }

        Destroy(target);
    }

    public void StartGame()
    {
        tagetsSpawned = 0;
        targetId = 0;
        SpawnNextTarget();
    }
}
