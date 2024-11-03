using UnityEngine;

public class Plate : BaseContainer
{
    [SerializeField] private int maxIngredientsInPlate = 4;

    protected override void Awake()
    {
        base.Awake();
        maxIngredients = maxIngredientsInPlate;
    }

    public override bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        return base.CanAcceptIngredient(ingredient) && IsIngredientReady(ingredient);
    }

    private bool IsIngredientReady(BaseIngredient ingredient)
    {
        return !ingredient.CanProcess(ProcessType.Cook) && 
               !ingredient.CanProcess(ProcessType.Cut) && 
               !ingredient.CanProcess(ProcessType.Wash);
    }
}