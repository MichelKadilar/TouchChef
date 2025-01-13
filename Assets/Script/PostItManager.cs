using UnityEngine;
using UnityEngine.UI;
using TMPro;
using NativeWebSocket;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class TaskMessage
{
    public string type;
    public string task;
    public string taskId;
    public string taskIcons;
    public string from;
    public string to;
}

public class PostItManager : MonoBehaviour
{
    [Header("Configuration")]
    public GameObject postItPrefab;
    public Transform[] spawnPoints;

    private WebSocket websocket;
    private Dictionary<string, GameObject> activePostIts = new Dictionary<string, GameObject>();
    private HashSet<Transform> occupiedSpawnPoints = new HashSet<Transform>(); // Pour suivre les points occupés

    async void Start()
    {
        websocket = new WebSocket("ws://websocket.chhilif.com/ws");
        websocket.OnMessage += HandleMessage;
        await websocket.Connect();
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif
    }

    private void HandleMessage(byte[] bytes)
    {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        TaskMessage taskMessage = JsonUtility.FromJson<TaskMessage>(message);

        if (taskMessage.type == "table_task")
        {
            CreatePostIt(taskMessage);
        }
    }

    private void CreatePostIt(TaskMessage task)
    {
        // Si un post-it avec cet ID existe déjà, on le détruit
        if (activePostIts.ContainsKey(task.taskId))
        {
            // Trouver et libérer le point de spawn le plus proche
            GameObject oldPostIt = activePostIts[task.taskId];
            Transform closestPoint = FindClosestSpawnPoint(oldPostIt.transform.position);
            if (closestPoint != null)
            {
                occupiedSpawnPoints.Remove(closestPoint);
            }
            
            Destroy(oldPostIt);
            activePostIts.Remove(task.taskId);
        }

        // Choisir un point de spawn aléatoire
        Vector3 spawnPosition = GetRandomSpawnPoint();

        // Créer le post-it
        GameObject postIt = Instantiate(postItPrefab, spawnPosition, Quaternion.identity);
        
        // Configurer le TextMeshPro pour les icônes
        TMP_Text iconsText = postIt.GetComponentInChildren<TMP_Text>();
        
        if (iconsText == null)
        {
            Debug.LogError("No TextMeshPro component found in children of: " + postIt.name);
            return;
        }
        Debug.Log("TextMeshPro: " + iconsText);
        if(iconsText != null) {
            iconsText.text = task.taskIcons;
        }
        
        // Ajouter le post-it à notre dictionnaire
        activePostIts.Add(task.taskId, postIt);
    }
    
    private Transform FindClosestSpawnPoint(Vector3 position)
    {
        float minDistance = float.MaxValue;
        Transform closestPoint = null;

        foreach (Transform point in spawnPoints)
        {
            float distance = Vector3.Distance(position, point.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closestPoint = point;
            }
        }

        return closestPoint;
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

    private async void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}