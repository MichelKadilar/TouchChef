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
        // On génère une nouvelle instance du légume à la position du panier
        GameObject vegetable = Instantiate(vegetablePrefab, transform.position, Quaternion.identity);
        IPickable pickable = vegetable.GetComponent<IPickable>();

        if (pickable != null)
        {
            pickable.OnPick();
        }
    }
}