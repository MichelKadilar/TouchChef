using UnityEngine;
using System.Collections.Generic;
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

    // Dictionnaire pour suivre les attributions workstation par joueur
    private Dictionary<string, WorkStation> playerWorkstations = new Dictionary<string, WorkStation>();
    
    // Cache de toutes les workstations dans la scène
    private List<WorkStation> allWorkstations;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            InitializeWorkstations();
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
        ProcessType requiredType = DetermineProcessType(message.assignedTask.taskName);
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
        }
        else
        {
            Debug.LogWarning($"WorkstationManager: Aucune station disponible pour le type {requiredType}");
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

    private ProcessType DetermineProcessType(string taskName)
    {
        if (taskName.Contains("couper") || taskName.Contains("Couper") || 
            taskName.Contains("découper") || taskName.Contains("Découper"))
            return ProcessType.Cut;
        if (taskName.Contains("cuire") || taskName.Contains("Cuire") || 
            taskName.Contains("préparer") || taskName.Contains("Préparer"))
            return ProcessType.Cook;
        if (taskName.Contains("laver") || taskName.Contains("Laver"))
            return ProcessType.Wash;
        
        return ProcessType.Cut;
    }

    private WorkStation FindAvailableWorkstation(ProcessType type)
    {
        Debug.Log($"Recherche d'une station de type {type}. Stations actuelles:");
        foreach (var station in allWorkstations)
        {
            bool isOccupied = playerWorkstations.ContainsValue(station);
            Debug.Log($"- Station '{station.gameObject.name}' : Type={station.GetStationType()}, Occupée={isOccupied}");
        }

        var availableStation = allWorkstations.Find(ws => 
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
            Color stationColor;
            if (ColorUtility.TryParseHtmlString(color, out stationColor))
            {
                Debug.Log($"Couleur parsée avec succès: {color} -> {stationColor}");
                highlight.SetHighlight(true, stationColor);
                
                // Vérification après l'application
                Debug.Log($"État du highlight après configuration: {highlight.IsHighlighted()}");
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