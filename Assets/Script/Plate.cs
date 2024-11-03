using UnityEngine;

public class Plate : BaseContainer
{
    [SerializeField] private int maxIngredientsInPlate = 4;
    [SerializeField] private float ingredientStackOffset = 0.1f;
    
    protected override void Awake()
    {
        base.Awake();
        maxIngredients = maxIngredientsInPlate;
    }

    public override void OnTouchPick(int touchId)
    {
        Debug.Log($"Plate OnTouchPick called with ID: {touchId}");
        if (!CurrentTouchId.HasValue && !IsBeingDragged)
        {
            OriginalPosition = transform.position;
            CurrentTouchId = touchId;
            IsBeingDragged = true;
            Debug.Log($"Plate {gameObject.name} is now being dragged");
        }
    }

    public override void OnTouchMove(int touchId, Vector3 position)
    {
        if (CurrentTouchId == touchId && IsBeingDragged)
        {
            transform.position = position;
        }
    }

    public override void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        if (CurrentTouchId == touchId && IsBeingDragged)
        {
            bool dropSuccessful = TryDropObject(screenPosition);
            
            if (!dropSuccessful)
            {
                Debug.Log($"Drop failed for plate {gameObject.name}, destroying");
                Destroy(gameObject);
            }

            IsBeingDragged = false;
            CurrentTouchId = null;
        }
    }

    protected override bool TryDropObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            TableStation tableStation = hit.collider.GetComponent<TableStation>();
            if (tableStation != null && tableStation.TryPlaceIngredient(gameObject))
            {
                Debug.Log($"Successfully placed plate on table station");
                return true;
            }
        }

        return false;
    }
}