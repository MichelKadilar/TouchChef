using Script.Conveyor;
using UnityEngine;

public class PickableObject : MonoBehaviour , IPickable
{
    protected Camera mainCamera;
    private WorkStation currentWorkStation = null;
    private WorkStation previousWorkStation = null;
    private ConveyorSystem conveyorSystem = null;
    private bool isFromBasket = true;
    private bool isFromConveyor = false;

    private Vector3 originalPosition;

    protected void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found! Picking and dropping functionality will fail.");
        }
    }

    public int? CurrentTouchId { get; }
    public bool IsBeingDragged { get; }
    public Vector3 OriginalPosition { get; }
    public void OnTouchPick(int touchId)
    {
        // Save the original position
        originalPosition = transform.position;

        // Detach from current workstation, if any
        if (currentWorkStation != null)
        {
            Debug.Log($"Picking up {gameObject.name} from workstation {currentWorkStation.name}");
            previousWorkStation = currentWorkStation;
            currentWorkStation.RemoveIngredient();
            currentWorkStation = null;
            isFromBasket = false;
        }
        if (isFromConveyor && conveyorSystem != null)
        {
            conveyorSystem.PauseProduct(gameObject);
        }

        Debug.Log($"Object {gameObject.name} picked up by touch {touchId}, IsFromBasket: {isFromBasket}, IsFromConveyor: {isFromConveyor}");
    }
    
    public void InitializeConveyor(ConveyorSystem system)
    {
        Debug.Log("Initializing conveyor for object: " + gameObject.name);
        conveyorSystem = system;
        isFromConveyor = true;
        isFromBasket = false;
    }

    public void OnTouchMove(int touchId, Vector3 position)
    {
        if (currentWorkStation != null)
        {
            Debug.Log($"Object {gameObject.name} is currently on workstation {currentWorkStation.name}, ignoring move.");
            return;
        }

        transform.position = position;
    }

    public void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        if (!TryDropObject(screenPosition))
        {
            OnPickFailed();
        }
    }

    public void OnPickFailed()
    {
        if (!isFromBasket && previousWorkStation != null)
        {
            Debug.Log($"Pick failed, returning {gameObject.name} to previous workstation");
            transform.position = originalPosition;

            if (previousWorkStation.TryPlaceIngredient(gameObject))
            {
                currentWorkStation = previousWorkStation;
                previousWorkStation = null;
            }
            return;
        }
        
        if (isFromConveyor && conveyorSystem != null)
        {
            Debug.Log($"Pick failed, returning {gameObject.name} to conveyor");
            conveyorSystem.ReturnProductToNearestPoint(gameObject);
            return;
        }

        Debug.Log($"Pick failed and no valid workstation available, destroying {gameObject.name}");
        Destroy(gameObject);
    }

    protected bool TryDropObject(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            DeliveryZone deliveryZone = hit.collider.GetComponent<DeliveryZone>();
            if (deliveryZone != null)
            {
                // Vérifier si l'ingrédient est valide pour la livraison
                var ingredient = GetComponent<BaseIngredient>();
                if (ingredient != null)
                {
                    return deliveryZone.TryDeliverIngredient(ingredient);
                }
                return false;
            }
            // Vérifier si c'est une poubelle
            TrashCan trashCan = hit.collider.GetComponent<TrashCan>();
            if (trashCan != null && trashCan.IsOpen)
            {
                Debug.Log($"Throwing {gameObject.name} in trash can successfully");
                trashCan.PlayThrowSound();
                Destroy(gameObject);
                return true;
            }
            
            
            ConveyorSystem conveyor = hit.collider.GetComponent<ConveyorSystem>();
            if (conveyor != null)
            {
                Debug.Log($"Attempting to place {gameObject.name} on conveyor");
                if (conveyor.AddExistingProduct(gameObject))
                {
                    if (currentWorkStation != null)
                    {
                        currentWorkStation.RemoveIngredient();
                    }
                    currentWorkStation = null;
                    previousWorkStation = null;
                    isFromBasket = false;
                    isFromConveyor = true;
                    this.conveyorSystem = conveyor;
                    return true;
                }
            }
            
            WorkStation workstation = hit.collider.GetComponent<WorkStation>();
            if (workstation != null)
            {
                Debug.Log($"Attempting to place {gameObject.name} on workstation: {workstation.name}");
                if (workstation.TryPlaceIngredient(gameObject))
                {
                    currentWorkStation = workstation;
                    previousWorkStation = null;
                    isFromBasket = false;
                    return true;
                }
            }
        }

        Debug.Log($"No valid workstation or conveyor found for {gameObject.name}");
        return false;
    }

    public void SetCurrentWorkStation(WorkStation workstation)
    {
        currentWorkStation = workstation;
        isFromBasket = false;
    }

    public WorkStation GetCurrentWorkStation()
    {
        return currentWorkStation;
    }
}
