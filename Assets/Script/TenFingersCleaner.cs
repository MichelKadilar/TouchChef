using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls; // Important pour les contr�les tactiles
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch; // D�finition explicite du type Touch
using System.Collections.Generic;

public class TenFingersCleaner : MonoBehaviour
{
    [SerializeField] private int requiredTouchCount = 10;
    [SerializeField] private float maxDistanceBetweenTouches = 100f;

    private Camera mainCamera;
    private bool isDetecting = false;

    // souris

    [SerializeField] private int requiredClicks = 10;
    [SerializeField] private float maxTimeBetweenClicks = 0.5f; // Temps maximum entre deux clics en secondes
    [SerializeField] private float maxClickRadius = 1f; // Rayon maximum autour du premier clic

    private List<Vector3> clickPositions;
    private float lastClickTime;
    private Vector3 firstClickPosition;
    private bool isTracking = false;

    // fin souris

    void Start()
    {
        // Activer le support tactile am�lior�
        UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Enable();
        mainCamera = Camera.main;

        //souris
        clickPositions = new List<Vector3>();
        lastClickTime = 0f;
    }

    void Update()
    {
        var activeTouches = Touch.activeTouches;

        List<Vector2> validTouchPositions = new List<Vector2>();

        foreach (var touch in activeTouches)
        {
            // Utiliser le bon type de TouchPhase
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Stationary ||
                touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                Vector3 worldPosition = GetWorldPosition(touch.screenPosition);
                if (IsPositionInsideObject(worldPosition))
                {
                    validTouchPositions.Add(touch.screenPosition);
                }
            }
        }

        if (validTouchPositions.Count == requiredTouchCount)
        {
            if (AreTouchesAdjacent(validTouchPositions))
            {
                if (!isDetecting)
                {
                    isDetecting = true;
                    OnTenFingersDetected();
                }
            }
        }
        else
        {
            isDetecting = false;
        }

        //souris

        if (Input.GetMouseButtonDown(0)) // D�tection du clic gauche
        {
            Vector3 worldPosition = GetWorldPosition(Input.mousePosition);

            // V�rifier si le clic est dans l'objet
            if (IsPositionInsideObject(worldPosition))
            {
                float currentTime = Time.time;

                // Si c'est le premier clic ou si le temps �coul� depuis le dernier clic est trop long
                if (!isTracking || (currentTime - lastClickTime) > maxTimeBetweenClicks)
                {
                    // R�initialiser
                    isTracking = true;
                    clickPositions.Clear();
                    firstClickPosition = worldPosition;
                }

                // V�rifier si le nouveau clic est dans le rayon autoris�
                if (Vector3.Distance(firstClickPosition, worldPosition) <= maxClickRadius)
                {
                    clickPositions.Add(worldPosition);
                    lastClickTime = currentTime;

                    Debug.Log($"Clic {clickPositions.Count}/{requiredClicks} enregistr�");

                    // V�rifier si nous avons atteint le nombre requis de clics
                    if (clickPositions.Count >= requiredClicks)
                    {
                        OnTenClicksDetected();
                        // R�initialiser
                        isTracking = false;
                        clickPositions.Clear();
                    }
                }
                else
                {
                    // Le clic est trop loin du pr�c�dent
                    isTracking = false;
                    clickPositions.Clear();
                    Debug.Log("Clic trop �loign� de la zone initiale, s�quence r�initialis�e");
                }
            }
        }

        // V�rifier si trop de temps s'est �coul� depuis le pr�c�dent clic
        if (isTracking && Time.time - lastClickTime > maxTimeBetweenClicks && clickPositions.Count > 0)
        {
            isTracking = false;
            clickPositions.Clear();
            Debug.Log("Temps �coul� trop long, s�quence r�initialis�e");
        }
    }

    private void OnTenClicksDetected()
    {
        Debug.Log("10 clics cons�cutifs d�tect�s dans la zone !");
        
        Renderer renderer = objectsCollided[0].GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Random.ColorHSV();
        }
    }

    private bool AreTouchesAdjacent(List<Vector2> touchPositions)
    {
        touchPositions.Sort((a, b) => a.x.CompareTo(b.x));

        for (int i = 0; i < touchPositions.Count - 1; i++)
        {
            float distance = Vector2.Distance(touchPositions[i], touchPositions[i + 1]);
            if (distance > maxDistanceBetweenTouches)
            {
                return false;
            }
        }

        return true;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            return hit.point;
        }

        return Vector3.zero;
    }

    private bool IsPositionInsideObject(Vector3 worldPosition)
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            return collider.bounds.Contains(worldPosition);
        }

        return false;
    }

    private bool isPositionAboveObject(Vector3 worldPosition)
    {
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            Vector3 min = collider.bounds.min;
            Vector3 max = collider.bounds.max;

            return (worldPosition.x >= min.x && worldPosition.x <= max.x) &&
           (worldPosition.y >= min.y && worldPosition.y <= max.y) &&
           (worldPosition.z >= max.z);
        }
        return false;
    }

        private void OnTenFingersDetected()
        {
            Debug.Log("10 doigts d�tect�s simultan�ment dans la zone !");

            /*Renderer renderer = objectsCollided[0].GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = Random.ColorHSV();
            }*/
        }

        void OnDisable()
        {
            // D�sactiver le support tactile am�lior� quand le script est d�sactiv�
            UnityEngine.InputSystem.EnhancedTouch.EnhancedTouchSupport.Disable();
        }

    // D�tection de collision

    // Liste pour stocker les objets d�tect�s
    private List<Collider> objectsCollided = new List<Collider>();

    // Appel�e lorsqu'un objet entre en collision
    private void OnTriggerEnter(Collider other)
    {
        if (!objectsCollided.Contains(other))
        {
            objectsCollided.Add(other);
            Debug.Log("Objet entr� dans le box collider : " + other.name);
        }
    }

    // Appel�e tant qu'un objet reste dans le trigger
    private void OnTriggerStay(Collider other)
    {
        //Debug.Log("Objet dans le box collider : " + other.name);
    }

    // Appel�e lorsqu'un objet sort du trigger
    private void OnTriggerExit(Collider other)
    {
        if (objectsCollided.Contains(other))
        {
            objectsCollided.Remove(other);
            Debug.Log("Objet sorti du box collider : " + other.name);
        }
    }

}