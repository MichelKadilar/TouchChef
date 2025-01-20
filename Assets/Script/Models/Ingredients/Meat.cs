using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Meat : BaseIngredient, ISliceable
{
    [Header("Visuals")]
    [SerializeField] private GameObject stateVisualContainer;
    [SerializeField] private GameObject rawVisual;
    [SerializeField] private GameObject cutVisual;
    [SerializeField] private GameObject cookedVisual;
    [SerializeField] private GameObject burnedVisual;
    
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;       // Source audio pour la cuisson
    [SerializeField] private AudioClip cookingSound;        // Son de cuisson continu
    [SerializeField] private AudioClip burningSound;        // Son court quand ça brûle
    [SerializeField] private float burningVolume = 1f;      // Volume du son de brûlure
    private AudioSource burningAudioSource;                 // Source audio séparée pour la brûlure

    private Slider _slider;
    private bool isCooking = false;
    private bool hasBeenCooked = false; // Track if meat has ever reached cooked state

    [Header("Slice Options")]
    public int neededSlices = 5;
    private int currentSlice = 0;
    
    [Header("Cooking Options")]
    [SerializeField] private float burnTime = 6f;
    private float cookingTimer = 0f;
    
    protected void Awake()
    {
        if (stateVisualContainer == null)
        {
            Debug.LogError($"{gameObject.name}: stateVisualContainer is not assigned!");
            return;
        }

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

        // Initialiser l'AudioSource si nécessaire (cuisson)
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.loop = true;  // Pour le son de cuisson continu
            audioSource.playOnAwake = false;
        }

        // Créer une source audio séparée pour le son de brûlure
        burningAudioSource = gameObject.AddComponent<AudioSource>();
        burningAudioSource.loop = false;
        burningAudioSource.playOnAwake = false;
        burningAudioSource.volume = burningVolume;

        if (allowedProcesses == null)
        {
            allowedProcesses = new List<ProcessType>();
        }
        allowedProcesses.Clear();
        allowedProcesses.Add(ProcessType.Cut);
        
        base.Awake();
        UpdateVisual();
    }

    public void Slice()
    {
        Debug.Log("Slicing meat");
        currentSlice++;
        _slider.gameObject.SetActive(true);
        _slider.value = currentSlice;
        if (currentSlice >= neededSlices)
        {
            currentState = IngredientState.Cut;
            allowedProcesses.Clear();
            allowedProcesses.Add(ProcessType.Cook);
            _slider.gameObject.SetActive(false);
            NotifyActionProgress("cut");
        }
        UpdateVisual();
    }

    protected override IEnumerator ProcessingCoroutine(ProcessType processType)
    {
        if (processType == ProcessType.Cook)
        {
            isProcessing = true;
            cookingTimer = 0f;
            hasBeenCooked = false; // Reset the cooked flag at start
            // Démarrer avec l'effet de feu pour la viande crue
            GetCurrentWorkStation()?.UpdateCookingEffects(IngredientState.Cut);
            StartCoroutine(WaitForRemoval());
            
            if (cookingSound != null && audioSource != null)
            {
                audioSource.clip = cookingSound;
                audioSource.loop = true;
                audioSource.Play();
                isCooking = true;
            }
            
            while (cookingTimer < burnTime && GetCurrentWorkStation()?.GetStationType() == ProcessType.Cook)
            {
                cookingTimer += Time.deltaTime;
                float progress = cookingTimer / processingTime;
                
                if (cookingTimer >= processingTime && currentState != IngredientState.Cooked)
                {
                    hasBeenCooked = true; // Mark that meat has reached cooked state
                    CompleteProcessing(ProcessType.Cook);
                    // Mettre à jour l'effet de feu pour l'état cuit
                    GetCurrentWorkStation()?.UpdateCookingEffects(IngredientState.Cooked);
                }
                else if (cookingTimer >= burnTime && currentState != IngredientState.Burned)
                {
                    // Arrêter le son de cuisson
                    if (audioSource != null)
                    {
                        audioSource.Stop();
                        isCooking = false;
                    }

                    // Jouer le son de brûlure
                    if (burningSound != null && burningAudioSource != null)
                    {
                        burningAudioSource.PlayOneShot(burningSound, burningVolume);
                        yield return new WaitForSeconds(0.1f);
                    }
                    
                    CompleteProcessing(ProcessType.Cook);
                }
                
                OnProcessingProgress(progress);
                yield return null;
            }
            
            StopCookingSound();
            isProcessing = false;
        }
        else
        {
            yield return base.ProcessingCoroutine(processType);
        }
    }

    protected override void CompleteProcessing(ProcessType processType)
    {
        switch (processType)
        {
            case ProcessType.Cook:
                if (cookingTimer < burnTime)
                {
                    currentState = IngredientState.Cooked;
                    allowedProcesses.Clear();
                    isProcessing = false;
                    GetCurrentWorkStation()?.UpdateCookingEffects(currentState);
                }
                else if (cookingTimer >= burnTime && this.GetCurrentWorkStation()?.GetStationType() == ProcessType.Cook)
                {
                    // L'état passe à Burned (le son de brûlure a déjà été joué)
                    currentState = IngredientState.Burned;
                    allowedProcesses.Clear();
                    GetCurrentWorkStation()?.UpdateCookingEffects(currentState);
                }
                break;
        }
        UpdateVisual();
    }

    private IEnumerator WaitForRemoval()
    {
        float timeoutDuration = 40f;
        float elapsedTime = 0f;
        WorkStation initialStation = GetCurrentWorkStation();
        bool hasBeenMoved = false;

        while (elapsedTime < timeoutDuration)
        {
            WorkStation currentStation = GetCurrentWorkStation();
    
            if (currentStation == null)
            {
                hasBeenMoved = true;
                StopCookingSound();
                initialStation?.UpdateCookingEffects(IngredientState.Raw);
            }
            else if (hasBeenMoved && currentStation != initialStation)
            {
                initialStation?.UpdateCookingEffects(IngredientState.Raw);
                StopCookingSound();
                if (WorkstationManager.Instance != null && hasBeenCooked && currentState != IngredientState.Burned)
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
            currentState = IngredientState.Burned;
            allowedProcesses.Clear();
            UpdateVisual();
        }
        initialStation?.UpdateCookingEffects(IngredientState.Raw);
    }

    private void StopCookingSound()
    {
        if (audioSource != null && isCooking)
        {
            audioSource.Stop();
            isCooking = false;
        }
    }

    private void OnDestroy()
    {
        StopCookingSound();
    }

    private void UpdateVisual()
    {
        rawVisual.SetActive(currentState == IngredientState.Raw);
        cutVisual.SetActive(currentState == IngredientState.Cut);
        cookedVisual.SetActive(currentState == IngredientState.Cooked);
        burnedVisual.SetActive(currentState == IngredientState.Burned);
    }
}