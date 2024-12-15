using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class Meat : BaseIngredient, ISliceable
    {
        [Header("Visuals")]
        [SerializeField] private GameObject stateVisualContainer;
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        [SerializeField] private GameObject cookedVisual;
        [SerializeField] private GameObject burnedVisual;
        
        private Slider _slider;

        [Header("Slice Options")]
        public int neededSlices = 5; // Number of slices needed to complete slicing
        
        private int currentSlice = 0; // Current slice count
        
        
        [Header("Cooking Options")]
        [SerializeField] private float burnTime = 6f; // Double the normal cooking time
        private float cookingTimer = 0f;
        
        protected void Awake()
        {
            // Ensure stateVisualContainer is assigned
            if (stateVisualContainer == null)
            {
                Debug.LogError($"{gameObject.name}: stateVisualContainer is not assigned!");
                return;
            }

            // Dynamically find the slider within the Canva child
            if (_slider == null)
            {
                Transform canvaTransform = stateVisualContainer.transform.Find("Canvas");
                if (canvaTransform != null)
                {
                    _slider = canvaTransform.GetComponentInChildren<Slider>();
                    _slider.maxValue = neededSlices;
                    _slider.gameObject.SetActive(false);
                }

                if (_slider == null)
                {
                    Debug.LogError($"{gameObject.name}: Slider is not found inside the Canvas child of stateVisualContainer!");
                }
            }
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
            _slider.gameObject.SetActive(true);
            _slider.value = currentSlice;
            if (currentSlice>=neededSlices)
            {
                currentState = IngredientState.Cut;
                allowedProcesses.Clear();
                allowedProcesses.Add(ProcessType.Cook);
                _slider.gameObject.SetActive(false);
                NotifyActionProgress("cut");
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
                    NotifyActionProgress("cut");  // On peut notifier directement pour le cut
                    break;
            
                case ProcessType.Cook:
                    if (cookingTimer < burnTime)
                    {
                        currentState = IngredientState.Cooked;
                        allowedProcesses.Clear();
                        isProcessing = false;
                        StartCoroutine(WaitForRemoval());  // Ajout du coroutine d'attente
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
        private IEnumerator WaitForRemoval()
        {
            float timeoutDuration = 3f;
            float elapsedTime = 0f;
            WorkStation initialStation = GetCurrentWorkStation();
            string originalPlayerId = initialStation?.GetAssignedPlayerId(); // Stockage du playerId original
            bool hasBeenMoved = false;

            // Si pas de playerId au départ, on abandonne
            if (string.IsNullOrEmpty(originalPlayerId))
            {
                Debug.LogWarning("No player assigned to initial cooking station");
                yield break;
            }

            yield return null;

            while (elapsedTime < timeoutDuration)
            {
                WorkStation currentStation = GetCurrentWorkStation();
        
                if (currentStation == null)
                {
                    hasBeenMoved = true;
                }
                else if (hasBeenMoved && currentStation != initialStation)
                {
                    Debug.Log($"Steak moved to new station - Sending notification for player {originalPlayerId}");
                    // Notification manuelle avec le playerId original
                    if (WorkstationManager.Instance != null)
                    {
                        WorkstationManager.Instance.UpdateTaskProgress(initialStation, "cook");
                    }
                    yield break;
                }

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            WorkStation finalStation = GetCurrentWorkStation();
            if (!hasBeenMoved && finalStation != null && finalStation.GetStationType() == ProcessType.Cook)
            {
                Debug.Log("Steak timeout - Setting to burned");
                currentState = IngredientState.Burned;
                allowedProcesses.Clear();
                UpdateVisual();
            }
        }
        
        public override float GetProcessingTimer()
        {
            return cookingTimer;
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
