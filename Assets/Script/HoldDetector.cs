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

    private void Awake()
    {
        ingredient = GetComponent<BaseIngredient>();
        if (ingredient == null)
        {
            Debug.LogError($"HoldDetector on {gameObject.name}: Missing BaseIngredient component!");
            return;
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

        var workStation = ingredient.GetCurrentWorkStation();
        if (workStation == null)
        {
            if (debugMode) Debug.Log($"Hold failed: No workstation for {gameObject.name}");
            return;
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

        Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
        worldPosition.z = transform.position.z - 0.1f;
        
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
        }

        indicatorInstance = Instantiate(holdIndicatorPrefab, worldPosition, Quaternion.identity);
        holdIndicator = indicatorInstance.GetComponent<HoldIndicator>();
        
        if (debugMode) Debug.Log($"Created hold indicator for {gameObject.name}");
    }

    public void UpdateHolding(int touchId, Vector2 position)
    {
        if (!isHolding || currentTouchId != touchId) return;

        if (debugMode) Debug.Log($"Updating hold position for {gameObject.name}: {position}");
        
        float distance = Vector2.Distance(holdStartPosition, position);
        if (distance > holdRadius)
        {
            if (debugMode) Debug.Log($"Hold canceled: Moved too far on {gameObject.name}");
            StopHolding();
            return;
        }

        if (indicatorInstance != null)
        {
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
            worldPosition.z = transform.position.z - 0.1f;
            indicatorInstance.transform.position = worldPosition;
        }
    }

    private IEnumerator HoldCoroutine()
    {
        if (debugMode) Debug.Log($"Starting hold coroutine for {gameObject.name}");
        
        while (isHolding && currentHoldTime < holdDuration)
        {
            currentHoldTime += Time.deltaTime;
            float progress = currentHoldTime / holdDuration;
            
            if (holdIndicator != null)
            {
                holdIndicator.SetProgress(progress);
            }
            
            if (debugMode && Time.frameCount % 30 == 0) // Log toutes les ~0.5 secondes
            {
                Debug.Log($"Hold progress on {gameObject.name}: {progress:F2}");
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

    public void StopHolding()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
            indicatorInstance = null;
            holdIndicator = null;
        }

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