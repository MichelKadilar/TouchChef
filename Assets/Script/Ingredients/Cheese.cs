using System.Collections.Generic;
using UnityEngine;

public class Cheese : BaseIngredient
{
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;
        
    protected override void Awake()
    {
        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);
            
        base.Awake();
        UpdateVisual();
    }

    protected override void CompleteProcessing(ProcessType processType)
    {
        if (processType == ProcessType.Cut)
        {
            currentState = IngredientState.Cut;
            allowedProcesses.Clear();
        }
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }
}