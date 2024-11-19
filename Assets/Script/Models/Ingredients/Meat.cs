using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Meat : BaseIngredient, ISliceable
    {
        [Header("Visuals")]
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        [SerializeField] private GameObject cookedVisual;
        [SerializeField] private GameObject burnedVisual;
        
     
        [Header("Slice Options")]
        public int neededSlices = 5; // Number of slices needed to complete slicing
        
        private int currentSlice = 0; // Current slice count
        
        
        [Header("Cooking Options")]
        [SerializeField] private float burnTime = 6f; // Double the normal cooking time
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
            Debug.Log($" CURRENT STATE MEAT : {currentState}");
        }

        public void Slice()
        {
            Debug.Log("Slicing meat");
            currentSlice++;
            if (currentSlice>=neededSlices)
            {
                currentState = IngredientState.Cut;
                allowedProcesses.Clear();
                allowedProcesses.Add(ProcessType.Cook);
            }
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
                        isProcessing = false;
                    }
                    else if (cookingTimer >= burnTime && this.GetCurrentWorkStation().GetStationType() == ProcessType.Cook)
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
