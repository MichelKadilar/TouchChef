using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System;

public class WorkstationManager : MonoBehaviour
{
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

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip stationAssignedSound;
    [SerializeField] private AudioClip stationUnavailableSound;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
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

    public void Initialize()
    {
        Debug.Log("WorkstationManager: Starting initialization...");
        if (allWorkstations == null || allWorkstations.Count == 0)
        {
            InitializeWorkstations();
        }
        else
        {
            Debug.Log("WorkstationManager: Already initialized with " + allWorkstations.Count + " stations");
        }
        
        foreach (var station in allWorkstations)
        {
            var highlight = station.GetComponent<WorkStationHighlight>();
            if (highlight != null)
            {
                highlight.SetHighlight(false);
            }
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
        Debug.Log($"HandleActiveTask: Début du traitement du message");
    
        if (message?.assignedTask?.cook == null)
        {
            Debug.LogError("HandleActiveTask: Message invalide - assignedTask ou cook est null");
            Debug.Log($"Message reçu: {JsonUtility.ToJson(message)}");
            return;
        }

        string playerId = message.assignedTask.cook.deviceId;
        ProcessType requiredType = DetermineProcessType(message.assignedTask.workstation);
    
        Debug.Log($"HandleActiveTask: Traitement pour joueur {playerId}, type requis {requiredType}");
        
        if (playerWorkstations.TryGetValue(playerId, out WorkStation currentStation))
        {
            Debug.Log($"Le joueur {playerId} est déjà assigné à la station {currentStation.gameObject.name}");
        
            if (currentStation.GetStationType() == requiredType)
            {
                Debug.Log("Même type de station - Conservation de la station actuelle");
                return;
            }
        
            Debug.Log("Type différent - Libération de l'ancienne station");
            ReleaseWorkstation(playerId);
        }

        WorkStation availableStation = FindAvailableWorkstation(requiredType);
    
        if (availableStation != null)
        {
            Debug.Log($"Station trouvée: {availableStation.gameObject.name} pour le joueur {playerId}");
            AssignWorkstation(availableStation, playerId, message.assignedTask.cook.color);
            PlayAssignmentSound(true);
        }
        else
        {
            Debug.LogWarning($"Aucune station disponible de type {requiredType} pour le joueur {playerId}");
            PlayAssignmentSound(false);
            SendWorkstationUnavailableMessage(playerId);
        }

        UpdateTaskProgress(message, playerId);
    }

    private void PlayAssignmentSound(bool success)
    {
        if (audioSource != null)
        {
            AudioClip clip = success ? stationAssignedSound : stationUnavailableSound;
            if (clip != null)
            {
                audioSource.PlayOneShot(clip);
            }
        }
    }

    private void SendWorkstationUnavailableMessage(string playerId)
    {
        var errorMessage = new WebSocketMessage
        {
            type = "workstation_unavailable",
            from = "unity",
            to = playerId
        };
        ClientWebSocket.Instance?.SendMessage(JsonUtility.ToJson(errorMessage));
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

    private void UpdateTaskProgress(WebSocketTaskMessage message, string playerId)
    {
        var progressData = new TaskProgressData
        {
            playerId = playerId,
            taskId = message.assignedTask.taskId,
            currentProgress = 0,
            targetProgress = int.Parse(message.assignedTask.quantity)
        };
        activeTasks[playerId] = progressData;
    }

    private WorkStation FindAvailableWorkstation(ProcessType type)
    {
        Debug.Log($"FindAvailableWorkstation: Recherche d'une station de type {type}");
        
        if (allWorkstations == null || allWorkstations.Count == 0)
        {
            Debug.LogError("FindAvailableWorkstation: Liste des stations est null ou vide, réinitialisation...");
            InitializeWorkstations();
        }

        Debug.Log($"Nombre total de stations: {allWorkstations.Count}");
        
        foreach (var station in allWorkstations)
        {
            var isValid = station != null && 
                          !station.gameObject.name.ToLower().StartsWith("table") &&
                          station.GetStationType() == type;
            var isOccupied = workstationPlayers.ContainsKey(station);
        
            Debug.Log($"Station: {station?.gameObject.name ?? "null"}" +
                      $"\n - Type: {station?.GetStationType()}" +
                      $"\n - Est valide: {isValid}" +
                      $"\n - Est occupée: {isOccupied}");
        }

        var validStations = allWorkstations
            .Where(ws => ws != null)  // Vérifier que la station existe
            .Where(ws => !ws.gameObject.name.ToLower().StartsWith("table"))
            .Where(ws => ws.GetStationType() == type)
            .Where(ws => !workstationPlayers.ContainsKey(ws))
            .ToList();

        Debug.Log($"Nombre de stations valides trouvées: {validStations.Count}");

        if (!validStations.Any())
        {
            Debug.LogWarning($"Aucune station disponible de type {type}");
            return null;
        }

        var selectedStation = validStations.First();
        Debug.Log($"Station sélectionnée: {selectedStation.gameObject.name}");
    
        return selectedStation;
    }

    private void AssignWorkstation(WorkStation station, string playerId, string color)
    {
        Debug.Log($"Assigning station '{station.gameObject.name}' to player {playerId}");
        
        if (playerWorkstations.ContainsKey(playerId))
        {
            ReleaseWorkstation(playerId);
        }
        
        if (workstationPlayers.ContainsKey(station))
        {
            string currentPlayer = workstationPlayers[station];
            if (currentPlayer != playerId)
            {
                ReleaseWorkstation(currentPlayer);
            }
        }
        
        playerWorkstations[playerId] = station;
        workstationPlayers[station] = playerId;
        
        var highlight = station.GetComponent<WorkStationHighlight>();
        if (highlight != null)
        {
            if (ColorUtility.TryParseHtmlString(color, out Color stationColor))
            {
                highlight.SetHighlight(true, stationColor);
                Debug.Log($"Successfully set highlight color: {color} -> {stationColor}");
            }
            else
            {
                Debug.LogError($"Failed to parse color: {color}");
            }
        }
        else
        {
            Debug.LogError($"No WorkStationHighlight component found on station '{station.gameObject.name}'");
        }
    }

    public void HandleUnactiveTask(string playerId)
    {
        Debug.Log($"WorkstationManager: Désactivation de la tâche pour le joueur {playerId}");
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogError("WorkstationManager: HandleUnactiveTask reçu un playerId null ou vide");
            return;
        }

        ReleaseWorkstation(playerId);
    }

    private void ReleaseWorkstation(string playerId)
    {
        if (playerWorkstations.TryGetValue(playerId, out WorkStation station))
        {
            Debug.Log($"Releasing station '{station.gameObject.name}' from player {playerId}");
            
            // Disable highlight
            var highlight = station.GetComponent<WorkStationHighlight>();
            if (highlight != null)
            {
                highlight.SetHighlight(false);
            }

            // Remove from tracking dictionaries
            workstationPlayers.Remove(station);
            playerWorkstations.Remove(playerId);
        }
    }

    public void UpdateTaskProgress(WorkStation workstation, string actionType)
    {
        if (!workstationPlayers.TryGetValue(workstation, out string playerId))
        {
            Debug.LogWarning("Pas de joueur associé à cette workstation");
            return;
        }

        if (!activeTasks.TryGetValue(playerId, out TaskProgressData taskData))
        {
            Debug.LogWarning($"Pas de tâche active pour le joueur {playerId}");
            return;
        }
    
        if (!IsActionValidForTask(actionType, workstation.GetStationType().ToString()))
        {
            Debug.Log($"Action {actionType} ne correspond pas au type de workstation {workstation.GetStationType()}");
            return;
        }

        taskData.currentProgress++;
    
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

        if (taskData.currentProgress >= taskData.targetProgress)
        {
            Debug.Log($"Tâche {taskData.taskId} terminée pour le joueur {playerId}");
            activeTasks.Remove(playerId);
            ReleaseWorkstation(playerId);
        }
    }

    private bool IsActionValidForTask(string actionType, string workstationType)
    {
        switch (actionType.ToLower())
        {
            case "cut":
                return workstationType.Equals("Cut", StringComparison.OrdinalIgnoreCase);
            case "cook":
                return workstationType.Equals("Cook", StringComparison.OrdinalIgnoreCase);
            case "wash":
                return workstationType.Equals("Wash", StringComparison.OrdinalIgnoreCase);
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
}