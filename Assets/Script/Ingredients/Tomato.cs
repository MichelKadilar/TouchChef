using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Ingredients
{
    public class Tomato : BaseIngredient
    {
        [Header("Slider")]
        [SerializeField] private GameObject sliderPrefabRight; // Prefab for the slider
        [SerializeField] private GameObject sliderPrefabLeft; // Prefab for the slider
        
        [Header("Slice Options")]
        public int neededSlices = 4; // Number of slices needed to complete slicing
        
        [SerializeField] private GameObject rawVisual; // Raw visual representation
        [SerializeField] private GameObject cutVisual; // Cut visual representation
        
        private Slider slider; // Reference to the slider
        private int currentSlice = 0; // Current slice count
        
        private GameObject instantiatedCutTomato; // To hold the instantiated cut visual
        private GameObject sliderInstance; // To hold the instantiated slider

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

        // This method is called to slice the tomato
        public void Slice(Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            Debug.Log("Slicing tomato!");
            if (currentState == IngredientState.Raw)
            {
                // Check if the slider has been instantiated
                if (sliderInstance == null)
                {
                    // Set the screen position for the slider with z = 10
                    Vector3 screenPos = camera.WorldToScreenPoint(new Vector3(ingredient.transform.position.x, ingredient.transform.position.y, 10));
                    Debug.Log($"Screen position: {screenPos}");

                    if (ingredient.GetCurrentWorkStation().workStationPosition == WorkStationPosition.RIGHT)
                    {
                        // Instantiate the slider prefab at this position
                        sliderInstance = Instantiate(sliderPrefabRight, screenPos, Quaternion.identity); 
                    }
                    else if (ingredient.GetCurrentWorkStation().workStationPosition == WorkStationPosition.LEFT)
                    {
                        // Instantiate the slider prefab at this position
                        sliderInstance = Instantiate(sliderPrefabLeft, screenPos, Quaternion.identity); 
                    }

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
                            scaler.referenceResolution = new Vector2(1920, 1080); // Set the reference resolution
                            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
                            scaler.matchWidthOrHeight = 0.5f; // Adjust this value as needed
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
                Debug.Log($"Sliced tomato! Current slice: {currentSlice}");

                if (currentSlice == neededSlices)
                {
                    Debug.Log("All slices completed!");
                    CompleteProcessing(ProcessType.Cut, position, camera, ingredient);
                }
            }
        }




        protected void CompleteProcessing(ProcessType processType, Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            switch (processType)
            {
                case ProcessType.Cut:
                    currentState = IngredientState.Cut;

                    // Instantiate the cut visual when slicing is complete
                    Vector3 rawPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y, 10));
                    instantiatedCutTomato = Instantiate(cutVisual, rawPosition, Quaternion.identity);

                    ingredient.gameObject.SetActive(false);
                    Debug.Log("Object sliced!");
                    
                    break;
            }
            // Destroy the slider after processing is complete
            if (sliderInstance != null)
            {
                Destroy(sliderInstance);
            }

            UpdateVisual(); // Update visual state (if needed)
        }

        private void UpdateVisual()
        {
            if (currentState == IngredientState.Raw)
            {
                rawVisual.SetActive(true);
                if (instantiatedCutTomato != null)
                {
                    Destroy(instantiatedCutTomato);
                }
            }
            else if (currentState == IngredientState.Cut)
            {
                Debug.Log("Cut visual instantiated!");
            }
        }
    }
}