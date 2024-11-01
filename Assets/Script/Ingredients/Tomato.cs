using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Script.Ingredients
{
    public class Tomato : BaseIngredient
    {
        [Header("Slider")]
        public GameObject sliderPrefab; // Prefab for the slider
        
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
            UpdateVisual(Vector2.zero);
            Debug.Log($" CURRENT STATE : {currentState}");
        }

        // This method is called to slice the tomato
        public void Slice(Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            Debug.Log("WAAAAAAAAAA : Slicing tomato!");
            if (currentState == IngredientState.Raw)
            {
                // Check if the slider has been instantiated
                if (sliderInstance == null)
                {
                    // Position the slider in the middle of the screen (camera view center)
                    
                    Vector3 rawPosition = camera.ViewportToWorldPoint(new Vector3(ingredient.transform.position.x, ingredient.transform.position.y, 10));
    
                    // Instantiate the slider prefab at this position
                    sliderInstance = Instantiate(sliderPrefab, rawPosition, Quaternion.identity);
                    
                    // Get the Slider component and set max and initial values
                    slider = sliderInstance.GetComponentInChildren<Slider>();
                    slider.maxValue = neededSlices;
                    slider.value = currentSlice;
                }


                
                currentSlice++;
                slider.value = currentSlice;
                Debug.Log($"WAAAAAAAA : Sliced tomato! Current slice: {currentSlice}");
                
                if (currentSlice == neededSlices)
                {
                    Debug.Log("WAAAAAAAAAA : Sliced all slices!");
                    CompleteProcessing(ProcessType.Cut, position, camera, ingredient);
                }
            }
        }

        protected void CompleteProcessing(ProcessType processType, Vector2 position, Camera camera, BaseIngredient ingredient)
        {
            Debug.Log("WAAAAAAAAAA : ANA WAST Completed processing!");
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

            UpdateVisual(position, camera); // Update visual state (if needed)
        }

        private void UpdateVisual(Vector2 position, Camera camera = null)
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
