using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    private TouchInputActions touchActions;
    private Dictionary<int, bool> activeTouches = new Dictionary<int, bool>();

    [SerializeField] private float touchRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;

    private void Awake()
    {
        Debug.Log("TouchManager: Awake");
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }

        touchActions = new TouchInputActions();
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        touchActions.Touch.PrimaryTouch.started += ctx =>
        {
            var touchId = GetTouchId(ctx);
            Debug.Log($"TouchManager: Touch started with ID {touchId}");
            OnTouchStarted(touchId);
        };

        touchActions.Touch.PrimaryTouch.canceled += ctx =>
        {
            var touchId = GetTouchId(ctx);
            Debug.Log($"TouchManager: Touch canceled with ID {touchId}");
            OnTouchEnded(touchId);
        };

        touchActions.Touch.PrimaryTouchPosition.performed += ctx =>
        {
            var touchId = GetTouchId(ctx);
            if (activeTouches.ContainsKey(touchId) && activeTouches[touchId])
            {
                Vector2 position = ctx.ReadValue<Vector2>();
                UpdateObjectPosition(touchId, position);
                Debug.Log($"Touch {touchId} position updated: {position}");
            }
        };
    }

    private int GetTouchId(InputAction.CallbackContext ctx)
    {
        // Récupérer l'ID unique du touch à partir du contexte
        if (ctx.control?.device is Touchscreen touchScreen)
        {
            return touchScreen.deviceId;
        }
        return 0; // Fallback pour la souris ou autres dispositifs
    }

    private void OnTouchStarted(int touchId)
    {
        activeTouches[touchId] = true;
        Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        HandleTouchBegan(touchId, position);
    }

    private void OnTouchEnded(int touchId)
    {
        if (activeTouches.ContainsKey(touchId) && activeTouches[touchId])
        {
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            HandleTouchEnded(touchId, position);
            activeTouches.Remove(touchId);
        }
    }

    private void HandleTouchBegan(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchBegan - TouchID: {touchId}, Position: {position}");
        Ray ray = mainCamera.ScreenPointToRay(position);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5f);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, interactableLayer);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                var basket = hit.collider.GetComponent<Basket>();
                if (basket != null)
                {
                    basket.OnTouchDown(touchId, position);
                    return;
                }

                var pickable = hit.collider.GetComponent<IPickable>();
                if (pickable != null && !pickable.IsBeingDragged)
                {
                    pickable.OnTouchPick(touchId);
                    return;
                }

                var workStation = hit.collider.GetComponent<WorkStation>();
                if (workStation != null)
                {
                    workStation.StartProcessing();
                    return;
                }
            }
        }
    }

    private void HandleTouchEnded(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchEnded - TouchID: {touchId}, Position: {position}");
        
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == touchId && pickable.IsBeingDragged)
            {
                pickable.OnTouchDrop(touchId, position);
            }
        }
    }

    private void UpdateObjectPosition(int touchId, Vector2 screenPosition)
    {
        Vector3 worldPosition = GetWorldPosition(screenPosition);
        
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == touchId && pickable.IsBeingDragged)
            {
                pickable.OnTouchMove(touchId, worldPosition);
                Debug.Log($"Moving object with touch {touchId} to: {worldPosition}");
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float distanceFromCamera = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            distanceFromCamera
        ));
        worldPos.z = -10f;
        return worldPos;
    }

    private void OnEnable()
    {
        touchActions.Enable();
    }

    private void OnDisable()
    {
        touchActions.Disable();
    }

    private void Start()
    {
        interactableLayer = 1 << 3;
        Debug.Log($"Layer mask configured: {interactableLayer.value}");
    }
}