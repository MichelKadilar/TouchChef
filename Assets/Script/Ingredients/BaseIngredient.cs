﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(HoldDetector))]
public abstract class BaseIngredient : PickableObject, IProcessable
{
    [SerializeField] protected List<ProcessType> allowedProcesses = new List<ProcessType>();
    [SerializeField] protected float processingTime = 3f;
    
    protected HoldDetector holdDetector;
    protected IngredientState currentState = IngredientState.Raw;
    protected bool isProcessing = false;
    
    public IngredientState CurrentState => currentState;
    
    protected override void Awake()
    {
        base.Awake();
        InitializeHoldDetector();
    }
    
    private void OnEnable()
    {
        InitializeHoldDetector();
    }

    private void InitializeHoldDetector()
    {
        if (holdDetector != null && holdDetector.OnHoldComplete.GetPersistentEventCount() > 0)
            return;
        
        holdDetector = GetComponent<HoldDetector>();
        if (holdDetector == null)
        {
            holdDetector = gameObject.AddComponent<HoldDetector>();
        }
        holdDetector.OnHoldComplete.RemoveListener(OnHoldCompleted);
        holdDetector.OnHoldComplete.AddListener(OnHoldCompleted);
    }
    
    public virtual bool CanProcess(ProcessType processType)
    {
        if (allowedProcesses == null) 
        {
            Debug.LogError($"{gameObject.name}: allowedProcesses is null!");
            return false;
        }
        
        if (isProcessing)
        {
            Debug.Log($"Cannot process {gameObject.name}: already processing");
            return false;
        }
    
        Debug.Log($"Checking if {gameObject.name} can process {processType}. Allowed processes: {string.Join(", ", allowedProcesses)}");
        bool canProcess = allowedProcesses.Contains(processType);
        return canProcess;
    }
    
    public virtual void Process(ProcessType processType)
    {
        if (!CanProcess(processType))
        {
            Debug.Log($"Process cancelled for {gameObject.name}: CanProcess returned false");
            return;
        }
        
        Debug.Log($"Starting process on {gameObject.name} of type {processType}");
        StartCoroutine(ProcessingCoroutine(processType));
    }
    
    public virtual bool CanStartProcessing(ProcessType processType)
    {
        if (IsBeingDragged)
        {
            Debug.Log($"Cannot start processing {gameObject.name}: currently being dragged");
            return false;
        }
    
        return CanProcess(processType);
    }
    
    protected virtual IEnumerator ProcessingCoroutine(ProcessType processType)
    {
        isProcessing = true;
        Debug.Log($"Processing started on {gameObject.name}");
        float timer = 0;
        
        while (timer < processingTime)
        {
            timer += Time.deltaTime;
            float progress = timer / processingTime;
            OnProcessingProgress(progress);
            yield return null;
        }
        
        CompleteProcessing(processType);
        isProcessing = false;
        Debug.Log($"Processing completed on {gameObject.name}");
    }
    
    protected virtual void CompleteProcessing(ProcessType processType)
    {
        Debug.Log($"Base CompleteProcessing called on {gameObject.name}");
    }
    
    protected virtual void OnProcessingProgress(float progress)
    {
        // Pour les effets visuels de progression
    }

    private void OnHoldCompleted(int touchId)
    {
        Debug.Log($"Hold completed on {gameObject.name}, initiating pick with touchId {touchId}");
        
        if (isProcessing || IsBeingDragged)
        {
            Debug.Log($"Cannot pick {gameObject.name}: {(isProcessing ? "currently processing" : "already being dragged")}");
            return;
        }

        OnTouchPick(touchId);
        Debug.Log($"Pick initiated successfully on {gameObject.name}");
    }

    public override void OnTouchPick(int touchId)
    {
        if (isProcessing)
        {
            Debug.Log($"Pick cancelled on {gameObject.name}: currently processing");
            return;
        }
        
        Debug.Log($"OnTouchPick called on {gameObject.name} with touchId {touchId}");
        base.OnTouchPick(touchId);
    }

    public override void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        float yRotation = transform.eulerAngles.y;
        base.OnTouchDrop(touchId, screenPosition);
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y = yRotation;
        transform.rotation = Quaternion.Euler(currentRotation);
    }

    private void OnDisable()
    {
        if (holdDetector != null)
        {
            holdDetector.OnHoldComplete.RemoveListener(OnHoldCompleted);
        }
        
        StopAllCoroutines();
    }
}