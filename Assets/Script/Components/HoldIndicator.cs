using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshRenderer))]  // S'assure que le MeshRenderer est présent
public class HoldIndicator : MonoBehaviour
{
    [SerializeField] private Material indicatorMaterial;
    private MaterialPropertyBlock propBlock;
    private MeshRenderer meshRenderer;
    
    [Header("Visual Settings")]
    public Color ringColor = new Color(0.25f, 1f, 0.36f, 0.8f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    public float radius = 0.45f;
    public float innerRadius = 0.35f;

    private void Awake()
    {
        Debug.Log($"HoldIndicator Awake called on {gameObject.name}");
        InitializeComponents();
        UpdateVisuals();
    }

    private void OnEnable()
    {
        Debug.Log($"HoldIndicator OnEnable called on {gameObject.name}");
        InitializeComponents();
        UpdateVisuals();
    }

    private void InitializeComponents()
    {
        if (propBlock == null) 
            propBlock = new MaterialPropertyBlock();
        
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            if (meshRenderer != null)
            {
                // Configuration pour être toujours visible
                meshRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                meshRenderer.receiveShadows = false;
                meshRenderer.sortingOrder = 100;
            }
        }
    }

    private void UpdateVisuals()
    {
        Debug.Log($"Updating visuals for {gameObject.name}");
        
        if (indicatorMaterial == null)
        {
            Debug.LogError($"Indicator material missing on {gameObject.name}");
            return;
        }

        meshRenderer.material = indicatorMaterial;
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetColor("_Color", ringColor);
        propBlock.SetColor("_BackgroundColor", backgroundColor);
        propBlock.SetFloat("_Radius", radius);
        propBlock.SetFloat("_InnerRadius", innerRadius);
        propBlock.SetFloat("_Progress", 0f);
        meshRenderer.SetPropertyBlock(propBlock);
    }

    public void SetProgress(float progress)
    {
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_Progress", progress);
        meshRenderer.SetPropertyBlock(propBlock);
        Debug.Log($"Progress updated to {progress:F2} on {gameObject.name}");
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying) return;
        InitializeComponents();
        UpdateVisuals();
    }
#endif
}