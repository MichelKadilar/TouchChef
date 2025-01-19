using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using uPIe;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class PostItManager : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject postItPrefab;
    public Transform[] spawnPoints;
    public uPIeMenu2 playersMenu;
    public float outlineTicketWidth = 4f;
    
    [Header("UI Player 1")]
    public Image colorPlayer1;
    public Image avatarPlayer1;
    [Header("UI Player 2")]
    public Image colorPlayer2;
    public Image avatarPlayer2;
    [Header("UI Player 3")]
    public Image colorPlayer3;
    public Image avatarPlayer3;
    [Header("UI Player 4")]
    public Image colorPlayer4;
    public Image avatarPlayer4;
    
    public Sprite[] avatars;
    public const int MAX_PLAYERS = 4;

    [Header("State")]

    public Dictionary<string, GameObject> activePostIts = new Dictionary<string, GameObject>(); // Pour suivre les post-its actifs
    
    private HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>(); // Pour suivre les points occupés

    
    void Start()
    {
        if (ClientWebSocket.Instance != null)
        {
            ClientWebSocket.Instance.OnTaskTableMessageReceived += HandleTaskMessage;
            ClientWebSocket.Instance.OnTasksLiskUpdated += HandleTasksListMessage;
            
            if (ClientWebSocket.Instance.tasks != null)
            {
                createPostItFromList(ClientWebSocket.Instance.tasks);
            }
        }
        else
        {
            Debug.LogError("ClientWebSocket instance not found!");
        }
    }
    private void HandleTaskMessage(WebSocketTaskTableMessage message)
    {
        if (message.type == "table_task")
        {
            CreatePostIt(message.taskId, message.taskIcons);
        }
    }
    
    private void HandleTasksListMessage(WebSocketTasksListMessage message)
    {
        if (message.type == "tasksList")
        {
            createPostItFromList(message.tasks);
        }
    }
    
    private void createPostItFromList(Task[] tasks)
    {
        foreach (var task in tasks)
        {
            CreatePostIt(task.id, task.icons);
        }
    }

    private void CreatePostIt(string taskId, string taskIcons)
    {
        // Si un post-it avec cet ID existe déjà, on ne fait rien
        if (activePostIts.ContainsKey(taskId)) return;

        // Choisir un point de spawn aléatoire
        Vector3 spawnPosition = GetRandomSpawnPoint();

        // Créer le post-it
        GameObject postIt = Instantiate(postItPrefab, spawnPosition, Quaternion.identity, transform);
        var postItInteraction = postIt.AddComponent<PostItInteraction>();
        postItInteraction.SetTaskId(taskId);
        
        ClientWebSocket.Instance.OnTaskTableMessageReceived += HandleTaskMessage;

        for (int i = 0; i < MAX_PLAYERS; i++)
        {
            if (i >= ClientWebSocket.Instance.players.Length)
            {
                DisablePlayer(i);
                continue;
            }
            EnablePLayer(i);
        }
        
        // Configurer le TextMeshPro pour les icônes
        TMP_Text iconsText = postIt.GetComponentInChildren<TMP_Text>();
        
        if (iconsText == null)
        {
            Debug.LogError("No TextMeshPro component found in children of: " + postIt.name);
            return;
        }
        if (iconsText != null) {
            iconsText.text = ConvertEmojiToSprite(taskIcons);
        }
        
        // Ajouter le post-it à notre dictionnaire
        activePostIts.Add(taskId, postIt);
        Debug.Log("PostItManager: PostIt created with ID: " + taskId + " at position: " + spawnPosition + " created successfully.");
    }
    
    private void EnablePLayer(int player)
    {
        Player[] players = ClientWebSocket.Instance.players;
        ColorUtility.TryParseHtmlString(players[player].color, out Color color);
        switch (player)
        {
            case 0:
                colorPlayer1.color = color;
                avatarPlayer1.sprite = avatars[int.Parse(players[player].avatar)-1];
                break;
            case 1:
                colorPlayer2.color = color;
                avatarPlayer2.sprite = avatars[int.Parse(players[player].avatar)-1];
                break;
            case 2:
                colorPlayer3.color = color;
                avatarPlayer3.sprite = avatars[int.Parse(players[player].avatar)-1];
                break;
            case 3:
                colorPlayer4.color = color;
                avatarPlayer4.sprite = avatars[int.Parse(players[player].avatar)-1];
                break;
        }
    }
    
    private void DisablePlayer(int player)
    {
        switch (player)
        {
            case 0:
                colorPlayer1.transform.parent.gameObject.SetActive(false);
                break;
            case 1:
                colorPlayer2.transform.parent.gameObject.SetActive(false);
                break;
            case 2:
                colorPlayer3.transform.parent.gameObject.SetActive(false);
                break;
            case 3:
                colorPlayer4.transform.parent.gameObject.SetActive(false);
                break;
        }
    }
    
    private string ConvertEmojiToSprite(string input)
    {
        Dictionary<string, int> emojiToSpriteIndex = new Dictionary<string, int>
        {
            {"\ud83d\udd2a", 0}, // couteau
            {"\ud83c\udf45", 1}, // tomato
            {"\ud83c\udf54", 2}, // burger
            {"\ud83e\uddc0", 3}, // cheese
            {"\ud83e\udd6c", 4}, // salad
            {"\ud83d\udd25", 5}, // fire
            {"\ud83e\udd69", 6}, // meat
            {"\ud83d\udca6", 7}, // water
        };

        string output = input;
        foreach (var emoji in emojiToSpriteIndex)
        {
            output = output.Replace(emoji.Key, $"<sprite={emoji.Value}>");
        }
        return output;
    }

    private Vector3 GetRandomSpawnPoint()
    {
        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("No spawn points defined!");
            return Vector3.zero;
        }

        // Créer une liste des points disponibles
        var availablePoints = spawnPoints.Where(point => !occupiedSpawnPoints.Contains(point)).ToList();

        if (availablePoints.Count == 0)
        {
            Debug.LogWarning("All spawn points are occupied! Reusing a random point.");
            // Si tous les points sont occupés, on prend un point au hasard
            return spawnPoints[Random.Range(0, spawnPoints.Length)].position;
        }

        // Sélectionner un point aléatoire parmi les disponibles
        Transform selectedPoint = availablePoints[Random.Range(0, availablePoints.Count)];
        occupiedSpawnPoints.Add(selectedPoint); // Marquer le point comme occupé
        return selectedPoint.position;
    }
    
    public void PlayerSelectedFromRadialMenu(int selectedPieceId, string taskId)
    {
        Player[] players = ClientWebSocket.Instance.players;
        Debug.Log("PostItManager: Player " + selectedPieceId + " selected from radial menu.");
        if (selectedPieceId >= players.Length || selectedPieceId < 0) return;
    
        // Créer l'objet message
        var message = new AssignTaskMessage
        {
            type = "assign_task",
            taskId = taskId,
            playerId = players[selectedPieceId].deviceId,
            from = "table",
            to = "angular"
        };

        // Convertir en JSON et envoyer
        string jsonMessage = JsonUtility.ToJson(message);
        if (ClientWebSocket.Instance != null)
        {
            ClientWebSocket.Instance.SendMessage(jsonMessage);

            Debug.Log("PostItManager: Player " + selectedPieceId + " selected for task: " + taskId);
            if (activePostIts.ContainsKey(taskId))
            {
                GameObject postIt = activePostIts[taskId];
                Debug.Log("activePostIts string" + JsonUtility.ToJson(activePostIts));
                
                // Obtenir ou ajouter le composant Outline
                Outline outline = postIt.GetComponent<Outline>();
                if (outline == null)
                {
                    outline = postIt.AddComponent<Outline>();
                }

                Debug.Log("PostItMananger: Outline Player color: " + players[selectedPieceId].color);

                // Convertir la couleur du joueur de format hexadécimal en Color
                if (ColorUtility.TryParseHtmlString(players[selectedPieceId].color, out Color playerColor))
                {
                    // Configurer l'outline
                    outline.OutlineMode = Outline.Mode.OutlineAll;
                    outline.OutlineColor = playerColor;
                    outline.OutlineWidth = outlineTicketWidth; 
                    outline.enabled = true;
                    Debug.Log("PostItManager: Outline enabled for player " + selectedPieceId);
                }
            }
        }
        else
        {
            Debug.LogError("[PostItManager] ClientWebSocket instance not found!");
        }
    }

    private async void OnDestroy()
    {
        if (ClientWebSocket.Instance != null)
        {
            ClientWebSocket.Instance.OnTaskTableMessageReceived -= HandleTaskMessage;
        }
    }
    
    
}

[System.Serializable]
public class AssignTaskMessage
{
    public string type;
    public string taskId;
    public string playerId;
    public string from;
    public string to;
}