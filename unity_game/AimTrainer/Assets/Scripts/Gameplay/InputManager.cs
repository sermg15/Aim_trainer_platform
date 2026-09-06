using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private ScoreManager scoreManager;

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
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                target.Hit();
            }
            else
            {
                Debug.Log("No target hit.");
                //scoreManager.RegisterMiss();
            }
        }
    }
}
