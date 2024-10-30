using System.Linq;
using UnityEngine;

public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    private TouchInputActions touchActions;
    private bool isPressed = false;
    private int currentTouchId = 0;

    [SerializeField] private float touchRadius = 0.5f; // Pour une meilleure détection
    [SerializeField] private LayerMask interactableLayer; // Pour filtrer les objets interactables

    private void Awake()
    {
        Debug.Log("TouchManager: Awake");
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }

        touchActions = new TouchInputActions();
        SetupInputActions();
    }

    private void SetupInputActions()
    {
        touchActions.Touch.PrimaryTouch.started += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.started triggered");
            OnTouchStarted();
        };
        
        touchActions.Touch.PrimaryTouch.canceled += ctx => {
            Debug.Log("TouchManager: PrimaryTouch.canceled triggered");
            OnTouchEnded();
        };

        // Modification principale ici : moved devient performed
        touchActions.Touch.PrimaryTouchPosition.performed += ctx => {
            if (isPressed)
            {
                Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
                UpdateObjectPosition(position);
                // Ajout d'un debug pour vérifier que la position est mise à jour
                Debug.Log($"Touch position updated: {position}");
            }
        };
    }

    private void OnEnable()
    {
        touchActions.Enable();
    }

    private void OnDisable()
    {
        touchActions.Disable();
    }

    private void OnTouchStarted()
    {
        isPressed = true;
        Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
        HandleTouchBegan(currentTouchId, position);
    }

    private void OnTouchEnded()
    {
        if (isPressed)
        {
            isPressed = false;
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            HandleTouchEnded(currentTouchId, position);
        }
    }

    private void HandleTouchBegan(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchBegan - Position: {position}");
        Ray ray = mainCamera.ScreenPointToRay(position);
        Debug.DrawRay(ray.origin, ray.direction * 100f, Color.red, 5f);
        Debug.Log($"Ray Origin: {ray.origin}, Direction: {ray.direction}");
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, interactableLayer);
        Debug.Log($"Number of hits: {hits.Length}");

        if (hits.Length > 0)
        {
            // Trier les hits par distance pour prendre le plus proche
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                Debug.Log($"Hit object: {hit.collider.gameObject.name} at distance {hit.distance}");

                // Vérifier d'abord si c'est un panier
                var basket = hit.collider.GetComponent<Basket>();
                if (basket != null)
                {
                    basket.OnTouchDown(touchId, position);
                    return;
                }

                // Ensuite vérifier si c'est un objet ramassable
                var pickable = hit.collider.GetComponent<IPickable>();
                if (pickable != null && !pickable.IsBeingDragged)
                {
                    pickable.OnTouchPick(touchId);
                    return;
                }

                // Vérifier si c'est une WorkStation avec un ingrédient processable
                var workStation = hit.collider.GetComponent<WorkStation>();
                if (workStation != null)
                {
                    workStation.StartProcessing();
                    return;
                }
            }
        }
    }
    private void Start()
    {
        interactableLayer = 1 << 3; // Layer 3 est "Interactable"
        Debug.Log($"Layer mask configured: {interactableLayer.value}");
    }

    private void HandleTouchEnded(int touchId, Vector2 position)
    {
        Debug.Log($"TouchManager: HandleTouchEnded - Position: {position}");
        
        // Utilisation de l'interface IPickable
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == touchId && pickable.IsBeingDragged)
            {
                pickable.OnTouchDrop(touchId, position);
            }
        }
    }

    private void UpdateObjectPosition(Vector2 screenPosition)
    {
        Vector3 worldPosition = GetWorldPosition(screenPosition);
        
        // Debug pour voir la position calculée
        Debug.Log($"World position: {worldPosition}");
    
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == currentTouchId && pickable.IsBeingDragged)
            {
                pickable.OnTouchMove(currentTouchId, worldPosition);
                // Debug pour confirmer le mouvement
                Debug.Log($"Moving object to: {worldPosition}");
            }
        }
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        // Modification importante ici : calcul correct de la position monde
        float distanceFromCamera = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            distanceFromCamera
        ));
        
        // Pour garder la même profondeur Z que le basket
        worldPos.z = -10f; // Même Z que dans Basket.OnTouchDown
        
        return worldPos;
    }
}