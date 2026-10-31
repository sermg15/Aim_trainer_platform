using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class Target : MonoBehaviour
{
    private float maxLifetime;
    private Coroutine lifeCoroutine;
    private bool alive;
    public event Action<Target> OnExpired;
    public event Action<Target> OnHit;

    public int TargetId { get; private set; }

    public void InitializeTarget(int targetId, float maxLifetime)
    {
        TargetId = targetId;
        this.maxLifetime = maxLifetime;
        if (lifeCoroutine != null) StopCoroutine(lifeCoroutine);
        lifeCoroutine = StartCoroutine(LifeCountdown());
    }

    private IEnumerator LifeCountdown()
    {
        alive = true;
        float remaining = maxLifetime;
        while (remaining > 0f)
        {
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!alive) yield break;
        alive = false;
        OnExpired?.Invoke(this);
    }

    public void Hit()
    {
        if (!alive) return;

        alive = false;

        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }

        OnHit?.Invoke(this);
    }

    private void OnDestroy()
    {
        if (lifeCoroutine != null)
        {
            StopCoroutine(lifeCoroutine);
            lifeCoroutine = null;
        }
    }
}
