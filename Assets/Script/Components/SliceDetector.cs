using UnityEngine;
using UnityEngine.InputSystem;

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
    private bool isSlicing = false;
    private bool canSlice = true;
    private float lastSliceTime = 0f;
    private int? activeTouch = null;
    private BaseIngredient currentIngredient = null;

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
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();
                var phase = touch.phase.ReadValue();

                switch (phase)
                {
                    case UnityEngine.InputSystem.TouchPhase.Began when activeTouch == null:
                        activeTouch = touchId;
                        if (canSlice && CheckTimeBetweenSlices())
                        {
                            TryStartSlice(touch.position.ReadValue());
                        }
                        break;

                    case UnityEngine.InputSystem.TouchPhase.Ended:
                    case UnityEngine.InputSystem.TouchPhase.Canceled:
                        if (touchId == activeTouch)
                        {
                            ResetSliceState();
                            activeTouch = null;
                        }
                        break;
                }
            }

            // Si aucun toucher n'est actif, réinitialiser l'état
            if (Touchscreen.current.touches.Count == 0)
            {
                ResetSliceState();
                activeTouch = null;
            }
        }
        else if (Mouse.current != null)
        {
            if (Mouse.current.rightButton.wasPressedThisFrame && 
                canSlice && 
                CheckTimeBetweenSlices())
            {
                TryStartSlice(Mouse.current.position.ReadValue());
            }
            else if (Mouse.current.rightButton.wasReleasedThisFrame)
            {
                ResetSliceState();
            }
        }
    }

    private bool CheckTimeBetweenSlices()
    {
        float currentTime = Time.time;
        if (currentTime - lastSliceTime >= minTimeBetweenSlices)
        {
            lastSliceTime = currentTime;
            return true;
        }
        return false;
    }

    private void TryStartSlice(Vector2 position)
    {
        if (mainCamera == null || isSlicing) return;

        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.SphereCastAll(ray, 1.5f, raycastDistance, ingredientLayer);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        
            GameObject clickedObject = hits[0].collider.gameObject;
            BaseIngredient ingredient = clickedObject.GetComponent<BaseIngredient>();

            if (ingredient != null && 
                ingredient is ISliceable sliceableIngredient && 
                ingredient.GetCurrentWorkStation()?.GetStationType() == ProcessType.Cut)
            {
                // Si c'est un nouvel ingrédient, réinitialiser complètement l'état
                if (currentIngredient != ingredient)
                {
                    CompleteReset();
                    currentIngredient = ingredient;
                }

                isSlicing = true;
                canSlice = false;

                if (debugMode) Debug.Log($"Slicing ingredient: {clickedObject.name}");
                
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

    private void ResetSliceState()
    {
        isSlicing = false;
        canSlice = true;
    }

    private void CompleteReset()
    {
        ResetSliceState();
        activeTouch = null;
        lastSliceTime = 0f;
        currentIngredient = null;
    }

    private void OnDisable()
    {
        CompleteReset();
    }
}