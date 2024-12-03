using UnityEngine;

public class SliceDetector : MonoBehaviour
{
    [Header("Slice Configuration")]
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
            Debug.LogError("SliceDetector: Main Camera not found!");
        }
    }

    private void Update()
    {
        DetectSliceInput();
    }

    private void DetectSliceInput()
    {
        if (Input.GetMouseButtonDown(1)) // Right mouse button click
        {
            if (debugMode) Debug.Log("/.//////////////////////// Right mouse button clicked.");

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // Perform raycast to detect ingredients
            if (Physics.Raycast(ray, out hit, raycastDistance, ingredientLayer))
            {
                GameObject clickedObject = hit.collider.gameObject;

                // Check if the object has a BaseIngredient component
                BaseIngredient ingredient = clickedObject.GetComponent<BaseIngredient>();
                if (ingredient != null && ingredient is ISliceable sliceableIngredient && ingredient.GetCurrentWorkStation().GetStationType()==ProcessType.Cut)
                {
                    // Call the Slice method
                    if (debugMode) Debug.Log($"/.//////////////////////// Slicing ingredient: {clickedObject.name}");
                    sliceableIngredient.Slice();
                }
                else
                {
                    if (debugMode) Debug.Log($"/.//////////////////////// Object clicked is not sliceable: {clickedObject.name} or not on a cutting station");
                }
            }
        }
    }
}