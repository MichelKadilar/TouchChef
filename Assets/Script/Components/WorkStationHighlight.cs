using UnityEngine;

[RequireComponent(typeof(WorkStation))]
public class WorkStationHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private float outlineWidth = 4f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseMinIntensity = 0.6f;
    [SerializeField] private float pulseMaxIntensity = 1f;
    [SerializeField] private bool highlightEnabled = false;  // Pour tester dans l'inspecteur
    [SerializeField] private Color testColor = Color.red;    // Pour tester dans l'inspecteur

    private Outline outlineComponent;
    private bool isHighlighted = false;
    private Color currentHighlightColor;
    private float currentPulseTime = 0f;
    private WorkStation workStation;

    private void Awake()
    {
        workStation = GetComponent<WorkStation>();
        
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        
        outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
        outlineComponent.OutlineWidth = outlineWidth;
        outlineComponent.enabled = false;
    }

    private void OnValidate()
    {
        // Cette fonction est appelée quand une valeur est modifiée dans l'inspecteur
        if (Application.isPlaying && outlineComponent != null)
        {
            SetHighlight(highlightEnabled, testColor);
        }
    }

    public void SetHighlight(bool enabled, Color? color = null)
    {
        if (outlineComponent == null) return;

        isHighlighted = enabled;
        outlineComponent.enabled = enabled;
        
        if (!enabled)
        {
            currentPulseTime = 0f;
            return;
        }
        
        if (color.HasValue)
        {
            currentHighlightColor = color.Value;
            outlineComponent.OutlineColor = currentHighlightColor;
        }
    }

    private void Update()
    {
        if (isHighlighted && outlineComponent != null && outlineComponent.enabled)
        {
            currentPulseTime += Time.deltaTime * pulseSpeed;
            float pulseIntensity = Mathf.Lerp(pulseMinIntensity, pulseMaxIntensity, 
                (Mathf.Sin(currentPulseTime) + 1f) * 0.5f);
            
            Color pulseColor = currentHighlightColor * pulseIntensity;
            outlineComponent.OutlineColor = pulseColor;
        }
    }

    public WorkStation GetWorkStation()
    {
        return workStation;
    }

    public bool IsHighlighted()
    {
        return isHighlighted;
    }
}