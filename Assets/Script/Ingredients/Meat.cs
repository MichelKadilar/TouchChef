using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Meat : BaseIngredient
    {
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        [SerializeField] private GameObject cookedVisual;
        [SerializeField] private GameObject burnedVisual;
        [SerializeField] private float burnTime = 6f; // Double du temps de cuisson normal
        
        private float cookingTimer = 0f;
        
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
            switch (processType)
            {
                case ProcessType.Cut:
                    currentState = IngredientState.Cut;
                    allowedProcesses.Clear();
                    allowedProcesses.Add(ProcessType.Cook);
                    break;
                case ProcessType.Cook:
                    if (cookingTimer < burnTime)
                    {
                        currentState = IngredientState.Cooked;
                        allowedProcesses.Clear();
                    }
                    else
                    {
                        currentState = IngredientState.Burned;
                        allowedProcesses.Clear();
                    }
                    break;
            }
            UpdateVisual();
        }

        protected override IEnumerator ProcessingCoroutine(ProcessType processType)
        {
            if (processType == ProcessType.Cook)
            {
                isProcessing = true;
                cookingTimer = 0f;
                
                while (cookingTimer < burnTime)
                {
                    cookingTimer += Time.deltaTime;
                    float progress = cookingTimer / processingTime;
                    
                    if (cookingTimer >= processingTime && currentState != IngredientState.Cooked)
                    {
                        CompleteProcessing(ProcessType.Cook);
                    }
                    else if (cookingTimer >= burnTime && currentState != IngredientState.Burned)
                    {
                        CompleteProcessing(ProcessType.Cook);
                    }
                    
                    OnProcessingProgress(progress);
                    yield return null;
                }
                
                isProcessing = false;
            }
            else
            {
                yield return base.ProcessingCoroutine(processType);
            }
        }

        private void UpdateVisual()
        {
            rawVisual.SetActive(currentState == IngredientState.Raw);
            cutVisual.SetActive(currentState == IngredientState.Cut);
            cookedVisual.SetActive(currentState == IngredientState.Cooked);
            burnedVisual.SetActive(currentState == IngredientState.Burned);
        }
    }
