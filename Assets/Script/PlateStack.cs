using UnityEngine;
public class PlateStack : MonoBehaviour
{
    [SerializeField] private GameObject platePrefab;
    
    public void OnTouchDown(int touchId, Vector2 position)
    {
        Debug.Log($"PlateStack.OnTouchDown called on {gameObject.name}");
        if (platePrefab == null)
        {
            Debug.LogError($"PlateStack {gameObject.name}: platePrefab not assigned!");
            return;
        }

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(
            position.x, 
            position.y, 
            Mathf.Abs(Camera.main.transform.position.z + 10)
        ));

        worldPosition.z = -10f;

        GameObject plate = Instantiate(platePrefab, worldPosition, Quaternion.identity);
        var pickable = plate.GetComponent<IPickable>();
        
        if (pickable != null)
        {
            pickable.OnTouchPick(touchId);
            Debug.Log($"Initialized drag on new plate with touchId: {touchId}");
        }
        else
        {
            Debug.LogError($"Created plate does not have IPickable component!");
            Destroy(plate);
        }
    }
}