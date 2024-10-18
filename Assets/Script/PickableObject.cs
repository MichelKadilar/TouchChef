using UnityEngine;

public class PickableObject : MonoBehaviour, IPickable
{
    private bool isPickedUp = false;
    private Camera mainCamera;
    
    public bool isReadyToBeClickable = false;

    void Start()
    {
        mainCamera = Camera.main;
    }

    public void OnPick()
    {
        Debug.Log("Object picked up: " + gameObject.name); // Log lorsque l'objet est ramassé
        isPickedUp = true;
    }

    public void OnDrop()
    {
        Debug.Log("Object dropped: " + gameObject.name); // Log lorsque l'objet est relâché
        isPickedUp = false;
    }

    public void OnMove(Vector3 newPosition)
    {
        if (isPickedUp)
        {
            transform.position = newPosition;
        }
    }

    void Update()
    {
        if (isPickedUp)
        {
            if (Input.GetMouseButton(0))
            {
                // Mouvement de l'objet avec la souris
                Vector3 mousePos = Input.mousePosition;
                mousePos.z = mainCamera.WorldToScreenPoint(transform.position).z;
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
                OnMove(worldPos);
            }
            else
            {
                // Si l'utilisateur relâche le clic, on tente de déposer l'objet
                Debug.Log("Mouse button released, trying to drop object.");
                TryDropObject();
            }
        }
    }

    void TryDropObject()
    {
        // On fait un raycast pour vérifier si on relâche au-dessus d'un WorkStation
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        Debug.Log("Raycast initiated at mouse position.");
        
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Debug.Log("Raycast hit: " + hit.collider.name);

            // Vérifie si on a bien touché un objet avec un script WorkStation
            WorkStation station = hit.collider.GetComponent<WorkStation>();
            if (station != null)
            {
                Debug.Log("Hit WorkStation: " + station.gameObject.name);

                // Si la station est disponible, on place l'objet
                if (station.TryPlaceIngredient(gameObject))
                {
                    Debug.Log("Ingredient placed successfully on the station.");
                    OnDrop(); // Libère l'objet une fois placé
                    isReadyToBeClickable = true; // Set this to true after a successful drop
                    return; // Quitte la fonction car l'objet a été déposé correctement
                }
                else
                {
                    Debug.LogWarning("WorkStation is occupied, can't place the object.");
                }
            }
            else
            {
                Debug.LogWarning("Raycast hit an object without WorkStation script.");
            }
        }
        else
        {
            Debug.LogWarning("Raycast did not hit any object.");
        }

        // Si on n'a pas trouvé d'endroit valide, l'objet est détruit
        Debug.LogError("No valid drop location found. Destroying object.");
        Destroy(gameObject);
    }
}
