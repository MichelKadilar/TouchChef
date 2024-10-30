using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseIngredient : PickableObject, IProcessable
{
    [SerializeField] protected List<ProcessType> allowedProcesses;
    [SerializeField] protected float processingTime = 3f;
    
    protected IngredientState currentState = IngredientState.Raw;
    protected bool isProcessing = false;
    
    public IngredientState CurrentState => currentState;
    
    protected override void Awake()
    {
        base.Awake();
        // Initialisation spécifique aux ingrédients
    }
    
    public virtual bool CanProcess(ProcessType processType)
    {
        return allowedProcesses.Contains(processType) && !isProcessing;
    }
    
    public virtual void Process(ProcessType processType)
    {
        if (!CanProcess(processType)) return;
        
        StartCoroutine(ProcessingCoroutine(processType));
    }
    
    protected virtual IEnumerator ProcessingCoroutine(ProcessType processType)
    {
        isProcessing = true;
        float timer = 0;
        
        while (timer < processingTime)
        {
            timer += Time.deltaTime;
            OnProcessingProgress(timer / processingTime);
            yield return null;
        }
        
        CompleteProcessing(processType);
        isProcessing = false;
    }
    
    protected virtual void CompleteProcessing(ProcessType processType)
    {
        // À implémenter dans les classes dérivées
    }
    
    protected virtual void OnProcessingProgress(float progress)
    {
        // Pour les effets visuels de progression
    }
}