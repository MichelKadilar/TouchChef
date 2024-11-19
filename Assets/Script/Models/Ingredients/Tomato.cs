using System.Collections.Generic;
using UnityEngine;

public class Tomato : BaseIngredient, ISliceable
{
    [Header("State Visuals")]
    [SerializeField] private GameObject stateVisualContainer; 
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;

    [Header("Slice Options")]
    public int neededSlices = 4;

    private int currentSlice = 0;

    protected override void Awake()
    {
        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);

        Debug.Log($"Tomato {gameObject.name} initializing with process: Cut");
        base.Awake();
        UpdateVisual();
        Debug.Log($" CURRENT STATE : {currentState}");
    }

    public void Slice()
    {
        Debug.Log("Slicing tomato !");
        if (currentState == IngredientState.Raw)
        {
            Debug.Log("CURRENT STATE : CUT");
            currentState = IngredientState.Cut;
            UpdateVisual();
            
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
        if (stateVisualContainer == null)
        {
            Debug.LogWarning("State visual container is not assigned!");
            return;
        }

        // Disable all child visuals
        foreach (Transform child in stateVisualContainer.transform)
        {
            child.gameObject.SetActive(false);
            Debug.Log($"Child {child.gameObject.name} visual disabled");
        }

        
        switch (currentState)
        {
            case IngredientState.Raw:
                if (rawVisual != null)
                    rawVisual.SetActive(true);
                break;
            case IngredientState.Cut:
                if (cutVisual != null)
                    cutVisual.SetActive(true);
                break;
        }
    }
}
