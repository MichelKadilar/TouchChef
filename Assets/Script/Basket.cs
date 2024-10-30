using UnityEngine;

public class Basket : MonoBehaviour
{
    [SerializeField] private GameObject ingredientPrefab;
    [SerializeField] private IngredientType ingredientType;
    
    public void OnTouchDown(int touchId, Vector2 position)
    {
        Debug.Log($"Basket.OnTouchDown called on {gameObject.name}");
        if (ingredientPrefab == null)
        {
            Debug.LogError($"Basket {gameObject.name}: ingredientPrefab not assigned!");
            return;
        }

        // Convertir la position du touch en position monde
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
            position.x, 
            position.y, 
            Mathf.Abs(Camera.main.transform.position.z + 10)
        ));
    
        GameObject ingredient = Instantiate(ingredientPrefab, worldPosition, Quaternion.identity);
        Debug.Log($"Created ingredient: {ingredient.name} at position {worldPosition}");
    
        if (ingredient.TryGetComponent<IPickable>(out var pickable))
        {
            pickable.OnTouchPick(touchId);
        }
        else
        {
            Debug.LogError($"Created ingredient does not have IPickable component!");
            Destroy(ingredient);
        }
    }
}