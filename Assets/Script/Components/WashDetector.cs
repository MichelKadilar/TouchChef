using Script.Interface;
using UnityEngine;

public class WashDetector : MonoBehaviour
{
    [Header("Wash Configuration")]
    [SerializeField] private LayerMask ingredientLayer; // Layer for ingredients
    [SerializeField] private float raycastDistance = 100f; // Max raycast distance

    [Header("Debug")]
    [SerializeField] private bool debugMode = true; // Toggle debug messages

    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("WashDetector: Main Camera not found!");
        }
    }

    private void Update()
    {
        DetectWashCommands();
    }

    private void DetectWashCommands()
    {
        if (Input.GetKeyDown(KeyCode.O)) // Start washing
        {
            Debug.Log("Key O: Start washing");
            PerformWashCommand(washAction: true);
        }

        if (Input.GetKeyDown(KeyCode.C)) // Stop washing
        {
            Debug.Log("Key C: Stop washing");
            PerformWashCommand(washAction: false);
        }
    }

    private void PerformWashCommand(bool washAction)
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        // Perform a raycast to find the object under the cursor
        if (Physics.Raycast(ray, out hit, raycastDistance, ingredientLayer))
        {
            GameObject clickedObject = hit.collider.gameObject;

            BaseIngredient ingredient = clickedObject.GetComponent<BaseIngredient>();
            if (ingredient != null && ingredient is IWashable washableIngredient && ingredient.GetCurrentWorkStation().GetStationType() == ProcessType.Wash)
            {
                if (washAction)
                {
                    washableIngredient.StartWash();
                    if (debugMode) Debug.Log($"Started washing: {clickedObject.name}");
                }
                else
                {
                    washableIngredient.StopWash();
                    if (debugMode) Debug.Log($"Stopped washing: {clickedObject.name}");
                }
            }
            else
            {
                if (debugMode) Debug.Log($"Object clicked is not washable or not on a washing station: {clickedObject.name}");
            }
        }
    }
}
