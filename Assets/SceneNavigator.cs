using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Nombre total de scènes dans le jeu")]
    [SerializeField] private int totalScenes = 12;
    
    [Tooltip("Activer/désactiver la navigation en boucle (Scene12 -> Scene1)")]
    [SerializeField] private bool loopNavigation = false;
    
    [Header("Input Keys")]
    [SerializeField] private KeyCode nextSceneKey = KeyCode.RightArrow;
    [SerializeField] private KeyCode previousSceneKey = KeyCode.LeftArrow;

    private void Awake()
    {
        // S'assurer qu'il n'y a qu'une seule instance du SceneNavigator
        if (FindObjectsOfType<SceneNavigator>().Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        // Garder le SceneNavigator entre les scènes
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // Navigation vers la scène suivante
        if (Input.GetKeyDown(nextSceneKey))
        {
            NavigateToNextScene();
        }
        // Navigation vers la scène précédente
        else if (Input.GetKeyDown(previousSceneKey))
        {
            NavigateToPreviousScene();
        }
    }

    private void NavigateToNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex;

        if (currentSceneIndex >= totalScenes)
        {
            if (loopNavigation)
            {
                nextSceneIndex = 1; // Retour à la première scène
            }
            else
            {
                Debug.Log("Dernière scène atteinte");
                return;
            }
        }
        else
        {
            nextSceneIndex = currentSceneIndex + 1;
        }

        LoadScene(nextSceneIndex);
    }

    private void NavigateToPreviousScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int previousSceneIndex;

        if (currentSceneIndex <= 1)
        {
            if (loopNavigation)
            {
                previousSceneIndex = totalScenes; // Aller à la dernière scène
            }
            else
            {
                Debug.Log("Première scène atteinte");
                return;
            }
        }
        else
        {
            previousSceneIndex = currentSceneIndex - 1;
        }

        LoadScene(previousSceneIndex);
    }

    private void LoadScene(int sceneIndex)
    {
        if (sceneIndex >= 1 && sceneIndex <= totalScenes)
        {
            try
            {
                SceneManager.LoadScene(sceneIndex);
                Debug.Log($"Chargement de la scène {sceneIndex}");
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Erreur lors du chargement de la scène {sceneIndex}: {e.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Index de scène invalide: {sceneIndex}");
        }
    }
}