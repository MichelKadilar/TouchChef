using UnityEngine;
using UnityEngine.Events;

public class WorkStation : MonoBehaviour
{
    [Header("Configuration")]
    [SerializeField] public ProcessType stationType;
    [SerializeField] public Transform ingredientPosition;
    [SerializeField] private float processRadius = 1f;
    [SerializeField] public WorkStationPosition workStationPosition;
    
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
    
    public string GetAssignedPlayerId()
    {
        return WorkstationManager.Instance?.GetPlayerIdForWorkstation(this);
    }

    private void InitializeCookingPan()
    {
        GameObject panObject = Instantiate(panPrefab, ingredientPosition.position, ingredientPosition.rotation);
        attachedPan = panObject.GetComponent<Pan>();
        
        if (attachedPan != null)
        {
            currentObject = panObject;
            currentContainer = attachedPan;
            isOccupied = true;
            
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
        
        if (currentObject == obj)
        {
            obj.transform.position = ingredientPosition.position;
            obj.transform.rotation = ingredientPosition.rotation;
            
            var pickable = obj.GetComponent<PickableObject>();
            if (pickable != null)
            {
                pickable.SetCurrentWorkStation(this);
            }
            return true;
        }
        
        if (isOccupied && currentObject != obj)
        {
            Debug.Log($"Workstation {gameObject.name} is occupied by different object {currentObject.name}");
            ShowInvalidPlacement();
            return false;
        }
        
        if (isCookingStation && attachedPan != null)
        {
            var ingredient = obj.GetComponent<BaseIngredient>();
            if (ingredient != null)
            {
                if (attachedPan.CanAcceptIngredient(ingredient))
                {
                    bool success = attachedPan.AddIngredient(ingredient);
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
        
        var processable = obj.GetComponent<BaseIngredient>();
        var container = obj.GetComponent<IContainer>();
        
        if (processable == null && container == null)
        {
            Debug.Log($"Object {obj.name} is neither ingredient nor container");
            ShowInvalidPlacement();
            return false;
        }
        
        if (container != null)
        {
            if (stationType == ProcessType.Cook || stationType == 0)
            {
                PlaceObject(obj, null, container);
                return true;
            }
            ShowInvalidPlacement();
            return false;
        }
        
        if (this.stationType == ProcessType.Cook)
        {
            StartProcessing();
            return true;
        }

        if (processable != null)
        {
            if (!processable.CanProcess(stationType))
            {
                Debug.Log($"Ingredient {obj.name} cannot be processed here - Type: {stationType}");
                ShowInvalidPlacement();
                return false;
            }

            PlaceObject(obj, processable, null);
            return true;
        }
        

        Debug.Log($"Object {obj.name} cannot be placed here");
        ShowInvalidPlacement();
        return false;
    }

    private void PlaceObject(GameObject obj, BaseIngredient processable, IContainer container)
    {
        obj.transform.position = ingredientPosition.position;
        obj.transform.rotation = ingredientPosition.rotation;

        currentObject = obj;
        currentProcessable = processable;
        currentContainer = container;
        isOccupied = true;

        var pickable = obj.GetComponent<PickableObject>();
        if (pickable != null)
        {
            pickable.SetCurrentWorkStation(this);
        }

        OnIngredientPlaced?.Invoke();
        UpdateVisuals();
        
        Debug.Log($"Successfully placed {obj.name} on workstation");
    }

    public void RemoveIngredient()
    {
        if (!isOccupied) return;

        if (isCookingStation && attachedPan != null)
        {
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
        
        Debug.Log($"TESTESTESTETSAOTHEUIYARGEAOIKR {currentProcessable.CanStartProcessing(stationType)}");

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

    protected virtual void CompleteProcessing()
    {
        isProcessing = false;
        UpdateVisuals();
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
    
    public Transform GetIngredientPosition()
    {
        return ingredientPosition;
    }
}