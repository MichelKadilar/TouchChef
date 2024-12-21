using System.Collections.Generic;
using UnityEngine;

public class Bread : BaseIngredient
{
    [SerializeField] private BreadType breadType;

    protected void Awake()
    {
        base.Awake();
        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Assemble);
    }

    public BreadType GetBreadType()
    {
        return breadType;
    }
}