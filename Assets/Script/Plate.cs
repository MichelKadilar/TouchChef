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
        // Vérifier si l'ingrédient est dans un état final (cuit, coupé, etc.)
        return base.CanAcceptIngredient(ingredient) && IsIngredientReady(ingredient);
    }

    private bool IsIngredientReady(BaseIngredient ingredient)
    {
        // L'ingrédient ne doit plus avoir de processus disponibles
        return !ingredient.CanProcess(ProcessType.Cook) && 
               !ingredient.CanProcess(ProcessType.Cut) && 
               !ingredient.CanProcess(ProcessType.Wash);
    }
}