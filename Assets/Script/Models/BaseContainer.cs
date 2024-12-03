using System.Collections.Generic;
using UnityEngine;

public abstract class BaseContainer : PickableObject, IContainer
{
    [Header("Container Settings")]
    [SerializeField] protected Transform ingredientAttachPoint;
    [SerializeField] protected int maxIngredients = 1;

    protected List<BaseIngredient> contents = new List<BaseIngredient>();
    protected HoldDetector holdDetector;

    private DraggingManager draggingManager;
    private int? currentTouchId;
    private Vector3 originalPosition;

    protected void Awake()
    {
        base.Awake();
        InitializeDraggingManager();
        InitializeHoldDetector();
    }

    private void InitializeDraggingManager()
    {
        draggingManager = FindObjectOfType<DraggingManager>();
        if (draggingManager == null)
        {
            Debug.LogError("DraggingManager not found in scene! Ensure one exists for managing dragging states.");
        }
    }

    private void InitializeHoldDetector()
    {
        holdDetector = GetComponent<HoldDetector>() ?? gameObject.AddComponent<HoldDetector>();
        holdDetector.OnHoldComplete.RemoveListener(OnHoldCompleted);
        holdDetector.OnHoldComplete.AddListener(OnHoldCompleted);
    }

    private void OnHoldCompleted(int touchId)
    {
        if (draggingManager != null && draggingManager.IsBeingDragged(this))
        {
            Debug.Log($"{gameObject.name} is already being dragged.");
            return;
        }

        OnTouchPick(touchId);
    }

    public virtual bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        return contents.Count < maxIngredients;
    }

    public virtual bool AddIngredient(BaseIngredient ingredient)
    {
        if (!CanAcceptIngredient(ingredient))
        {
            Debug.Log($"{gameObject.name} cannot accept more ingredients.");
            return false;
        }

        contents.Add(ingredient);
        AttachIngredient(ingredient);
        Debug.Log($"{ingredient.name} added to {gameObject.name}. Total contents: {contents.Count}");
        return true;
    }

    public virtual bool RemoveIngredient(BaseIngredient ingredient)
    {
        if (!contents.Contains(ingredient))
        {
            Debug.Log($"{ingredient.name} is not in {gameObject.name}.");
            return false;
        }

        contents.Remove(ingredient);
        DetachIngredient(ingredient);
        Debug.Log($"{ingredient.name} removed from {gameObject.name}. Remaining contents: {contents.Count}");
        return true;
    }

    private void AttachIngredient(BaseIngredient ingredient)
    {
        if (ingredientAttachPoint == null)
        {
            Debug.LogError($"Attach point missing for {gameObject.name}!");
            return;
        }

        ingredient.transform.SetParent(ingredientAttachPoint);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localRotation = Quaternion.identity;
    }

    private void DetachIngredient(BaseIngredient ingredient)
    {
        ingredient.transform.SetParent(null);
    }

    public List<BaseIngredient> GetContents()
    {
        return new List<BaseIngredient>(contents);
    }

    public Transform GetIngredientAttachPoint()
    {
        return ingredientAttachPoint;
    }

    public int? CurrentTouchId => currentTouchId;
    public bool IsBeingDragged => draggingManager?.IsBeingDragged(this) ?? false;
    public Vector3 OriginalPosition => originalPosition;

    public void OnTouchPick(int touchId)
    {
        if (holdDetector != null && !holdDetector.IsHolding)
        {
            Debug.Log($"{gameObject.name} is not ready to be picked.");
            return;
        }

        if (draggingManager != null && draggingManager.IsBeingDragged(this))
        {
            Debug.Log($"{gameObject.name} is already being dragged.");
            return;
        }

        currentTouchId = touchId;
        originalPosition = transform.position;

        base.OnTouchPick(touchId);
        draggingManager?.StartDragging(this);
    }

    public void OnTouchMove(int touchId, Vector3 position)
    {
        if (currentTouchId != touchId)
        {
            Debug.LogWarning($"{gameObject.name} received move from a different touch ID.");
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

        draggingManager?.StopDragging(this);
        currentTouchId = null;
    }

    public virtual void OnPickFailed()
    {
        Debug.Log($"{gameObject.name} drop failed, returning to original position.");
        transform.position = originalPosition;
        draggingManager?.StopDragging(this);
    }

    protected virtual bool TryDropObject(Vector2 screenPosition)
    {
        if (mainCamera == null) return false;

        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            WorkStation workstation = hit.collider.GetComponent<WorkStation>();
            if (workstation != null)
            {
                Debug.Log($"Attempting to place {gameObject.name} on workstation: {workstation.name}");
                if (workstation.TryPlaceIngredient(gameObject))
                {
                    return true;
                }
            }
        }

        Debug.Log($"{gameObject.name} could not find a valid workstation.");
        return false;
    }
}
