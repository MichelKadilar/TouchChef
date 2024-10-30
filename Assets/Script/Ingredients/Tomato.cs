using System.Collections.Generic;
using UnityEngine;

namespace Script.Ingredients
{
    public class Tomato : BaseIngredient
    {
        [SerializeField] private GameObject rawVisual;
        [SerializeField] private GameObject cutVisual;
        
        protected override void Awake()
        {
            base.Awake();
            allowedProcesses = new List<ProcessType> { ProcessType.Cut };
            UpdateVisual();
        }

        protected override void CompleteProcessing(ProcessType processType)
        {
            switch (processType)
            {
                case ProcessType.Cut:
                    if (currentState == IngredientState.Washed)
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