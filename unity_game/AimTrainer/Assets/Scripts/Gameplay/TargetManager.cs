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
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameObject leftUp;
    [SerializeField] private GameObject rightDown;
    public int tagetsSpawned = 0;
    static int maxTargets = 30;

    public event Action OnGameFinished;

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

        var targetComponent = instance.GetComponent<Target>();
        if (targetComponent != null)
        {
            // Suscribimos con métodos nombrados para poder desuscribirlos después
            targetComponent.OnExpired += HandleTargetExpired;
            targetComponent.OnHit += HandleTargetClicked;
            targetComponent.InitializeTarget();
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
        scoreManager.RegisterHit();

        RemoveTarget(t.gameObject);

        SpawnNextTarget();
    }

    private void HandleTargetExpired(Target t)
    {
        scoreManager.RegisterMiss();

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
        SpawnNextTarget();
    }
}
