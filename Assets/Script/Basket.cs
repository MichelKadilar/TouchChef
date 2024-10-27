using UnityEngine;

public class Basket : MonoBehaviour
{
    public GameObject vegetablePrefab;
    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnTouchDown(int touchId, Vector2 position)
    {
        Debug.Log($"Basket touché avec touchId: {touchId} à la position écran: {position}");

        Vector3 spawnPosition = transform.position + new Vector3(0, 0, -10);
        GameObject vegetable = Instantiate(vegetablePrefab, spawnPosition, Quaternion.identity);

        if (vegetable.TryGetComponent<IPickable>(out var pickable))
        {
            Debug.Log($"Objet créé: {vegetable.name} et associé à touchId: {touchId}");
            pickable.OnTouchPick(touchId);
        }
        else
        {
            Debug.LogWarning("Le prefab vegetable ne contient pas de composant IPickable.");
        }
    }
}