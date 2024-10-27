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
    public bool isReadyToBeClickable = false;

    protected virtual void Start()
    {
        mainCamera = Camera.main;
    }

    public virtual void OnTouchPick(int touchId)
    {
        if (!CurrentTouchId.HasValue)
        {
            CurrentTouchId = touchId;
            Debug.Log($"Object picked up by touch {touchId}: {gameObject.name}");
        }
    }

    public virtual void OnTouchMove(int touchId, Vector3 position)
    {
        if (CurrentTouchId == touchId)
        {
            transform.position = position;
        }
    }

    public virtual void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        if (CurrentTouchId == touchId)
        {
            TryDropObject(screenPosition);
            CurrentTouchId = null;
        }
    }

    protected virtual void TryDropObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            WorkStation station = hit.collider.GetComponent<WorkStation>();
            if (station != null && station.TryPlaceIngredient(gameObject))
            {
                Debug.Log("Ingredient placed successfully on the station.");
                isReadyToBeClickable = true;
                return;
            }
        }

        Debug.Log("No valid drop location found. Destroying object.");
        Destroy(gameObject);
    }
}