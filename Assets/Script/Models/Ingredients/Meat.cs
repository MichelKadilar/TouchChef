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
        Debug.Log($"CURRENT STATE MEAT : {currentState}");
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
                    CompleteProcessing(ProcessType.Cook);
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
                        
                        // Attendre un peu que le son démarre
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
                    StartCoroutine(WaitForRemoval());
                }
                else if (cookingTimer >= burnTime && this.GetCurrentWorkStation()?.GetStationType() == ProcessType.Cook)
                {
                    // L'état passe à Burned (le son de brûlure a déjà été joué)
                    currentState = IngredientState.Burned;
                    allowedProcesses.Clear();
                }
                break;
            
            // Les autres cas restent inchangés...
        }
        UpdateVisual();
    }

    private IEnumerator WaitForRemoval()
    {
        float timeoutDuration = 3f;
        float elapsedTime = 0f;
        WorkStation initialStation = GetCurrentWorkStation();
        string originalPlayerId = initialStation?.GetAssignedPlayerId();
        bool hasBeenMoved = false;

        if (string.IsNullOrEmpty(originalPlayerId))
        {
            yield break;
        }

        while (elapsedTime < timeoutDuration)
        {
            WorkStation currentStation = GetCurrentWorkStation();
    
            if (currentStation == null)
            {
                hasBeenMoved = true;
                StopCookingSound();  // Arrêter le son quand retiré de la station
            }
            else if (hasBeenMoved && currentStation != initialStation)
            {
                StopCookingSound();  // Arrêter le son si déplacé vers une autre station
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
            currentState = IngredientState.Burned;
            allowedProcesses.Clear();
            UpdateVisual();
        }
    }

    private void StopCookingSound()
    {
        if (audioSource != null && isCooking)
        {
            audioSource.Stop();
            isCooking = false;
        }

        // Si le son de brûlure jouait, il s’agit d’un PlayOneShot, donc rien à stopper spécifiquement
        // (PlayOneShot ne se base pas sur un clip actif, c’est un son lancé ponctuellement)
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
