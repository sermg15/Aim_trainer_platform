using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float maxRayDistance = 100f;
    [SerializeField] private ScoreManager scoreManager;
    [SerializeField] private GameManager gameManager;
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
        if (gameManager.inGame)
        {
            if (!Input.GetMouseButtonDown(0)) return;

            CheckMouseClick();
        }
    }

    private void CheckMouseClick()
    {
        Vector2 normalizedClickPosition = new Vector2(
            Input.mousePosition.x / Screen.width,
            Input.mousePosition.y / Screen.height
        );

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Target target = hit.collider.GetComponent<Target>();
            if (target != null)
            {
                sessionRecorder.RegisterClick(target.TargetId, normalizedClickPosition, true);
                target.Hit();
            }
            else
            {
                Debug.Log("Se ha pulsado algo que no es un objeto target");
                sessionRecorder.RegisterClick(null, normalizedClickPosition, false);
                //scoreManager.RegisterMiss();
            }
        }
        else
        {
            Debug.Log("No se ha pulsado nada");
            sessionRecorder.RegisterClick(null, normalizedClickPosition, false);
            //scoreManager.RegisterMiss();
        }
    }
}
