using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Meat : BaseIngredient
    {
        [Header("Visuals")]
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        [SerializeField] private GameObject cookedVisual;
        [SerializeField] private GameObject burnedVisual;
        
        [Header("Slider")]
        [SerializeField] private GameObject sliderPrefab; // Prefab for the slider
        
        [Header("Slice Options")]
        public int neededSlices = 5; // Number of slices needed to complete slicing
    
        private Slider slider; // Reference to the slider
        private int currentSlice = 0; // Current slice count
    
        private GameObject sliderInstance; // To hold the instantiated slider

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
        // Method to handle slicing
        public void Slice(Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            Debug.Log("WAAA : Slicing meat!");
            
            if (currentState == IngredientState.Raw)
            {
                // Check if the slider has been instantiated
                if (sliderInstance == null)
                {
                    // Set the screen position for the slider with z = 10
                    Vector3 screenPos = camera.WorldToScreenPoint(new Vector3(ingredient.transform.position.x, ingredient.transform.position.y, 10));
                    Debug.Log($"Screen position: {screenPos}");

                    // Instantiate the slider prefab at this position
                    sliderInstance = Instantiate(sliderPrefab, screenPos, Quaternion.identity);

                    // Set the slider as a child of the Canvas (to ensure it appears in UI space)
                    Canvas sliderCanvas = sliderInstance.GetComponentInParent<Canvas>();
                    if (sliderCanvas != null)
                    {
                        sliderCanvas.renderMode = RenderMode.ScreenSpaceCamera;

                        // Get the CanvasScaler and configure it
                        CanvasScaler scaler = sliderCanvas.GetComponent<CanvasScaler>();
                        if (scaler != null)
                        {
                            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                            scaler.referenceResolution = new Vector2(1920, 1080);
                            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                            scaler.matchWidthOrHeight = 0.5f;
                        }
                    }

                    // Get the Slider component and set max and initial values
                    slider = sliderInstance.GetComponentInChildren<Slider>();
                    slider.maxValue = neededSlices;
                    slider.value = currentSlice;
                }

                // Update the slider's position each time it slices
                Vector3 updatedScreenPos = camera.WorldToScreenPoint(new Vector3(ingredient.transform.position.x, ingredient.transform.position.y, 10));
                sliderInstance.transform.position = updatedScreenPos;

                currentSlice++;
                slider.value = currentSlice;
                Debug.Log($"Sliced meat! Current slice: {currentSlice}");

                if (currentSlice == neededSlices)
                {
                    Debug.Log("All slices completed!");
                    CompleteProcessingCut(position, camera, ingredient);
                }
            }
        }
        
        protected void CompleteProcessingCut(Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            currentState = IngredientState.Cut;
            allowedProcesses.Clear();
            allowedProcesses.Add(ProcessType.Cook);

            
            Vector3 rawPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
            Quaternion rotation = Quaternion.Euler(-90, 0, 0); 
            Instantiate(cutVisual, rawPosition, rotation);

           
            ingredient.gameObject.SetActive(false);
            Debug.Log("Meat sliced!");
            
            if (sliderInstance != null)
            {
                Destroy(sliderInstance);
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
            cookedVisual.SetActive(currentState == IngredientState.Cooked);
            burnedVisual.SetActive(currentState == IngredientState.Burned);
            
            if (currentState == IngredientState.Cut)
            {
                Debug.Log("Cut visual instantiated!");
            }
        }
    }
