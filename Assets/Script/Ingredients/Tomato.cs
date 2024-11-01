using System;
using System.Collections.Generic;
using UnityEngine;

namespace Script.Ingredients
{
    public class Tomato : BaseIngredient
    {
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        
        private GameObject instantiatedCutTomato; // To hold the instantiated cut visual

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

        public void Slice(Vector2 position, Camera camera,BaseIngredient ingredient)
        {
            
            if (currentState == IngredientState.Raw)
            {
                CompleteProcessing(ProcessType.Cut, position,camera,ingredient);
            }
        }

        protected void CompleteProcessing(ProcessType processType, Vector2 position,Camera camera,BaseIngredient ingredient)
        {
            switch (processType)
            {
                case ProcessType.Cut:
                    currentState = IngredientState.Cut;
                    break;
            }
            UpdateVisual(position,camera,ingredient);
        }

        private void UpdateVisual(Vector2 position,Camera camera = null,BaseIngredient ingredient = null)
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
                if (camera == null || ingredient == null)
                {
                    Debug.LogError("Camera or ingredient is null!");
                    return;
                }
                Debug.Log("Position of the raw visual: " + position);
                Vector3 rawPosition = camera.ScreenToWorldPoint(new Vector3(position.x, position.y,15));
                instantiatedCutTomato = Instantiate(cutVisual, rawPosition, Quaternion.identity);

                ingredient.gameObject.SetActive(false);
                Debug.Log("Object sliced!");
            }
        }

        
    }
}