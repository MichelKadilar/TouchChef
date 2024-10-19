using UnityEngine;

public class Basket : MonoBehaviour
{
    public GameObject vegetablePrefab; // Le prefab du légume (carotte, tomate, etc.)

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    void OnMouseDown()
    {
        Vector3 spawnPosition = transform.position + new Vector3(0, 0, -10);
        // On génère une nouvelle instance du légume à la position du panier
        GameObject vegetable = Instantiate(vegetablePrefab, spawnPosition, Quaternion.identity);
        IPickable pickable = vegetable.GetComponent<IPickable>();

        if (pickable != null)
        {
            pickable.OnPick();
        }
    }
}