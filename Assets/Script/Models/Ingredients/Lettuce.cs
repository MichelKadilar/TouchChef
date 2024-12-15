using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Lettuce : BaseIngredient, ISliceable, IWashable
{
    [Header("State Visuals")]
    [SerializeField] private GameObject stateVisualContainer;
    [SerializeField] private GameObject rawVisual;      // Salade propre
    [SerializeField] private GameObject cutVisual;
    [SerializeField] private GameObject dirtyVisual;    // Salade sale
    [SerializeField] private GameObject waterVisual;

    private Slider _washSlider;

    [Header("Wash Settings")]
    [SerializeField] private int requiredWashes = 4;
    private int currentWashes = 0;

    [Header("Slice Options")]
    public int neededSlices = 2;
    private int currentSlice = 0;

    protected void Awake()
    {
        if (stateVisualContainer == null)
        {
            Debug.LogError($"{gameObject.name}: stateVisualContainer is not assigned!");
            return;
        }

        if (_washSlider == null)
        {
            Transform canvaTransform = stateVisualContainer.transform.Find("Canvas");
            if (canvaTransform != null)
            {
                _washSlider = canvaTransform.GetComponentInChildren<Slider>();
                _washSlider.maxValue = requiredWashes;
                _washSlider.gameObject.SetActive(false);
            }

            if (_washSlider == null)
            {
                Debug.LogError($"{gameObject.name}: Wash slider is not found inside the Canvas child of stateVisualContainer!");
            }
        }

        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }

        waterVisual.SetActive(false);
        allowedProcesses.Clear();
        // On commence par permettre uniquement le lavage
        allowedProcesses.Add(ProcessType.Wash);

        base.Awake();
        UpdateVisual();
    }

    public void Slice()
    {
        // On vérifie que la salade est à l'état Washed avant de permettre la découpe
        if (currentState == IngredientState.Washed)
        {
            currentSlice++;
            _washSlider.gameObject.SetActive(true);
            _washSlider.value = currentSlice;
            if (currentSlice >= neededSlices)
            {
                currentState = IngredientState.Cut;
                _washSlider.gameObject.SetActive(false);
                NotifyActionProgress("cut");
            }
            UpdateVisual();
        }
        else
        {
            Debug.Log("La salade doit être lavée avant d'être coupée !");
        }
    }

    public void DoWash()
    {
        if (currentState != IngredientState.Raw || currentWashes >= requiredWashes) return;

        currentWashes++;
        _washSlider.gameObject.SetActive(true);
        _washSlider.value = currentWashes;

        if (currentWashes >= requiredWashes)
        {
            currentState = IngredientState.Washed;
            _washSlider.gameObject.SetActive(false);
            
            // Une fois lavée, on permet la découpe
            allowedProcesses.Clear();
            allowedProcesses.Add(ProcessType.Cut);
            
            NotifyActionProgress("wash");
            UpdateVisual();
        }
    }

    public void StartWash()
    {
        if (currentState == IngredientState.Raw && !IsClean())
        {
            waterVisual.SetActive(true);
            _washSlider.gameObject.SetActive(true);
        }
    }

    public void StopWash()
    {
        waterVisual.SetActive(false);
        if (!IsClean())
        {
            _washSlider.gameObject.SetActive(false);
        }
    }

    private void UpdateVisual()
    {
        dirtyVisual.SetActive(currentState == IngredientState.Raw && !IsClean());
        rawVisual.SetActive(currentState == IngredientState.Washed);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }

    public float GetCleanliness()
    {
        return (float)currentWashes / requiredWashes;
    }

    public bool IsClean()
    {
        return currentWashes >= requiredWashes;
    }
}