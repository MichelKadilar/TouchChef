﻿using System.Collections.Generic;
using UnityEngine;

namespace Script.Ingredients
{
    public class Tomato : BaseIngredient
    {
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        
        protected override void Awake()
        {
            if (allowedProcesses == null)
            {
                allowedProcesses = new List<ProcessType>();
            }
            allowedProcesses.Clear(); // S'assurer qu'il n'y a pas de doublons
            allowedProcesses.Add(ProcessType.Cut);
        
            Debug.Log($"Tomato {gameObject.name} initializing with process: Cut");
            base.Awake();
            UpdateVisual();
        }

        protected override void CompleteProcessing(ProcessType processType)
        {
            switch (processType)
            {
                case ProcessType.Cut:
                    // Plus de vérification de l'état Washed
                    currentState = IngredientState.Cut;
                    break;
            }
            UpdateVisual();
        }

        private void UpdateVisual()
        {
            if (rawVisual != null) rawVisual.SetActive(currentState == IngredientState.Raw);
            if (cutVisual != null) cutVisual.SetActive(currentState == IngredientState.Cut);
        }
    }
}