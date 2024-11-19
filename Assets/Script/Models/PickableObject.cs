using UnityEngine;

public class PickableObject : MonoBehaviour, IPickable
{
    protected Camera mainCamera;
    private WorkStation currentWorkStation = null;
    private WorkStation previousWorkStation = null;
    private bool isFromBasket = true;
    
    public int? CurrentTouchId { get; protected set; }
    public bool IsBeingDragged { get; protected set; }
    public Vector3 OriginalPosition { get; protected set; }

    protected virtual void Awake()
    {
        mainCamera = Camera.main;
    }

    public virtual void OnTouchPick(int touchId)
    {
        if (!CurrentTouchId.HasValue && !IsBeingDragged)
        {
            OriginalPosition = transform.position;
            
            if (currentWorkStation != null)
            {
                Debug.Log($"Picking up {gameObject.name} from workstation {currentWorkStation.name}");
                previousWorkStation = currentWorkStation;
                currentWorkStation.RemoveIngredient();
                isFromBasket = false;
            }

            CurrentTouchId = touchId;
            IsBeingDragged = true;
            Debug.Log($"Object picked up by touch {touchId}: {gameObject.name}, IsFromBasket: {isFromBasket}");
        }
    }
    public virtual void OnTouchMove(int touchId, Vector3 position)
    {
        if (CurrentTouchId == touchId && IsBeingDragged)
        {
            transform.position = position;
        }
    }

    public virtual void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        if (CurrentTouchId == touchId && IsBeingDragged)
        {
            bool dropSuccessful = TryDropObject(screenPosition);
            
            if (!dropSuccessful)
            {
                OnPickFailed();
            }

            IsBeingDragged = false;
            CurrentTouchId = null;
        }
    }

    public virtual void OnPickFailed()
    {
        if (!isFromBasket && previousWorkStation != null)
        {
            Debug.Log($"Pick failed, returning {gameObject.name} to previous workstation");
            transform.position = OriginalPosition;
            if (previousWorkStation.TryPlaceIngredient(gameObject))
            {
                currentWorkStation = previousWorkStation;
                previousWorkStation = null;
                return;
            }
        }
        
        Debug.Log($"Pick failed and from basket or no valid workstation, destroying {gameObject.name}");
        Destroy(gameObject);
    }
    

    protected virtual bool TryDropObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        var ownCollider = GetComponent<Collider>();
        if (ownCollider != null)
        {
            ownCollider.enabled = false;
        }
        
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);
        
        if (ownCollider != null)
        {
            ownCollider.enabled = true;
        }

        if (IsOverTrash(hits))
        {
            Debug.Log($"Dropping {gameObject.name} in trash - destroying object");
            Destroy(gameObject);
            return true;
        }

        bool foundWorkstation = false;
        foreach (var hit in hits)
        {
            if (hit.collider.gameObject == gameObject) continue;
            WorkStation newStation = hit.collider.GetComponent<WorkStation>();
            if (newStation != null)
            {
                foundWorkstation = true;
                Debug.Log($"Attempting to place on workstation: {newStation.name}");
                if (!isFromBasket && newStation == previousWorkStation)
                {
                    Debug.Log($"Dropping back on previous workstation: {newStation.name}");
                    bool success = newStation.TryPlaceIngredient(gameObject);
                    if (success) 
                    {
                        currentWorkStation = newStation;
                        isFromBasket = false;
                    }
                    return success;
                }
                
                if (newStation.TryPlaceIngredient(gameObject))
                {
                    Debug.Log($"Successfully moved to new workstation: {newStation.name}");
                    currentWorkStation = newStation;
                    previousWorkStation = null;
                    isFromBasket = false;
                    return true;
                }
            }
        }

        if (!foundWorkstation)
        {
            Debug.Log($"No workstation found during drop for {gameObject.name}");
        }
        
        if (!isFromBasket && previousWorkStation != null)
        {
            Debug.Log($"Dropped outside, returning to previous workstation: {previousWorkStation.name}");
            transform.position = OriginalPosition;
            bool success = previousWorkStation.TryPlaceIngredient(gameObject);
            if (success)
            {
                currentWorkStation = previousWorkStation;
                return true;
            }
        }
        
        Debug.Log($"Object is from basket or has no previous workstation, destroying {gameObject.name}");
        Destroy(gameObject);
        return true;
    }

    private bool IsOverTrash(RaycastHit[] hits)
    {
        foreach (var hit in hits)
        {
            if (hit.collider.CompareTag("Trash"))
            {
                return true;
            }
        }
        return false;
    }

    public void SetCurrentWorkStation(WorkStation station)
    {
        currentWorkStation = station;
        isFromBasket = false;
    }

    public WorkStation GetCurrentWorkStation()
    {
        return currentWorkStation;
    }
    
}