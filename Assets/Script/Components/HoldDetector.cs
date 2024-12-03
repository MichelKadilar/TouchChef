using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class HoldDetector : MonoBehaviour
{
    [Header("Hold Configuration")]
    [SerializeField] private float holdDuration = 0.5f;
    [SerializeField] private float holdRadius = 0.5f;
    
    [Header("Debug")]
    [SerializeField] private bool debugMode = true;
    
    [Header("Visual Feedback")]
    [SerializeField] private GameObject holdIndicatorPrefab;
    
    public UnityEvent<int> OnHoldComplete = new UnityEvent<int>();
    
    private GameObject indicatorInstance;
    private HoldIndicator holdIndicator;
    private float currentHoldTime;
    private bool isHolding;
    private int? currentTouchId;
    private Vector2 holdStartPosition;
    private BaseIngredient ingredient;
    private IContainer container;
    private Camera mainCamera;
    public bool IsHolding => isHolding;
    

    private void Awake()
    {
        ingredient = GetComponent<BaseIngredient>();
        container = GetComponent<IContainer>();
        
        if (ingredient == null && container == null)
        {
           Debug.LogError($"HoldDetector on {gameObject.name}: Missing BaseIngredient or IContainer component!");
            return;
        }
        
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }
    }

     public void StartHolding(int touchId, Vector2 position)
    {
        if (debugMode) Debug.Log($"StartHolding called on {gameObject.name} with touchId {touchId}");
        
        if (isHolding || currentTouchId.HasValue)
        {
            if (debugMode) Debug.Log($"Hold failed: Already holding or touch assigned on {gameObject.name}");
            return;
        }

        WorkStation workStation = null;
        if (ingredient != null)
        {
            workStation = ingredient.GetCurrentWorkStation();
        }
        else if (container != null)
        {
            var pickable = container as PickableObject;
            if (pickable != null)
            {
                workStation = pickable.GetCurrentWorkStation();
            }
        }

        if (workStation == null)
        {
            if (debugMode) Debug.Log($"No workstation for {gameObject.name}, allowing hold");
        }
        else
        {
            Collider2D workStationCollider = workStation.GetComponent<Collider2D>();
            if (workStationCollider != null)
            {
                Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
                
                if (!workStationCollider.bounds.Contains(worldPosition))
                {
                    if (debugMode) Debug.Log($"Hold failed: Touch outside workstation bounds for {gameObject.name}");
                    return;
                }
            }
        }

        if (debugMode) Debug.Log($"Starting hold process on {gameObject.name}");
        
        
        currentTouchId = touchId;
        holdStartPosition = position;
        isHolding = true;
        currentHoldTime = 0f;
    
        CreateVisualIndicator(position);
        StartCoroutine(HoldCoroutine());
    }


    private void CreateVisualIndicator(Vector2 position)
    {
        if (holdIndicatorPrefab == null)
        {
            Debug.LogError($"HoldDetector on {gameObject.name}: holdIndicatorPrefab not assigned!");
            return;
        }

        if (mainCamera == null)
        {
            Debug.LogError($"HoldDetector on {gameObject.name}: mainCamera is null!");
            return;
        }
        
        CleanupIndicator();
        
        Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
        worldPosition.z = transform.position.z - 0.1f;
    
        Debug.Log($"Creating hold indicator for {gameObject.name} at position {worldPosition}");
    
        try
        {
            indicatorInstance = Instantiate(holdIndicatorPrefab, worldPosition, Quaternion.identity);
            
            holdIndicator = indicatorInstance.GetComponentInChildren<HoldIndicator>();
            
            if (holdIndicator == null)
            {
                Debug.LogError($"Created indicator doesn't have HoldIndicator component in children!");
                Destroy(indicatorInstance);
                return;
            }
            
            float desiredScale = 0.5f;
            indicatorInstance.transform.localScale = new Vector3(desiredScale, desiredScale, desiredScale);
        
            Debug.Log($"Hold indicator successfully created and initialized for {gameObject.name}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Error creating indicator: {e.Message}");
            CleanupIndicator();
        }
    }
    private void CleanupIndicator()
    {
        if (indicatorInstance != null)
        {
            holdIndicator = null;
            Destroy(indicatorInstance);
            indicatorInstance = null;
        }
    }

    private IEnumerator HoldCoroutine()
    {
        if (debugMode) Debug.Log($"Starting hold coroutine for {gameObject.name}");
        currentHoldTime = 0f;
        
        while (isHolding && currentHoldTime < holdDuration)
        {
            if (indicatorInstance == null || holdIndicator == null)
            {
                Debug.LogWarning($"Lost indicator during hold on {gameObject.name}, recreating...");
                CreateVisualIndicator(holdStartPosition);
                if (holdIndicator == null)
                {
                    StopHolding();
                    yield break;
                }
            }

            currentHoldTime += Time.deltaTime;
            float progress = currentHoldTime / holdDuration;
            
            try
            {
                if (holdIndicator != null)
                {
                    holdIndicator.SetProgress(progress);
                    if (debugMode) Debug.Log($"Progress updated to {progress:F2} on {gameObject.name}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error updating progress: {e.Message}");
                StopHolding();
                yield break;
            }
            
            yield return null;
        }

        if (isHolding && currentTouchId.HasValue)
        {
            Debug.Log($"Hold complete on {gameObject.name}, invoking OnHoldComplete");
            OnHoldComplete?.Invoke(currentTouchId.Value);
        }
    
        StopHolding();
    }

    public void UpdateHolding(int touchId, Vector2 position)
    {
        if (!isHolding || currentTouchId != touchId) return;

        float distance = Vector2.Distance(holdStartPosition, position);
        if (distance > holdRadius)
        {
            if (debugMode) Debug.Log($"Hold canceled: Moved too far on {gameObject.name}");
            StopHolding();
            return;
        }

        if (indicatorInstance != null)
        {
            Vector3 worldPosition = mainCamera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
            worldPosition.z = transform.position.z - 0.1f;
            indicatorInstance.transform.position = worldPosition;
        }
    }

    public void StopHolding()
    {
        CleanupIndicator();
        isHolding = false;
        currentTouchId = null;
        currentHoldTime = 0f;
        StopAllCoroutines();

        if (debugMode) Debug.Log($"Stopped holding on {gameObject.name}");
    }

    private void OnDisable()
    {
        StopHolding();
    }
}