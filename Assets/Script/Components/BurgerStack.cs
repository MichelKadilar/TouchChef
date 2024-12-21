using System.Collections.Generic;
using UnityEngine;

public class BurgerStack
{
    private readonly Transform stackRoot;
    private readonly List<BaseIngredient> ingredients;
    private float currentStackHeight;
    private const float INGREDIENT_SPACING = 0.1f;

    public BurgerStack(Transform root)
    {
        stackRoot = root;
        ingredients = new List<BaseIngredient>();
        currentStackHeight = 0f;
    }

    public void InitializeWithBottomBun(Bread bottomBun)
    {
        Clear();
        AddToStack(bottomBun);
    }

    public bool AddIngredient(BaseIngredient ingredient)
    {
        if (ingredients.Count == 0)
        {
            return false;
        }

        AddToStack(ingredient);
        return true;
    }

    public bool FinalizeBurger(Bread topBun)
    {
        if (ingredients.Count == 0)
        {
            return false;
        }

        AddToStack(topBun);
        return true;
    }

    private void AddToStack(BaseIngredient ingredient)
    {
        ingredient.transform.SetParent(stackRoot);
            
        Vector3 newPosition = stackRoot.position;
        newPosition.y += currentStackHeight;
        ingredient.transform.position = newPosition;
            
        ingredients.Add(ingredient);
        currentStackHeight += INGREDIENT_SPACING;
    }

    public void Clear()
    {
        foreach (var ingredient in ingredients)
        {
            if (ingredient != null && ingredient.gameObject != null)
            {
                ingredient.transform.SetParent(null);
            }
        }
        ingredients.Clear();
        currentStackHeight = 0f;
    }
}