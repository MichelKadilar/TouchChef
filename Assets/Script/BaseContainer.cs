using System.Collections.Generic;
using UnityEngine;

public abstract class BaseContainer : PickableObject, IContainer
{
    [SerializeField] protected UnityEngine.Transform ingredientAttachPoint;  // Spécifié explicitement
    [SerializeField] protected int maxIngredients = 1;
    
    protected List<BaseIngredient> contents = new List<BaseIngredient>();
    protected HoldDetector holdDetector;

    protected override void Awake()
    {
        base.Awake();
        InitializeHoldDetector();
    }

    private void InitializeHoldDetector()
    {
        holdDetector = GetComponent<HoldDetector>();
        if (holdDetector == null)
        {
            holdDetector = gameObject.AddComponent<HoldDetector>();
        }
        holdDetector.OnHoldComplete.RemoveListener(OnHoldCompleted);
        holdDetector.OnHoldComplete.AddListener(OnHoldCompleted);
    }

    private void OnHoldCompleted(int touchId)
    {
        if (!IsBeingDragged)
        {
            OnTouchPick(touchId);
        }
    }

    public virtual bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        return contents.Count < maxIngredients;
    }

    public virtual bool AddIngredient(BaseIngredient ingredient)
    {
        if (!CanAcceptIngredient(ingredient)) return false;

        contents.Add(ingredient);
        ingredient.transform.SetParent(ingredientAttachPoint);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localRotation = Quaternion.identity;
        return true;
    }

    public virtual bool RemoveIngredient(BaseIngredient ingredient)
    {
        if (!contents.Contains(ingredient)) return false;

        contents.Remove(ingredient);
        ingredient.transform.SetParent(null);
        return true;
    }

    public List<BaseIngredient> GetContents()
    {
        return new List<BaseIngredient>(contents);
    }

    public UnityEngine.Transform GetIngredientAttachPoint()  // Spécifié explicitement
    {
        return ingredientAttachPoint;
    }

    public override void OnTouchMove(int touchId, Vector3 position)
    {
        base.OnTouchMove(touchId, position);
        // Les ingrédients suivent automatiquement grâce au parenting
    }
}
