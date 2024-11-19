
using System.Collections.Generic;
using UnityEngine;

public class Lettuce : BaseIngredient, ISliceable
{
    [Header("State Visuals")]
    [SerializeField] private GameObject stateVisualContainer; 
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;
    
    [Header("Slice Options")]
    public int neededSlices = 2;

    private int currentSlice = 0;
    
    protected override void Awake()
    {
        // Ensure stateVisualContainer is assigned
        if (stateVisualContainer == null)
        {
            Debug.LogError($"{gameObject.name}: stateVisualContainer is not assigned!");
            return;
        }
        

        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);

    
        
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
            if (currentSlice >= neededSlices)
            {
                Debug.Log("CURRENT STATE : CUT");
                currentState = IngredientState.Cut;
                
            }
        }
        UpdateVisual();
    }
    
    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }

}
