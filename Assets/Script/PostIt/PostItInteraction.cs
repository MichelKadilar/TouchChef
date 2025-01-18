using UnityEngine;
using uPIe;

public class PostItInteraction : MonoBehaviour
{
    private uPIeMenu2 playersMenu;
    private bool isHolding;
    private float holdTimer;
    private readonly float holdDuration = 1f; // Durée de maintien nécessaire
    private string taskId;

    void Start()
    {
        // Récupérer la référence du menu depuis le PostItManager
        playersMenu = FindObjectOfType<PostItManager>().playersMenu;
        if (playersMenu != null)
        {
            playersMenu.gameObject.SetActive(false); // Cacher le menu au départ
        }
    }

    void OnMouseDown()
    {
        isHolding = true;
        holdTimer = 0f;
    }
    
    public void SetTaskId(string id)
    {
        taskId = id;
    }

    void OnMouseUp()
    {
        int selectedPieceId = playersMenu.SelectedPieceId;
        isHolding = false;
        if (playersMenu != null && playersMenu.gameObject.activeSelf)
        {
            // Vérifier si on est sur un bouton
            Vector2 mousePosition = Input.mousePosition;
            Vector2 menuPosition = playersMenu.transform.position;
            float distance = Vector2.Distance(mousePosition, menuPosition);
            float threshold = 140f; // Distance seuil pour valider le clic
            float minDistance = 20f; // Distance minimale pour éviter les clics accidentels et aussi pouvoir relacher au milieu

            if (distance < threshold && distance > minDistance && selectedPieceId != -1)
            {
                var postItManager = FindObjectOfType<PostItManager>();
                postItManager.PlayerSelectedFromRadialMenu(selectedPieceId, taskId);
            }
            
            // Reset la sélection et cache le menu
            playersMenu.SelectedPieceId = -1;
            playersMenu.CurrentlyActiveOption = null;
            playersMenu.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (isHolding)
        {
            //Debug.Log("Holding for " + holdTimer + " seconds...");
            holdTimer += Time.deltaTime;
            if (holdTimer >= holdDuration && playersMenu != null)
            {
                // Redimensionner tous les boutons du menu radial
                foreach (var button in playersMenu.MenuOptions)
                {
                    RectTransform rectTransform = button.GetComponent<RectTransform>();
                    if (rectTransform != null)
                    {
                        rectTransform.sizeDelta = new Vector2(128, 128); 
                    }
                }
                // Positionner et afficher le menu
                playersMenu.gameObject.SetActive(true);
                playersMenu.transform.position = Camera.main.WorldToScreenPoint(transform.position);
            }
        }
    }
}