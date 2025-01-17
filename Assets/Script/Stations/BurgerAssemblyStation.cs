using UnityEngine;

public class BurgerAssemblyStation : WorkStation
{
    private BurgerStack burgerStack;
    private bool isReadyForIngredients;

    private void Start()
    {
        burgerStack = new BurgerStack(ingredientPosition);
        isReadyForIngredients = false;
        stationType = ProcessType.Assemble;
    }

    public override bool TryPlaceIngredient(GameObject obj)
    {
        var ingredient = obj.GetComponent<BaseIngredient>();
        
        if (ingredient == null || !ingredient.CanProcess(ProcessType.Assemble))
        {
            return false;
        }

        if (!isReadyForIngredients)
        {
            var bottomBun = ingredient as Bread;
            if (bottomBun != null && bottomBun.GetBreadType() == BreadType.Bottom)
            {
                // Ne pas définir la rotation ici, laisser BurgerStack s'en charger
                burgerStack.InitializeWithBottomBun(bottomBun);
                isReadyForIngredients = true;
                isOccupied = true;
                currentObject = obj;
                return true;
            }
            return false;
        }

        if (!ValidateIngredient(ingredient))
        {
            return false;
        }

        var topBun = ingredient as Bread;
        if (topBun != null && topBun.GetBreadType() == BreadType.Top)
        {
            bool success = burgerStack.FinalizeBurger(topBun);
            if (success)
            {
                isReadyForIngredients = false;
                isOccupied = true;
                currentObject = obj;
            }
            return success;
        }

        bool addSuccess = burgerStack.AddIngredient(ingredient);
        if (addSuccess)
        {
            isOccupied = true;
            currentObject = obj;
        }
        return addSuccess;
    }

    private bool ValidateIngredient(BaseIngredient ingredient)
    {
        if (ingredient is Lettuce lettuce)
        {
            return lettuce.CurrentState == IngredientState.Cut;
        }
        
        if (ingredient is Tomato tomato)
        {
            return tomato.CurrentState == IngredientState.Cut;
        }
        
        if (ingredient is Meat meat)
        {
            return meat.CurrentState == IngredientState.Cooked;
        }
        
        if (ingredient is Cheese cheese)
        {
            return cheese.CurrentState == IngredientState.Cut;
        }
        
        return ingredient is Bread;
    }

    protected override void UpdateVisuals()
    {
        if (availableVisual != null)
        {
            availableVisual.SetActive(!isOccupied);
        }
    }

    public new void RemoveIngredient()
    {
        base.RemoveIngredient();
        if (burgerStack != null)
        {
            burgerStack.Clear();
            isReadyForIngredients = false;
            isOccupied = false;
            UpdateVisuals();
        }
    }
}