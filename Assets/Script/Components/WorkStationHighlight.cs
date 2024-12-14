using UnityEngine;

[RequireComponent(typeof(WorkStation))]
public class WorkStationHighlight : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private Color defaultHighlightColor = Color.yellow;
    [SerializeField] private float outlineWidth = 4f;
    [SerializeField] private float pulseSpeed = 1f;
    [SerializeField] private float pulseMinIntensity = 0.6f;
    [SerializeField] private float pulseMaxIntensity = 1f;

    private Outline outlineComponent;
    private bool isHighlighted = false;
    private Color currentHighlightColor;
    private float currentPulseTime = 0f;

    private void Awake()
    {
        // S'assurer que nous avons un composant Outline
        outlineComponent = GetComponent<Outline>();
        if (outlineComponent == null)
        {
            outlineComponent = gameObject.AddComponent<Outline>();
        }
        
        // Configuration initiale de l'Outline
        outlineComponent.OutlineMode = Outline.Mode.OutlineAll;
        outlineComponent.OutlineWidth = outlineWidth;
        outlineComponent.enabled = false; // Désactivé par défaut
        
        currentHighlightColor = defaultHighlightColor;
    }

    private void Start()
    {
        // Activer l'outline en rouge par défaut pour les tests
        SetHighlight(true, Color.red);
    }

    public void SetHighlight(bool enabled, Color? color = null)
    {
        if (outlineComponent == null) return;

        isHighlighted = enabled;
        outlineComponent.enabled = enabled;
        
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
            // Effet de pulsation
            currentPulseTime += Time.deltaTime * pulseSpeed;
            float pulseIntensity = Mathf.Lerp(pulseMinIntensity, pulseMaxIntensity, 
                (Mathf.Sin(currentPulseTime) + 1f) * 0.5f);
            
            Color pulseColor = currentHighlightColor * pulseIntensity;
            outlineComponent.OutlineColor = pulseColor;
        }
    }
}