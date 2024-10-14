using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public Transform ingredientPosition; // Position par défaut où l'ingrédient sera placé
    private bool isOccupied = false;

    public bool TryPlaceIngredient(GameObject ingredient)
    {
        if (!isOccupied)
        {
            Debug.Log("Placing ingredient: " + ingredient.name + " on station: " + gameObject.name);
            ingredient.transform.position = ingredientPosition.position;
            ingredient.transform.rotation = ingredientPosition.rotation;
            isOccupied = true;
            return true;
        }
        else
        {
            Debug.LogWarning("Station is already occupied: " + gameObject.name);
            return false;
        }
    }

    public void RemoveIngredient()
    {
        Debug.Log("Ingredient removed from station: " + gameObject.name);
        isOccupied = false;
    }
}