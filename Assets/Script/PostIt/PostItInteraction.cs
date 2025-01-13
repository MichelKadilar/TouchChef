using UnityEngine;
using uPIe;

public class PostItInteraction : MonoBehaviour
{
    private uPIeMenu2 playersMenu;
    private Canvas canvas;
    private bool isHolding;
    private float holdTimer;
    private readonly float holdDuration = 1f; // Durée de maintien nécessaire

    void Start()
    {
        // Récupérer la référence du menu depuis le PostItManager
        playersMenu = FindObjectOfType<PostItManager>().playersMenu;
        if (playersMenu != null)
        {
            playersMenu.gameObject.SetActive(false); // Cacher le menu au départ
            canvas = playersMenu.GetComponentInParent<Canvas>();
        }
    }

    void OnMouseDown()
    {
        isHolding = true;
        holdTimer = 0f;
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
            float threshold = 130f; // Ajustez cette valeur selon vos besoins
        
            if (distance < threshold)
            {
                if (selectedPieceId != -1)
                {
                    var postItManager = FindObjectOfType<PostItManager>();
                    postItManager.PlayerSelectedFromRadialMenu(selectedPieceId);
                }
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