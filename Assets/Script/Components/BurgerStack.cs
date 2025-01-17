using System.Collections.Generic;
using UnityEngine;

public class BurgerStack
{
    private readonly Transform stackRoot;
    private readonly List<BaseIngredient> ingredients;
    private const float INGREDIENT_SPACING = 0.1f;

    public BurgerStack(Transform root)
    {
        stackRoot = root;
        ingredients = new List<BaseIngredient>();
    }

    public void InitializeWithBottomBun(Bread bottomBun)
    {
        Clear();
        bottomBun.transform.SetParent(stackRoot);
        bottomBun.transform.rotation = Quaternion.Euler(0f, 180f, 0f);
        
        Vector3 startPos = stackRoot.position;
        bottomBun.transform.position = startPos;
        ingredients.Add(bottomBun);

        BoxCollider collider = bottomBun.GetComponent<BoxCollider>();
        if (collider != null)
        {
            Debug.Log($"Pain posé à z={bottomBun.transform.position.z}, " +
                     $"taille={collider.bounds.size.z}, " +
                     $"centre={collider.bounds.center.z}");
        }
    }

    public bool AddIngredient(BaseIngredient ingredient)
    {
        if (ingredients.Count == 0)
        {
            Debug.LogWarning("Impossible d'ajouter un ingrédient sans le pain du bas");
            return false;
        }

        ingredient.transform.SetParent(stackRoot);
        ingredient.transform.rotation = Quaternion.identity;

        BaseIngredient lastIngredient = ingredients[ingredients.Count - 1];

        BoxCollider newCollider = ingredient.GetComponent<BoxCollider>();
        BoxCollider lastCollider = lastIngredient.GetComponent<BoxCollider>();

        if (newCollider == null || lastCollider == null)
        {
            Debug.LogError($"Colliders manquants! {ingredient.name}: {newCollider == null}, {lastIngredient.name}: {lastCollider == null}");
            return false;
        }

        // Position de base (garder x et y de la station)
        Vector3 newPosition = stackRoot.position;
        newPosition.x = lastIngredient.transform.position.x;
        newPosition.y = lastIngredient.transform.position.y;

        // Calculer le point le plus haut du dernier ingrédient
        float lastIngredientHeight = lastCollider.bounds.size.z;
        float lastIngredientTopPoint = lastCollider.bounds.center.z - (lastIngredientHeight / 2f); // Inversé le signe ici

        // Calculer la position pour le nouvel ingrédient
        float newIngredientHeight = newCollider.bounds.size.z;
        float halfNewHeight = newIngredientHeight / 2f;

        // Monter en Z (donc soustraire car -Z est vers le haut dans Unity 2D)
        newPosition.z = lastIngredientTopPoint - INGREDIENT_SPACING - halfNewHeight;

        ingredient.transform.position = newPosition;

        Debug.Log($"Empilage de {ingredient.name}:\n" +
                 $"Sur: {lastIngredient.name}\n" +
                 $"Position de base Z: {lastIngredient.transform.position.z}\n" +
                 $"Point le plus haut: {lastIngredientTopPoint}\n" +
                 $"Nouvelle position Z: {newPosition.z}\n" +
                 $"Espacement: {INGREDIENT_SPACING}");

        ingredients.Add(ingredient);
        return true;
    }

    public bool FinalizeBurger(Bread topBun)
    {
        if (ingredients.Count == 0)
        {
            Debug.LogWarning("Impossible de finaliser le burger sans ingrédients");
            return false;
        }

        return AddIngredient(topBun);
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
    }
}