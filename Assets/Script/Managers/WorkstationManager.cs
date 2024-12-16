using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;

public class WorkstationManager : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stationAssignedSound;
    [SerializeField] private AudioClip stationUnavailableSound;
    private static WorkstationManager instance;
    public static WorkstationManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<WorkstationManager>();
            }
            return instance;
        }
    }

    private Dictionary<string, WorkStation> playerWorkstations = new Dictionary<string, WorkStation>();
    private List<WorkStation> allWorkstations;
    private Dictionary<string, TaskProgressData> activeTasks = new Dictionary<string, TaskProgressData>();
    private Dictionary<WorkStation, string> workstationPlayers = new Dictionary<WorkStation, string>();

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeWorkstations();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeWorkstations()
    {
        allWorkstations = new List<WorkStation>(FindObjectsOfType<WorkStation>());
        Debug.Log($"WorkstationManager: Initialized with {allWorkstations.Count} stations:");
        foreach (var station in allWorkstations)
        {
            Debug.Log($"- Station '{station.gameObject.name}' de type {station.GetStationType()}");
        }
    }

    public void HandleActiveTask(WebSocketTaskMessage message)
    {
        Debug.Log("WorkstationManager: Traitement d'une nouvelle tâche active");

        if (message.assignedTask == null || message.assignedTask.cook == null)
        {
            Debug.LogError("WorkstationManager: Message invalide reçu - assignedTask ou cook est null");
            return;
        }

        string playerId = message.assignedTask.cook.deviceId;
        ProcessType requiredType = DetermineProcessType(message.assignedTask.workstation);
        Debug.Log($"WorkstationManager: Tâche reçue - Joueur: {playerId}, Type requis: {requiredType}");

        if (playerWorkstations.TryGetValue(playerId, out WorkStation currentStation))
        {
            if (currentStation.GetStationType() == requiredType)
            {
                Debug.Log("WorkstationManager: Même type de station - Conservation de la station actuelle");
                return;
            }
            Debug.Log("WorkstationManager: Type différent - Libération de l'ancienne station");
            ReleaseWorkstation(playerId);
        }

        WorkStation availableStation = FindAvailableWorkstation(requiredType);
        if (availableStation != null)
        {
            Debug.Log($"WorkstationManager: Station disponible trouvée pour le type {requiredType}");
            AssignWorkstation(availableStation, playerId, message.assignedTask.cook.color);
            workstationPlayers[availableStation] = playerId;
            if (audioSource != null && stationAssignedSound != null)
            {
                audioSource.PlayOneShot(stationAssignedSound);
            }
        }
        else
        {
            if (audioSource != null && stationUnavailableSound != null)
            {
                audioSource.PlayOneShot(stationUnavailableSound);
            }
            var errorMessage = new WebSocketMessage
            {
                type = "workstation_unavailable",
                from = "unity",
                to = playerId
            };
            ClientWebSocket.Instance?.SendMessage(JsonUtility.ToJson(errorMessage));
        }

        var progressData = new TaskProgressData
        {
            playerId = playerId,
            taskId = message.assignedTask.taskId,
            currentProgress = 0,
            targetProgress = int.Parse(message.assignedTask.quantity)
        };
        activeTasks[playerId] = progressData;
    }

    private int ExtractTargetValue(string taskName)
    {
        try
        {
            string[] words = taskName.Split(' ');
            foreach (string word in words)
            {
                if (int.TryParse(word, out int number))
                {
                    return number;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Erreur lors de l'extraction de la valeur cible : {e.Message}");
        }
        return 1;
    }

    public void UpdateTaskProgress(WorkStation workstation, string actionType)
    {
        // Vérifier si le workstation est associé à un joueur
        if (!workstationPlayers.TryGetValue(workstation, out string playerId))
        {
            Debug.LogWarning("Pas de joueur associé à cette workstation");
            return;
        }

        // Vérifier si le joueur a une tâche active
        if (!activeTasks.TryGetValue(playerId, out TaskProgressData taskData))
        {
            Debug.LogWarning($"Pas de tâche active pour le joueur {playerId}");
            return;
        }
    
        // Vérifier si l'action correspond à la workstation de la tâche
        if (!IsActionValidForTask(actionType, workstation.GetStationType().ToString()))
        {
            Debug.Log($"Action {actionType} ne correspond pas au type de workstation {workstation.GetStationType()}");
            return;
        }

        taskData.currentProgress++;
    
        // Créer et envoyer le message de progression
        var progressMessage = new TaskProgressMessage
        {
            type = "taskProgress",
            from = "unity",
            to = "angular",
            progressData = new TaskProgressData
            {
                playerId = playerId,
                taskId = taskData.taskId,
                currentProgress = taskData.currentProgress,
                targetProgress = taskData.targetProgress
            }
        };

        string jsonMessage = JsonUtility.ToJson(progressMessage);
        Debug.Log($"Envoi du message de progression: {jsonMessage}");
        ClientWebSocket.Instance?.SendMessage(jsonMessage);

        // Vérifier si la tâche est terminée
        if (taskData.currentProgress >= taskData.targetProgress)
        {
            Debug.Log($"Tâche {taskData.taskId} terminée pour le joueur {playerId}");
            activeTasks.Remove(playerId);
        }
    }
    
    private bool IsActionValidForTask(string actionType, string workstationType)
    {
        switch (actionType.ToLower())
        {
            case "cut":
                return workstationType.Equals("Cut", System.StringComparison.OrdinalIgnoreCase);
            case "cook":
                return workstationType.Equals("Cook", System.StringComparison.OrdinalIgnoreCase);
            case "wash":
                return workstationType.Equals("Wash", System.StringComparison.OrdinalIgnoreCase);
            default:
                Debug.LogWarning($"Type d'action non reconnu: {actionType}");
                return false;
        }
    }

    public string GetPlayerIdForWorkstation(WorkStation workstation)
    {
        workstationPlayers.TryGetValue(workstation, out string playerId);
        return playerId;
    }

    public void HandleUnactiveTask(string playerId)
    {
        Debug.Log($"WorkstationManager: Désactivation de la tâche pour le joueur {playerId}");
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("WorkstationManager: HandleUnactiveTask reçu un playerId null ou vide");
            return;
        }

        if (playerWorkstations.ContainsKey(playerId))
        {
            ReleaseWorkstation(playerId);
            Debug.Log($"WorkstationManager: Station libérée pour le joueur {playerId}");
        }
        else
        {
            Debug.Log($"WorkstationManager: Aucune station trouvée pour le joueur {playerId}");
        }
    }

    private ProcessType DetermineProcessType(string workstation)
    {
        switch (workstation.ToLower())
        {
            case "grill":
                return ProcessType.Cook;
            case "planche":
                return ProcessType.Cut;
            case "evier":
                return ProcessType.Wash;
            default:
                Debug.LogWarning($"Unknown workstation type: {workstation}");
                return ProcessType.Cut;
        }
    }

    private WorkStation FindAvailableWorkstation(ProcessType type)
    {
        Debug.Log($"Recherche d'une station de type {type}. Stations actuelles:");
    
        // Filtrer d'abord les stations qui commencent par "table"
        var validStations = allWorkstations.Where(ws => 
            !ws.gameObject.name.ToLower().StartsWith("table")).ToList();

        foreach (var station in validStations)
        {
            bool isOccupied = playerWorkstations.ContainsValue(station);
            Debug.Log($"- Station '{station.gameObject.name}' : Type={station.GetStationType()}, Occupée={isOccupied}");
        }

        var availableStation = validStations.Find(ws => 
            !playerWorkstations.ContainsValue(ws) && 
            ws.GetStationType() == type);

        if (availableStation != null)
        {
            Debug.Log($"Station trouvée : '{availableStation.gameObject.name}'");
        }
        else
        {
            Debug.Log($"Aucune station disponible de type {type}");
        }

        return availableStation;
    }
    private void AssignWorkstation(WorkStation station, string playerId, string color)
    {
        Debug.Log($"Tentative d'assignation de la station '{station.gameObject.name}' au joueur {playerId}");
        playerWorkstations[playerId] = station;

        var highlight = station.GetComponent<WorkStationHighlight>();
        if (highlight != null)
        {
            Debug.Log($"Configuration du highlight pour la station '{station.gameObject.name}'");
            if (ColorUtility.TryParseHtmlString(color, out Color stationColor))
            {
                highlight.SetHighlight(true, stationColor);
                Debug.Log($"Couleur parsée avec succès: {color} -> {stationColor}");
            }
            else
            {
                Debug.LogError($"Échec du parse de la couleur: {color}");
            }
        }
        else
        {
            Debug.LogError($"Pas de composant WorkStationHighlight trouvé sur la station '{station.gameObject.name}'");
        }
    }

    private void ReleaseWorkstation(string playerId)
    {
        if (playerWorkstations.TryGetValue(playerId, out WorkStation station))
        {
            Debug.Log($"Libération de la station '{station.gameObject.name}' du joueur {playerId}");
            var highlight = station.GetComponent<WorkStationHighlight>();
            if (highlight != null)
            {
                highlight.SetHighlight(false);
                Debug.Log($"Highlight désactivé pour la station '{station.gameObject.name}'");
            }

            playerWorkstations.Remove(playerId);
        }
        else
        {
            Debug.Log($"Aucune station trouvée pour le joueur {playerId}");
        }
    }
}
