using UnityEngine;
using UnityEngine.Events;

public class WorkStation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] private ProcessType stationType;
    [SerializeField] public Transform ingredientPosition;
    [SerializeField] private float processRadius = 1f;
    
    [Header("Cooking Station Settings")]
    [SerializeField] private bool isCookingStation;
    [SerializeField] private GameObject panPrefab;
    
    [Header("Visual Feedback")]
    [SerializeField] public GameObject availableVisual;
    [SerializeField] public GameObject processingVisual;
    [SerializeField] private GameObject invalidPlacementVisual;
    
    [Header("Events")]
    public UnityEvent<float> OnProcessingProgress = new UnityEvent<float>();
    public UnityEvent OnProcessingComplete = new UnityEvent();
    public UnityEvent OnIngredientPlaced = new UnityEvent();
    public UnityEvent OnIngredientRemoved = new UnityEvent();

    public bool isOccupied = false;
    public GameObject currentObject = null;
    public BaseIngredient currentProcessable = null;
    public IContainer currentContainer = null;
    public bool isProcessing = false;
    public Pan attachedPan = null;

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
            // Configurer la poêle comme l'ingrédient actuel
            currentObject = panObject;
            currentContainer = attachedPan;
            isOccupied = true;
            
            // Configurer la workstation pour la poêle
            var pickable = panObject.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.SetCurrentWorkStation(this);
            }
        }
        else
        {
            Debug.LogError($"Pan prefab does not have Pan component!");
            Destroy(panObject);
        }
    }

    public virtual bool TryPlaceIngredient(GameObject obj)
    {
        Debug.Log($"Attempting to place {obj.name} on workstation {gameObject.name} of type {stationType}");

        if (isOccupied && currentObject != obj)
        {
            Debug.Log($"Workstation {gameObject.name} is occupied by different object");
            ShowInvalidPlacement();
            return false;
        }

        // Vérifier si c'est un ingrédient ou un contenant
        var processable = obj.GetComponent<BaseIngredient>();
        var container = obj.GetComponent<IContainer>();

        // Si c'est une station de cuisson avec une poêle attachée
        if (isCookingStation && attachedPan != null)
        {
            if (processable != null)
            {
                if (attachedPan.CanAcceptIngredient(processable))
                {
                    bool success = attachedPan.AddIngredient(processable);
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

        // Gestion des contenants (poêles mobiles)
        if (container != null)
        {
            // Si c'est une station de cuisson, on accepte les poêles
            if (stationType == ProcessType.Cook)
            {
                obj.transform.position = ingredientPosition.position;
                obj.transform.rotation = ingredientPosition.rotation;

                currentObject = obj;
                currentContainer = container;
                isOccupied = true;

                var pickable = obj.GetComponent<PickableObject>();
                if (pickable != null)
                {
                    pickable.SetCurrentWorkStation(this);
                }

                OnIngredientPlaced?.Invoke();
                UpdateVisuals();
                return true;
            }
            // Les contenants peuvent aussi être placés sur des tables (stations sans type)
            else if (stationType == 0)  // Pour les tables
            {
                obj.transform.position = ingredientPosition.position;
                obj.transform.rotation = ingredientPosition.rotation;

                currentObject = obj;
                currentContainer = container;
                isOccupied = true;

                var pickable = obj.GetComponent<PickableObject>();
                if (pickable != null)
                {
                    pickable.SetCurrentWorkStation(this);
                }

                OnIngredientPlaced?.Invoke();
                UpdateVisuals();
                return true;
            }
            ShowInvalidPlacement();
            return false;
        }

        // Gestion standard des ingrédients
        if (processable != null)
        {
            if (!processable.CanProcess(stationType))
            {
                Debug.Log($"Ingredient {obj.name} cannot be processed here - Type: {stationType}");
                ShowInvalidPlacement();
                return false;
            }

            obj.transform.position = ingredientPosition.position;
            obj.transform.rotation = ingredientPosition.rotation;

            if (currentObject != obj)
            {
                currentObject = obj;
                currentProcessable = processable;
                isOccupied = true;

                var pickable = obj.GetComponent<PickableObject>();
                if (pickable != null)
                {
                    pickable.SetCurrentWorkStation(this);
                }

                OnIngredientPlaced?.Invoke();
            }

            UpdateVisuals();
            return true;
        }

        Debug.Log($"Object {obj.name} cannot be placed here");
        ShowInvalidPlacement();
        return false;
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
            currentObject = null;
            currentProcessable = null;
            currentContainer = null;
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
            bool success = attachedPan.StartCooking();
            if (success)
            {
                ShowProcessingVisual();
            }
            else
            {
                isProcessing = false;
            }
            return;
        }

        if (currentProcessable != null && currentProcessable.CanStartProcessing(stationType))
        {
            Debug.Log($"Starting processing of {currentObject.name} - Type: {stationType}");
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
        return isOccupied && (currentObject != null);
    }

    public virtual bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        if (isCookingStation && attachedPan != null)
        {
            return attachedPan.CanAcceptIngredient(ingredient);
        }
        return !isOccupied || currentObject == ingredient.gameObject;
    }

    public ProcessType GetStationType()
    {
        return stationType;
    }

    protected virtual void ShowInvalidPlacement()
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

    protected virtual void UpdateVisuals()
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