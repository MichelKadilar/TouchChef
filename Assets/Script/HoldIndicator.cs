using UnityEngine;

[ExecuteInEditMode]
public class HoldIndicator : MonoBehaviour
{
    [SerializeField] private Material indicatorMaterial;
    private MaterialPropertyBlock propBlock;
    private MeshRenderer meshRenderer;
    
    [Header("Visual Settings")]
    public Color ringColor = new Color(0.2f, 0.6f, 1f, 0.8f);
    public Color backgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.3f);
    public float radius = 0.45f;
    public float innerRadius = 0.35f;

    private void Awake()
    {
        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisuals();
    }

    private void OnValidate()
    {
        if (propBlock == null) propBlock = new MaterialPropertyBlock();
        if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
        UpdateVisuals();
    }

    private void UpdateVisuals()
    {
        if (propBlock == null) return;
        if (meshRenderer == null) return;
        if (indicatorMaterial == null) return;

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
        if (propBlock == null || meshRenderer == null) return;
        meshRenderer.GetPropertyBlock(propBlock);
        propBlock.SetFloat("_Progress", progress);
        meshRenderer.SetPropertyBlock(propBlock);
    }
}