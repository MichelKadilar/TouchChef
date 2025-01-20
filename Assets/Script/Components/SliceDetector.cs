using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class SliceDetector : MonoBehaviour
{
    [Header("Slice Configuration")]
    [SerializeField] private LayerMask ingredientLayer;
    [SerializeField] private float raycastDistance = 100f;
    [SerializeField] private float minTimeBetweenSlices = 0.2f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip sliceSound;
    [SerializeField] private float sliceVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Camera mainCamera;
    
    private class WorkstationState
    {
        public bool isSlicing = false;
        public bool canSlice = true;
        public float lastSliceTime = 0f;
        public int? activeTouch = null;
        public BaseIngredient currentIngredient = null;
    }
    
    private Dictionary<WorkStation, WorkstationState> workstationStates = new Dictionary<WorkStation, WorkstationState>();
    private Dictionary<int, WorkStation> activeTouches = new Dictionary<int, WorkStation>(); // Pour suivre quelle touche contrôle quelle workstation

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("SliceDetector: Main Camera not found!");
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) return;
        }

        HandleSliceInput();
    }

    private void HandleSliceInput()
    {
        if (Touchscreen.current != null)
        {
            // Créer une liste des touches actives actuelles
            HashSet<int> currentTouches = new HashSet<int>();
            
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();
                currentTouches.Add(touchId);
                var phase = touch.phase.ReadValue();
                Vector2 position = touch.position.ReadValue();

                switch (phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        TryStartSlice(position, touchId);
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        if (activeTouches.TryGetValue(touchId, out WorkStation workStation))
                        {
                            if (workstationStates.TryGetValue(workStation, out WorkstationState state))
                            {
                                ResetWorkstationState(state);
                            }
                            activeTouches.Remove(touchId);
                        }
                        break;
                }
            }

            // Nettoyer les touches qui ne sont plus actives
            List<int> touchesToRemove = new List<int>();
            foreach (var touch in activeTouches)
            {
                if (!currentTouches.Contains(touch.Key))
                {
                    if (workstationStates.TryGetValue(touch.Value, out WorkstationState state))
                    {
                        ResetWorkstationState(state);
                    }
                    touchesToRemove.Add(touch.Key);
                }
            }
            foreach (int touchId in touchesToRemove)
            {
                activeTouches.Remove(touchId);
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame)
            {
                TryStartSlice(Mouse.current.position.ReadValue(), 0);
            }
            else if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                if (activeTouches.TryGetValue(0, out WorkStation workStation))
                {
                    if (workstationStates.TryGetValue(workStation, out WorkstationState state))
                    {
                        ResetWorkstationState(state);
                    }
                    activeTouches.Remove(0);
                }
            }
        }
    }

    private bool CheckTimeBetweenSlices(WorkstationState state)
    {
        float currentTime = Time.time;
        if (currentTime - state.lastSliceTime >= minTimeBetweenSlices)
        {
            state.lastSliceTime = currentTime;
            return true;
        }
        return false;
    }

    private void TryStartSlice(Vector2 position, int touchId)
    {
        if (mainCamera == null) return;

        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 1.5f, raycastDistance, ingredientLayer);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
            GameObject clickedObject = hits[0].collider.gameObject;
            BaseIngredient ingredient = clickedObject.GetComponent<BaseIngredient>();

            if (ingredient != null && 
                ingredient is ISliceable sliceableIngredient)
            {
                WorkStation workStation = ingredient.GetCurrentWorkStation();
                if (workStation != null && workStation.GetStationType() == ProcessType.Cut)
                {
                    // Créer ou obtenir l'état de la workstation
                    if (!workstationStates.ContainsKey(workStation))
                    {
                        workstationStates[workStation] = new WorkstationState();
                    }
                    WorkstationState state = workstationStates[workStation];

                    // Vérifier que la workstation n'est pas déjà contrôlée par une autre touche
                    if ((state.activeTouch == null || state.activeTouch == touchId) && 
                        state.canSlice && 
                        CheckTimeBetweenSlices(state))
                    {
                        // Si c'est un nouvel ingrédient, réinitialiser son état
                        if (state.currentIngredient != ingredient)
                        {
                            ResetWorkstationState(state);
                            state.currentIngredient = ingredient;
                        }

                        state.isSlicing = true;
                        state.canSlice = false;
                        state.activeTouch = touchId;
                        activeTouches[touchId] = workStation;

                        if (debugMode) Debug.Log($"Slicing ingredient: {clickedObject.name} on workstation");
                        
                        if (ingredient.CurrentState != IngredientState.Cut)
                        {
                            sliceableIngredient.Slice();
                            
                            if (audioSource != null && sliceSound != null)
                            {
                                audioSource.PlayOneShot(sliceSound, sliceVolume);
                            }
                        }
                    }
                }
            }
        }
    }

    private void ResetWorkstationState(WorkstationState state)
    {
        state.isSlicing = false;
        state.canSlice = true;
        state.activeTouch = null;
    }

    private void OnDisable()
    {
        workstationStates.Clear();
        activeTouches.Clear();
    }
}