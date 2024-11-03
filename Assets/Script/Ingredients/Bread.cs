using System.Collections.Generic;
using UnityEngine;

public class Bread : BaseIngredient
{
    [Header("Visual")]
    [SerializeField] private GameObject breadVisual;

    protected override void Awake()
    {
        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        
        base.Awake();
        UpdateVisual();
        Debug.Log($"Bread initialized with state: {currentState}");
    }

    protected override void CompleteProcessing(ProcessType processType)
    {
        Debug.Log("Bread cannot be processed");
    }

    private void UpdateVisual()
    {
        if (breadVisual != null)
        {
            breadVisual.SetActive(true);
        }
    }

    public override bool CanProcess(ProcessType processType)
    {
        return false;
    }

    public override void OnTouchDrop(int touchId, Vector2 screenPosition)
    {
        float yRotation = transform.eulerAngles.y;
        base.OnTouchDrop(touchId, screenPosition);
        Vector3 currentRotation = transform.eulerAngles;
        currentRotation.y = yRotation;
        transform.rotation = Quaternion.Euler(currentRotation);
    }
}