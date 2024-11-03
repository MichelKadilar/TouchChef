using UnityEngine;

public class Pan : BaseContainer
{
    [SerializeField] private ProcessType supportedCookingType = ProcessType.Cook;
    private BaseIngredient currentIngredient;

    public ProcessType GetSupportedCookingType()
    {
        return supportedCookingType;
    }

    public override bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        return base.CanAcceptIngredient(ingredient) && ingredient.CanProcess(supportedCookingType);
    }

    public bool StartCooking()
    {
        if (contents.Count == 0) return false;
        
        var ingredient = contents[0];
        if (ingredient.CanProcess(supportedCookingType))
        {
            ingredient.Process(supportedCookingType);
            return true;
        }
        return false;
    }
}