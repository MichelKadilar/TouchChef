using UnityEngine;

public class PlateStack : MonoBehaviour
{
    [SerializeField] private GameObject platePrefab;
    [SerializeField] private Transform spawnPoint;
    
    public void OnTouchDown(int touchId, Vector2 position)
    {
        if (platePrefab == null)
        {
            Debug.LogError("Plate prefab not assigned!");
            return;
        }
        
        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
            position.x, 
            position.y, 
            Mathf.Abs(Camera.main.transform.position.z + 10)
        ));
        
        GameObject plate = Instantiate(platePrefab, worldPosition, Quaternion.identity);
        
        if (plate.TryGetComponent<IPickable>(out var pickable))
        {
            pickable.OnTouchPick(touchId);
        }
        else
        {
            Debug.LogError("Created plate does not have IPickable component!");
            Destroy(plate);
        }
    }
}