using UnityEngine;
using UnityEngine.Events;

public class WorkStation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ProcessType stationType;
    [SerializeField] private Transform ingredientPosition;
    [SerializeField] private float processRadius = 1f;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject availableVisual; // Indicateur visuel quand la station est libre
    [SerializeField] private GameObject processingVisual; // Indicateur visuel pendant le processing
    [SerializeField] private GameObject invalidPlacementVisual; // Feedback visuel quand placement impossible
    
    [Header("Events")]
    public UnityEvent<float> OnProcessingProgress = new UnityEvent<float>();
    public UnityEvent OnProcessingComplete = new UnityEvent();
    public UnityEvent OnIngredientPlaced = new UnityEvent();
    public UnityEvent OnIngredientRemoved = new UnityEvent();

    private bool isOccupied = false;
    private GameObject currentIngredient = null;
    private BaseIngredient currentProcessable = null;

    private void Start()
    {
        UpdateVisuals();
    }

    public bool TryPlaceIngredient(GameObject ingredient)
    {
        // Si déjà occupé et pas le même ingrédient, refuser
        if (isOccupied && currentIngredient != ingredient)
        {
            ShowInvalidPlacement();
            return false;
        }

        // Vérifier si l'ingrédient peut être processé
        var processable = ingredient.GetComponent<BaseIngredient>();
        if (processable == null || !processable.CanProcess(stationType))
        {
            Debug.Log($"Ingredient {ingredient.name} cannot be processed here - Type: {stationType}");
            ShowInvalidPlacement();
            return false;
        }

        // Placer l'ingrédient
        ingredient.transform.position = ingredientPosition.position;
        ingredient.transform.rotation = ingredientPosition.rotation;

        // Si c'est un nouvel ingrédient
        if (currentIngredient != ingredient)
        {
            currentIngredient = ingredient;
            currentProcessable = processable;
            isOccupied = true;
            
            // Mettre à jour la référence de la workstation dans l'ingrédient
            var pickable = ingredient.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.SetCurrentWorkStation(this);
            }

            OnIngredientPlaced?.Invoke();
        }

        UpdateVisuals();
        return true;
    }

    public void RemoveIngredient()
    {
        if (!isOccupied) return;

        isOccupied = false;
        currentIngredient = null;
        currentProcessable = null;
        
        OnIngredientRemoved?.Invoke();
        UpdateVisuals();
    }

    public void StartProcessing()
    {
        if (currentProcessable != null && currentProcessable.CanProcess(stationType))
        {
            Debug.Log($"Starting processing of {currentIngredient.name} - Type: {stationType}");
            
            // S'abonner aux événements de progression
            currentProcessable.Process(stationType);
            ShowProcessingVisual();
        }
        else
        {
            Debug.Log($"Cannot process {currentIngredient?.name} - Type: {stationType}");
            ShowInvalidPlacement();
        }
    }

    public bool HasIngredient()
    {
        return isOccupied && currentIngredient != null;
    }

    public bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        return !isOccupied || currentIngredient == ingredient.gameObject;
    }

    public ProcessType GetStationType()
    {
        return stationType;
    }

    private void ShowInvalidPlacement()
    {
        if (invalidPlacementVisual != null)
        {
            StartCoroutine(ShowTemporaryVisual(invalidPlacementVisual, 0.5f));
        }
    }

    private void ShowProcessingVisual()
    {
        if (processingVisual != null)
        {
            processingVisual.SetActive(true);
            availableVisual?.SetActive(false);
        }
    }

    private void UpdateVisuals()
    {
        if (availableVisual != null)
        {
            availableVisual.SetActive(!isOccupied);
        }
        if (processingVisual != null)
        {
            processingVisual.SetActive(false);
        }
    }

    private System.Collections.IEnumerator ShowTemporaryVisual(GameObject visual, float duration)
    {
        visual.SetActive(true);
        yield return new WaitForSeconds(duration);
        visual.SetActive(false);
    }

    // Gizmos pour visualiser la zone d'interaction dans l'éditeur
    private void OnDrawGizmos()
    {
        if (ingredientPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ingredientPosition.position, processRadius);
        }
    }
}