using UnityEngine;
using UnityEngine.Events;

public class WorkStation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ProcessType stationType;
    [SerializeField] private Transform ingredientPosition;
    [SerializeField] private float processRadius = 1f;
    
    [Header("Cooking Station Settings")]
    [SerializeField] private bool isCookingStation;
    [SerializeField] private GameObject panPrefab;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject availableVisual;
    [SerializeField] private GameObject processingVisual;
    [SerializeField] private GameObject invalidPlacementVisual;
    
    [Header("Events")]
    public UnityEvent<float> OnProcessingProgress = new UnityEvent<float>();
    public UnityEvent OnProcessingComplete = new UnityEvent();
    public UnityEvent OnIngredientPlaced = new UnityEvent();
    public UnityEvent OnIngredientRemoved = new UnityEvent();

    private bool isOccupied = false;
    private GameObject currentIngredient = null;
    private BaseIngredient currentProcessable = null;
    private bool isProcessing = false;
    private Pan attachedPan = null;

    private void Start()
    {
        if (isCookingStation && panPrefab != null)
        {
            InitializeCookingPan();
        }
        UpdateVisuals();
    }

    private void InitializeCookingPan()
    {
        GameObject panObject = Instantiate(panPrefab, ingredientPosition.position, ingredientPosition.rotation);
        attachedPan = panObject.GetComponent<Pan>();
        
        if (attachedPan != null)
        {
            // Désactiver le HoldDetector au début puisque la poêle est attachée
            var holdDetector = panObject.GetComponent<HoldDetector>();
            if (holdDetector != null)
            {
                holdDetector.enabled = false;
            }
            
            // Configurer la poêle comme l'ingrédient actuel
            currentIngredient = panObject;
            isOccupied = true;
        }
        else
        {
            Debug.LogError($"Pan prefab does not have Pan component!");
            Destroy(panObject);
        }
    }

    public bool TryPlaceIngredient(GameObject ingredient)
    {
        Debug.Log($"Attempting to place {ingredient.name} on workstation {gameObject.name} of type {stationType}");

        // Si c'est une station de cuisson avec une poêle
        if (isCookingStation && attachedPan != null)
        {
            var baseIngredient = ingredient.GetComponent<BaseIngredient>();
            if (baseIngredient != null)
            {
                if (attachedPan.CanAcceptIngredient(baseIngredient))
                {
                    bool success = attachedPan.AddIngredient(baseIngredient);
                    if (success)
                    {
                        OnIngredientPlaced?.Invoke();
                    }
                    return success;
                }
                ShowInvalidPlacement();
                return false;
            }
        }

        // Comportement standard pour les autres stations
        if (isOccupied && currentIngredient != ingredient)
        {
            Debug.Log($"Workstation {gameObject.name} is occupied by different ingredient");
            ShowInvalidPlacement();
            return false;
        }

        var processable = ingredient.GetComponent<BaseIngredient>();
        if (processable != null)
        {
            if (!processable.CanProcess(stationType))
            {
                Debug.Log($"Ingredient {ingredient.name} cannot be processed here - Type: {stationType}");
                ShowInvalidPlacement();
                return false;
            }
        }
        else
        {
            Debug.Log($"Ingredient {ingredient.name} does not have BaseIngredient component");
            ShowInvalidPlacement();
            return false;
        }

        ingredient.transform.position = ingredientPosition.position;
        ingredient.transform.rotation = ingredientPosition.rotation;

        if (currentIngredient != ingredient)
        {
            currentIngredient = ingredient;
            currentProcessable = processable;
            isOccupied = true;
        
            var pickable = ingredient.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.SetCurrentWorkStation(this);
            }

            OnIngredientPlaced?.Invoke();
            Debug.Log($"Successfully placed {ingredient.name} on workstation {gameObject.name}");
        }

        UpdateVisuals();
        return true;
    }

    protected virtual void CompleteProcessing()
    {
        isProcessing = false;
        UpdateVisuals();
    }

    public void RemoveIngredient()
    {
        if (!isOccupied) return;

        if (isCookingStation && attachedPan != null)
        {
            // Ne pas effacer la poêle, juste son contenu
            attachedPan.GetContents().ForEach(ingredient => attachedPan.RemoveIngredient(ingredient));
        }
        else
        {
            isOccupied = false;
            currentIngredient = null;
            currentProcessable = null;
        }
        
        OnIngredientRemoved?.Invoke();
        UpdateVisuals();
    }

    public void StartProcessing()
    {
        if (isProcessing || !HasIngredient()) return;

        isProcessing = true;
    
        if (isCookingStation && attachedPan != null)
        {
            Debug.Log($"Starting cooking process with pan on {gameObject.name}");
            attachedPan.StartCooking();
            ShowProcessingVisual();
            return;
        }

        if (currentProcessable != null && currentProcessable.CanStartProcessing(stationType))
        {
            Debug.Log($"Starting processing of {currentIngredient.name} - Type: {stationType}");
            currentProcessable.Process(stationType);
            ShowProcessingVisual();
        }
    }

    public bool HasIngredient()
    {
        if (isCookingStation && attachedPan != null)
        {
            return attachedPan.GetContents().Count > 0;
        }
        return isOccupied && currentIngredient != null;
    }

    public bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        if (isCookingStation && attachedPan != null)
        {
            return attachedPan.CanAcceptIngredient(ingredient);
        }
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
        // Pour les stations de cuisson, on considère l'occupation différemment
        bool showAvailable = isCookingStation && attachedPan != null ? 
            attachedPan.GetContents().Count == 0 : !isOccupied;

        if (availableVisual != null)
        {
            availableVisual.SetActive(showAvailable);
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

    private void OnDrawGizmos()
    {
        if (ingredientPosition != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(ingredientPosition.position, processRadius);
        }
    }
}