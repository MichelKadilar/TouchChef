using UnityEngine;
using System.Collections;

public class TrashCan : MonoBehaviour
{
    public float rotationAngle = 45f;
    public float rotationSpeed = 2f;
    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    // Ajout des références pour les sons
    public AudioSource audioSource;
    public AudioClip openSound;
    public AudioClip closeSound;
    public AudioClip throwSound;

    private bool isAnimating = false;
    private Coroutine currentAnimation;
    public bool IsOpen => isOpen;

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, rotationAngle, 0) * closedRotation;
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    public void SetOpen(bool open)
    {
        if (isOpen == open) return;
        
        isOpen = open;
        if (currentAnimation != null)
        {
            StopCoroutine(currentAnimation);
        }
        
        if (open)
        {
            currentAnimation = StartCoroutine(AnimateOpen());
        }
        else
        {
            currentAnimation = StartCoroutine(AnimateClose());
        }
    }

    private IEnumerator AnimateOpen()
    {
        isAnimating = true;
        if (openSound != null)
        {
            audioSource.PlayOneShot(openSound);
        }

        float elapsed = 0;
        Quaternion startRotation = transform.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, openRotation, elapsed);
            yield return null;
        }

        transform.rotation = openRotation;
        isAnimating = false;
    }

    private IEnumerator AnimateClose()
    {
        isAnimating = true;
        if (closeSound != null)
        {
            audioSource.PlayOneShot(closeSound);
        }

        float elapsed = 0;
        Quaternion startRotation = transform.rotation;

        while (elapsed < 1f)
        {
            elapsed += Time.deltaTime * rotationSpeed;
            transform.rotation = Quaternion.Lerp(startRotation, closedRotation, elapsed);
            yield return null;
        }

        transform.rotation = closedRotation;
        isAnimating = false;
    }

    public void PlayThrowSound()
    {
        if (throwSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(throwSound);
            StartCoroutine(CloseAfterThrow());
        }
    }

    private IEnumerator CloseAfterThrow()
    {
        yield return new WaitForSeconds(2f); // Attendre 1 seconde
        SetOpen(false); // Déclenche l'animation de fermeture
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                SetOpen(!isOpen);
            }
        }
    }
}