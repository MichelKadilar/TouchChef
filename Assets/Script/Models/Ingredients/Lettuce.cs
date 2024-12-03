
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class Lettuce : BaseIngredient, ISliceable, IWashable
{
    [Header("State Visuals")]
    [SerializeField] private GameObject stateVisualContainer; 
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;
    [SerializeField] private GameObject waterVisual;
    
    private Slider _slider;
    
    [Header("Slice Options")]
    public int neededSlices = 2;

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
        waterVisual.SetActive(false);
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);
        allowedProcesses.Add(ProcessType.Wash);

    
        
        base.Awake();
        UpdateVisual();
        Debug.Log($"CURRENT STATE: {currentState}");
    }
    
    public void Slice()
    {
        Debug.Log("Slicing Lettuce !");
        if (currentState == IngredientState.Raw)
        {
            currentSlice++;
            _slider.gameObject.SetActive(true);
            _slider.value = currentSlice;
            if (currentSlice >= neededSlices)
            {
                Debug.Log("CURRENT STATE : CUT");
                currentState = IngredientState.Cut;
                _slider.gameObject.SetActive(false);
                
            }
        }
        UpdateVisual();
    }

    public void StartWash()
    {
        waterVisual.SetActive(true);
    }
    
    public void StopWash()
    {
        waterVisual.SetActive(false);
    }
    
    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }

}
