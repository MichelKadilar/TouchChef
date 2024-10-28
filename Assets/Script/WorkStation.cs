using UnityEngine;

public class WorkStation : MonoBehaviour
{
    public Transform ingredientPosition;
    private bool isOccupied = false;
    private GameObject currentIngredient = null;

    public bool TryPlaceIngredient(GameObject ingredient)
    {
        if (!isOccupied || currentIngredient == ingredient)
        {
            Debug.Log($"Placing ingredient {ingredient.name} on station {gameObject.name}");
            
            ingredient.transform.position = ingredientPosition.position;
            ingredient.transform.rotation = ingredientPosition.rotation;
            
            currentIngredient = ingredient;
            isOccupied = true;
            
            var pickable = ingredient.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.SetCurrentWorkStation(this);
            }
            
            return true;
        }
        
        Debug.Log($"Cannot place {ingredient.name} on station {gameObject.name} - already occupied by {currentIngredient?.name}");
        return false;
    }

    public void RemoveIngredient()
    {
        Debug.Log($"Removing ingredient from station {gameObject.name}");
        isOccupied = false;
        currentIngredient = null;
    }

    public bool HasIngredient()
    {
        return isOccupied && currentIngredient != null;
    }
}