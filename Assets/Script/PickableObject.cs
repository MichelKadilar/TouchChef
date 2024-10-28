using UnityEngine;
public interface IPickable
{
    void OnTouchPick(int touchId);
    void OnTouchMove(int touchId, Vector3 position);
    void OnTouchDrop(int touchId, Vector2 screenPosition);
}
public class PickableObject : MonoBehaviour, IPickable
{
    public int? CurrentTouchId { get; private set; }
    protected Camera mainCamera;
    private WorkStation currentWorkStation = null;
    private bool isBeingDragged = false;
    private Vector3 originalPosition; // Garde en mémoire la position originale en cas d'échec du déplacement

    protected virtual void Start()
    {
        mainCamera = Camera.main;
    }

    public virtual void OnTouchPick(int touchId)
    {
        if (!CurrentTouchId.HasValue && !isBeingDragged)
        {
            // Sauvegarde de la position actuelle
            originalPosition = transform.position;
            
            // Si l'objet est sur une workstation, on la libère temporairement
            if (currentWorkStation != null)
            {
                Debug.Log($"Picking up {gameObject.name} from workstation {currentWorkStation.name}");
                currentWorkStation.RemoveIngredient();
            }

            CurrentTouchId = touchId;
            isBeingDragged = true;
            Debug.Log($"Object picked up by touch {touchId}: {gameObject.name}");
        }
    }

    public virtual void OnTouchMove(int touchId, Vector3 position)
    {
        if (CurrentTouchId == touchId && isBeingDragged)
        {
            transform.position = position;
        }
    }

    public virtual void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        if (CurrentTouchId == touchId && isBeingDragged)
        {
            if (!TryDropObject(screenPosition))
            {
                // Si le drop a échoué et qu'on venait d'une workstation, on retourne à la position originale
                if (currentWorkStation != null)
                {
                    Debug.Log($"Drop failed, returning {gameObject.name} to original position");
                    transform.position = originalPosition;
                    currentWorkStation.TryPlaceIngredient(gameObject);
                }
            }

            isBeingDragged = false;
            CurrentTouchId = null;
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
                    newStation.TryPlaceIngredient(gameObject);
                    return true;
                }
                
                // Si on essaie de placer sur une nouvelle workstation
                if (!newStation.HasIngredient() && newStation.TryPlaceIngredient(gameObject))
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
            return false;
        }
        
        // Si on drop en dehors d'une workstation et qu'on ne venait pas d'une workstation
        Debug.Log("Dropped outside workstation, destroying object");
        Destroy(gameObject);
        return true;
    }

    // Méthode helper pour définir la workstation actuelle (utile pour l'initialisation)
    public void SetCurrentWorkStation(WorkStation station)
    {
        currentWorkStation = station;
    }
}