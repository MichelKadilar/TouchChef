using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tomato : BaseIngredient, ISliceable
{
    [Header("State Visuals")]
    [SerializeField] private GameObject stateVisualContainer; 
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;
    
    private Slider _slider;

    [Header("Slice Options")]
    public int neededSlices = 4;

    private int currentSlice = 0;

    protected void Awake()
    {
        // Ensure stateVisualContainer is assigned
        if (stateVisualContainer == null)
        {
            Debug.LogError($"{gameObject.name}: stateVisualContainer is not assigned!");
            return;
        }

        // Dynamically find the slider within the Canva child
        if (_slider == null)
        {
            Transform canvaTransform = stateVisualContainer.transform.Find("Canvas");
            if (canvaTransform != null)
            {
                _slider = canvaTransform.GetComponentInChildren<Slider>();
                _slider.maxValue = neededSlices;
                _slider.gameObject.SetActive(false);
            }

            if (_slider == null)
            {
                Debug.LogError($"{gameObject.name}: Slider is not found inside the Canvas child of stateVisualContainer!");
            }
        }

        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);

        Debug.Log($"Tomato {gameObject.name} initializing with process: Cut");
        base.Awake();
        UpdateVisual();
        Debug.Log($"CURRENT STATE: {currentState}");
    }


    public void Slice()
    {
        Debug.Log("Slicing tomato !");
        if (currentState == IngredientState.Raw)
        {
            currentSlice++;
            _slider.gameObject.SetActive(true);
            _slider.value = currentSlice;
            Debug.Log("Slider value: " + _slider.value);
            if (currentSlice >= neededSlices)
            {
                Debug.Log("CURRENT STATE : CUT");
                currentState = IngredientState.Cut;
                _slider.gameObject.SetActive(false);
                UpdateVisual();
                NotifyActionProgress("cut");
            }
            
            
        }
    }

    protected void CompleteProcessing(ProcessType processType)
    {
        switch (processType)
        {
            case ProcessType.Cut:
                currentState = IngredientState.Cut;
                break;
        }

        UpdateVisual();
    }

    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }
}
