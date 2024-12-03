using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiTouchDragDrop : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;

    private Camera mainCamera;

    // Dictionaries to track touch interactions
    private Dictionary<int, IPickable> activeDragObjects = new Dictionary<int, IPickable>();

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");
    }

    private void Update()
    {
        HandleTouchInput();
    }

    private void HandleTouchInput()
    {
        if (Touchscreen.current != null)
        {
            // Handle touchscreen input
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();
                Vector2 position = touch.position.ReadValue();

                switch (touch.phase.ReadValue())
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        HandleTouchStart(touchId, position);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Moved:
                    case UnityEngine.InputSystem.TouchPhase.Stationary:
                        HandleTouchMove(touchId, position);
                        break;
                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        HandleTouchEnd(touchId, position);
                        break;
                }
            }
        }
        else if (Mouse.current != null)
        {
            // Handle mouse input
            int touchId = 0;
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
                HandleTouchStart(touchId, mousePosition);
            else if (Mouse.current.leftButton.isPressed)
                HandleTouchMove(touchId, mousePosition);
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
                HandleTouchEnd(touchId, mousePosition);
        }
    }

    private void HandleTouchStart(int touchId, Vector2 position)
    {
        // Cast a ray to check for interactable objects
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, interactableLayer);

        if (hits.Length > 0)
        {
            // Sort hits by distance to prioritize closer objects
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            foreach (var hit in hits)
            {
                // Try handling different types of interactions in order
                if (HandleBasketTouch(hit, touchId, position)) return;
                if (HandlePlateStackTouch(hit, touchId, position)) return;
                if (HandleIngredientTouch(hit, touchId)) return;
                if (HandleContainerTouch(hit, touchId)) return;
                if (HandlePickableObjectTouch(hit, touchId)) return;
            }
        }
    }

    private void HandleTouchMove(int touchId, Vector2 position)
    {
        // Move dragged objects immediately
        if (activeDragObjects.TryGetValue(touchId, out IPickable pickable))
        {
            Vector3 worldPosition = GetWorldPosition(position);
            pickable.OnTouchMove(touchId, worldPosition);
        }
    }

    private void HandleTouchEnd(int touchId, Vector2 position)
    {
        // Drop dragged objects
        if (activeDragObjects.TryGetValue(touchId, out IPickable pickable))
        {
            pickable.OnTouchDrop(touchId, position);
            activeDragObjects.Remove(touchId);
        }

        ProcessIngredients();
    }

    private bool HandleBasketTouch(RaycastHit hit, int touchId, Vector2 position)
    {
        var basket = hit.collider.GetComponent<Basket>();
        if (basket != null)
        {
            basket.OnTouchDown(touchId, position);
            return true;
        }
        return false;
    }

    private bool HandlePlateStackTouch(RaycastHit hit, int touchId, Vector2 position)
    {
        var plateStack = hit.collider.GetComponent<PlateStack>();
        if (plateStack != null)
        {
            plateStack.OnTouchDown(touchId, position);
            return true;
        }
        return false;
    }

    private bool HandleIngredientTouch(RaycastHit hit, int touchId)
    {
        var ingredient = hit.collider.GetComponent<BaseIngredient>();
        var pickable = hit.collider.GetComponent<IPickable>();

        if (ingredient != null)
        {
            return HandlePickup(pickable, touchId);
        }
        return false;
    }

    private bool HandleContainerTouch(RaycastHit hit, int touchId)
    {
        var container = hit.collider.GetComponent<BaseContainer>();
        var pickable = hit.collider.GetComponent<IPickable>();

        if (container != null)
        {
            return HandlePickup(pickable, touchId);
        }
        return false;
    }

    private bool HandlePickableObjectTouch(RaycastHit hit, int touchId)
    {
        var pickable = hit.collider.GetComponent<IPickable>();
        if (pickable != null && !pickable.IsBeingDragged)
        {
            return HandlePickup(pickable, touchId);
        }
        return false;
    }

    private bool HandlePickup(IPickable pickable, int touchId)
    {
        if (pickable != null)
        {
            pickable.OnTouchPick(touchId);
            activeDragObjects[touchId] = pickable;
            return true;
        }
        return false;
    }

    private void ProcessIngredients()
    {
        var ingredients = FindObjectsOfType<BaseIngredient>();
        foreach (var ingredient in ingredients)
        {
            // Process cooking if on a cooking workstation
            if (ingredient.CanProcess(ProcessType.Cook) && 
                ingredient.GetCurrentWorkStation()?.GetStationType() == ProcessType.Cook)
            {
                ingredient.Process(ProcessType.Cook);
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
}