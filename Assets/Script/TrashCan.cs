using UnityEngine;

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

    void Start()
    {
        closedRotation = transform.rotation;
        openRotation = Quaternion.Euler(0, rotationAngle, 0) * closedRotation;
        
        // S'assurer qu'on a un AudioSource
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && hit.transform == transform)
            {
                isOpen = !isOpen;
                
                // Jouer le son approprié
                if (isOpen && openSound != null)
                {
                    audioSource.PlayOneShot(openSound);
                }
                else if (!isOpen && closeSound != null)
                {
                    audioSource.PlayOneShot(closeSound);
                }
            }
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        float animationSpeed = isOpen ? rotationSpeed : rotationSpeed * 6f;
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, animationSpeed * Time.deltaTime);
    }
    
}