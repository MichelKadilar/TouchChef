using UnityEngine;
using UnityEngine.InputSystem;

public class WashDetector : MonoBehaviour
{
    [Header("Wash Configuration")]
    [SerializeField] private LayerMask ingredientLayer;
    [SerializeField] private float raycastDistance = 100f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip washSound;
    [SerializeField] private float washVolume = 1f;

    [Header("Debug")]
    [SerializeField] private bool debugMode = true;

    private Camera mainCamera;
    private bool isWashing = false;

    private void Awake()
    {
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("WashDetector: Main Camera not found!");
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
        }
    }

    private void Update()
    {
        // Mettre à jour la référence de la caméra si nécessaire
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null)
            {
                Debug.LogWarning("WashDetector: Waiting for camera...");
                return;
            }
        }

        HandleWashInput();
    }

    private void HandleWashInput()
    {
        if (Touchscreen.current != null)
        {
            // Gestion tactile
            foreach (var touch in Touchscreen.current.touches)
            {
                int touchId = touch.touchId.ReadValue();
                Vector2 position = touch.position.ReadValue();

                switch (touch.phase.ReadValue())
                {
                    case UnityEngine.InputSystem.TouchPhase.Began:
                        TryStartWash(position);
                        break;
                }
            }
        }
        else if (Mouse.current != null)
        {
            // Gestion souris
            if (Mouse.current.middleButton.wasPressedThisFrame)
            {
                Vector2 mousePosition = Mouse.current.position.ReadValue();
                TryStartWash(mousePosition);
            }
        }
    }

    private void TryStartWash(Vector2 position)
    {
        if (mainCamera == null)
        {
            Debug.LogWarning("WashDetector: Camera not available for wash operation");
            return;
        }

        if (debugMode) Debug.Log("Tentative de lavage à la position: " + position);
        
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, raycastDistance, ingredientLayer);

        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (var hit in hits)
        {
            var ingredient = hit.collider.GetComponent<BaseIngredient>();
            if (ingredient == null)
            {
                ingredient = hit.collider.GetComponentInParent<BaseIngredient>();
            }

            if (ingredient != null)
            {
                WorkStation workStation = ingredient.GetCurrentWorkStation();
                if (workStation != null && workStation.GetStationType() == ProcessType.Wash && 
                    ingredient is IWashable washable && !isWashing)
                {
                    isWashing = true;
                    
                    // Effectuer le lavage
                    washable.StartWash();
                    washable.DoWash();
                    
                    // Jouer le son
                    if (audioSource != null && washSound != null)
                    {
                        audioSource.PlayOneShot(washSound, washVolume);
                    }
                    
                    // Arrêter le lavage après un court délai
                    StartCoroutine(StopWashAfterDelay(washable, 0.1f));
                    
                    if (debugMode) Debug.Log($"Lavage effectué sur {ingredient.name}");
                    break;
                }
            }
        }
    }

    private System.Collections.IEnumerator StopWashAfterDelay(IWashable washable, float delay)
    {
        yield return new WaitForSeconds(delay);
        washable.StopWash();
        isWashing = false;
    }
}