using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiTouchDragDrop : MonoBehaviour
{
    [SerializeField] private LayerMask interactableLayer;
    private Camera mainCamera;

    private Dictionary<int, PickableObject> activeTouches = new Dictionary<int, PickableObject>();
    private Dictionary<int, TouchInfo> touchInfos = new Dictionary<int, TouchInfo>();

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
            Debug.LogError("Main Camera not found!");
    }

    private void Update()
    {
        if (Touchscreen.current != null) 
        {
            // Handle touchscreen input
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();

                if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    HandleTouchStart(touchId, touch.position.ReadValue());
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Moved || 
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Stationary)
                {
                    HandleTouchMove(touchId, touch.position.ReadValue());
                }
                else if (touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Ended || 
                         touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled)
                {
                    HandleTouchEnd(touchId);
                }
            }
        }
        else if (Mouse.current != null) 
        {
            // Handle mouse input
            int touchId = 0; // You can use touchId 0 for the first mouse input
            Vector2 mousePosition = Mouse.current.position.ReadValue();

            if (Mouse.current.leftButton.wasPressedThisFrame)
            {
                HandleTouchStart(touchId, mousePosition);
            }
            else if (Mouse.current.leftButton.isPressed)
            {
                HandleTouchMove(touchId, mousePosition);
            }
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
            {
                HandleTouchEnd(touchId);
            }
        }
    }

    
    private GameObject GetTouchedObject(Vector2 position)
    {
        Ray ray = mainCamera.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            return hit.collider.gameObject;
        }
        return null; 
    }

    // HANDLE TOUCH START
    private void HandleTouchStart(int touchId, Vector2 position)
    {
        Debug.Log($"HandleTouchStart - TouchID: {touchId}, Position: {position}");

        // Create a TouchInfo object to store touch info for this touchId
        GameObject touchedObject = GetTouchedObject(position);
        var pickableObject = touchedObject?.GetComponent<PickableObject>();
    
        if (pickableObject != null)
        {
            activeTouches[touchId] = pickableObject;
            touchInfos[touchId] = new TouchInfo
            {
                startPosition = position,
                startTime = Time.time,
                isHolding = false,
                targetObject = touchedObject
            };
        }
        

        // Cast a ray to check for interactable objects
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, interactableLayer);

        if (hits.Length > 0)
        {
            // Sort the hits by distance to prioritize closer objects
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

            // Process the closest interactable object
            foreach (var hit in hits)
            {
                if (HandleBasketTouch(hit, touchId)) return;
                if (HandlePlateStackTouch(hit, touchId)) return;
                if (HandleIngredientTouch(hit, touchId)) return;
                if (HandleContainerTouch(hit, touchId)) return;
                if (HandlePickableObjectTouch(hit, touchId)) return;
            }
        }
    }


    private bool IsAnyObjectBeingDragged()
    {
        return FindObjectsOfType<MonoBehaviour>()
            .OfType<IPickable>()
            .Any(p => p.IsBeingDragged);
    }

    private bool HandleBasketTouch(RaycastHit hit, int touchId)
    {
        Debug.Log($"//////////////////// Basket touched: {hit.collider.name}");
        var basket = hit.collider.GetComponent<Basket>();
        Vector2 basketPosition = hit.collider.transform.position;
        if (basket != null)
        {
            Debug.Log($"//////////////// Basket touched: {basket.name}");
            basket.OnTouchDown(touchId, basketPosition);
            return true; 
        }
        return false;
    }

    private bool HandlePlateStackTouch(RaycastHit hit, int touchId)
    {
        var plateStack = hit.collider.GetComponent<PlateStack>();
        if (plateStack != null)
        {
            Debug.Log($"PlateStack touched: {plateStack.name}");
            plateStack.OnTouchDown(touchId, hit.point);
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
            if (ingredient.GetCurrentWorkStation() != null)
            {
                HandleHoldDetection(ingredient, pickable, touchId, touchInfos[touchId].startPosition);
                return true;
            }
            else
            {
                Debug.Log($"Picking up ingredient: {ingredient.name}");
                pickable?.OnTouchPick(touchId);
                return true;
            }
        }
        return false;
    }

    private bool HandleContainerTouch(RaycastHit hit, int touchId)
    {
        var container = hit.collider.GetComponent<BaseContainer>();
        var pickable = hit.collider.GetComponent<IPickable>();

        if (container != null)
        {
            if (container.GetCurrentWorkStation() != null)
            {
                HandleHoldDetection(container, pickable, touchId, touchInfos[touchId].startPosition);
                return true;
            }
            else
            {
                Debug.Log($"Picking up container: {container.name}");
                pickable?.OnTouchPick(touchId);
                return true;
            }
        }
        return false;
    }

    private bool HandlePickableObjectTouch(RaycastHit hit, int touchId)
    {
        var pickable = hit.collider.GetComponent<IPickable>();
        if (pickable != null && !pickable.IsBeingDragged)
        {
            Debug.Log($"Picking up other object: {hit.collider.name}");
            pickable.OnTouchPick(touchId);
            return true;
        }
        return false;
    }

    private void HandleHoldDetection(MonoBehaviour interactableObject, IPickable pickable, int touchId, Vector2 position)
    {
        var holdDetector = interactableObject.GetComponent<HoldDetector>();
        if (holdDetector != null)
        {
            Debug.Log($"Starting hold on {interactableObject.name}");
            holdDetector.StartHolding(touchId, position);
            touchInfos[touchId].isHolding = true;
        }
        else
        {
            Debug.Log($"Picking up {interactableObject.name}");
            pickable?.OnTouchPick(touchId);
        }
    }



    private void HandleTouchMove(int touchId, Vector2 position)
    {
        if (activeTouches.ContainsKey(touchId))
        {
            var touchedObject = activeTouches[touchId];
            if (touchedObject != null)
            {
                Vector3 newPosition = GetWorldPosition(position);
                touchedObject.transform.position = newPosition;
                Debug.Log($"Touch {touchId} moved {touchedObject.name} to {newPosition}");
            }
        }
    }


    private void HandleTouchEnd(int touchId)
    {
        if (activeTouches.ContainsKey(touchId))
        {
            Debug.Log($"Touch {touchId} ended on {activeTouches[touchId].name}");
            activeTouches.Remove(touchId);
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
