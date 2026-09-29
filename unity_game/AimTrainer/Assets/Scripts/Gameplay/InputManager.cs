using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private TargetManager targetManager;
    [SerializeField] private SessionRecorder sessionRecorder;

    private void Awake()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        else
        {
            Debug.Log("Main camera is already assigned.");
        }
    }

    private void Update()
    {
        if (!Input.GetMouseButtonDown(0)) return;

        CheckMouseClick();
    }

    private void CheckMouseClick()
    {
        Target currentTarget = targetManager.CurrentTarget;

        if (currentTarget == null)
            return;

        Vector2 clickPosition = GetNormalizedClickPos();

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        bool hitTarget = false;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Target target = hit.collider.GetComponent<Target>();
            if (target != null && target == currentTarget)
            {
                hitTarget = true;
            }
            else
            {
                Debug.Log("Se ha pulsado algo que no es un objeto target");
                hitTarget = false;
            }
        }
        
        sessionRecorder.RegisterClick(currentTarget.TargetId, clickPosition, currentTarget.ElapsedTimeMs, hitTarget);

        if (hitTarget)
        {
            currentTarget.Hit();
        }
    }

    private Vector2 GetNormalizedClickPos()
    {
        Vector3 viewportPosition = mainCamera.ScreenToViewportPoint(Input.mousePosition);

        return new Vector2(
            viewportPosition.x,
            viewportPosition.y
        );
    }
}
