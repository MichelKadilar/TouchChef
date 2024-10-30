using UnityEngine;

public class PickableObject : MonoBehaviour, IPickable
{
    protected Camera mainCamera;
    private WorkStation currentWorkStation = null;
    
    // Implémentation des propriétés de IPickable
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
            // Sauvegarde de la position actuelle
            OriginalPosition = transform.position;
            
            // Si l'objet est sur une workstation, on la libère temporairement
            if (currentWorkStation != null)
            {
                Debug.Log($"Picking up {gameObject.name} from workstation {currentWorkStation.name}");
                currentWorkStation.RemoveIngredient();
            }

            CurrentTouchId = touchId;
            IsBeingDragged = true;
            Debug.Log($"Object picked up by touch {touchId}: {gameObject.name}");
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
        if (currentWorkStation != null)
        {
            Debug.Log($"Pick failed, returning {gameObject.name} to original position");
            transform.position = OriginalPosition;
            if (!currentWorkStation.TryPlaceIngredient(gameObject))
            {
                // Si même le retour à la workstation d'origine échoue, on détruit l'objet
                Debug.Log($"Failed to return to original workstation, destroying {gameObject.name}");
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.Log($"Pick failed and no workstation, destroying {gameObject.name}");
            Destroy(gameObject);
        }
    }

    protected virtual bool TryDropObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            WorkStation newStation = hit.collider.GetComponent<WorkStation>();
            if (newStation != null)
            {
                // Si on essaie de placer sur la même workstation
                if (newStation == currentWorkStation)
                {
                    Debug.Log("Dropping back on the same workstation");
                    return newStation.TryPlaceIngredient(gameObject);
                }
                
                // Si on essaie de placer sur une nouvelle workstation
                if (newStation.TryPlaceIngredient(gameObject))
                {
                    Debug.Log($"Successfully moved to new workstation: {newStation.name}");
                    currentWorkStation = newStation;
                    return true;
                }
                else
                {
                    Debug.Log("New workstation is occupied or placement failed");
                    return false;
                }
            }
        }

        // Si on drop en dehors d'une workstation et qu'on venait d'une workstation
        if (currentWorkStation != null)
        {
            Debug.Log("Dropped outside workstation, returning to original position");
            transform.position = OriginalPosition;
            currentWorkStation.TryPlaceIngredient(gameObject);
            return true;
        }
        
        // Si on drop en dehors d'une workstation et qu'on ne venait pas d'une workstation
        Debug.Log("Dropped outside workstation, destroying object");
        Destroy(gameObject); // Assurez-vous que cette ligne s'exécute
        return true;
    }

    public void SetCurrentWorkStation(WorkStation station)
    {
        currentWorkStation = station;
    }

    public WorkStation GetCurrentWorkStation()
    {
        return currentWorkStation;
    }
    
}