using System.Collections.Generic;
using UnityEngine;

public class RecipeManager : MonoBehaviour
{
    private List<Recipe> availableRecipes;
    private Recipe currentRecipe;
    
    public event System.Action<Recipe> OnRecipeCompleted;
    public event System.Action<Recipe> OnRecipeFailed;
    
    public bool ValidateRecipe(List<BaseIngredient> ingredients)
    {
        //TODO
        return true;
    }
}