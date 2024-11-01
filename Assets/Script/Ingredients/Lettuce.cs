using System.Collections.Generic;
using UnityEngine;

public class Lettuce : BaseIngredient
{
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject washedVisual;
    [SerializeField] private GameObject cutVisual;
        
    protected override void Awake()
    {
        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Wash);
            
        base.Awake();
        UpdateVisual();
    }

    protected override void CompleteProcessing(ProcessType processType)
    {
        switch (processType)
        {
            case ProcessType.Wash:
                currentState = IngredientState.Washed;
                allowedProcesses.Clear();
                allowedProcesses.Add(ProcessType.Cut);
                break;
            case ProcessType.Cut:
                currentState = IngredientState.Cut;
                allowedProcesses.Clear();
                break;
        }
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        washedVisual.SetActive(currentState == IngredientState.Washed);
        cutVisual.SetActive(currentState == IngredientState.Cut);
    }
}