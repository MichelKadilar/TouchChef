using UnityEngine;
using NativeWebSocket;
using UnityEngine.SceneManagement;
using Script.Conveyor;
using System;
using System.Collections;

public class ClientWebSocket : MonoBehaviour
{
    public static ClientWebSocket Instance { get; private set; }

    // Event pour les systèmes qui ont besoin d'être notifiés des messages
    public event Action<WebSocketTaskMessage> OnTaskMessageReceived;
    public event Action<WebSocketTaskTableMessage> OnTaskTableMessageReceived;
    public event Action<Product> OnProductMessageReceived;
    public CookData[] players;
    public Task[] tasks;
    public event Action<CookData[]> OnPlayersUpdated;
    public event Action<WebSocketTasksListMessage> OnTasksLiskUpdated;
    private bool isGameStarted = false;

    private WebSocket _websocket;
    private WorkstationManager workstationManager;
    private ConveyorSystem conveyorSystem;
    private PostItManager postItManager;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    async void Start()
    {
        Debug.Log("ClientWebSocket: Démarrage...");
        InitializeManagers();
        
        _websocket = new WebSocket("wss://websocket.chhilif.com/ws");

        _websocket.OnOpen += () =>
        {
            Debug.Log("ClientWebSocket: Connection ouverte!");
        };

        _websocket.OnError += (e) =>
        {
            Debug.LogError($"ClientWebSocket: Erreur! {e}");
        };

        _websocket.OnClose += (e) =>
        {
            Debug.Log("ClientWebSocket: Connection fermée!");
        };

        _websocket.OnMessage += HandleWebSocketMessage;

        try
        {
            await _websocket.Connect();
        }
        catch (Exception e)
        {
            Debug.LogError($"ClientWebSocket: Erreur de connexion: {e.Message}");
        }
    }

    private void InitializeManagers()
    {
        Debug.Log("ClientWebSocket: Initialisation des managers...");
        workstationManager = WorkstationManager.Instance;
        conveyorSystem = FindObjectOfType<ConveyorSystem>();
        
        if (workstationManager == null)
        {
            Debug.LogWarning("ClientWebSocket: WorkstationManager non trouvé!");
            GameObject controleur = GameObject.Find("Controleur");
            if (controleur != null)
            {
                workstationManager = controleur.GetComponent<WorkstationManager>();
                if (workstationManager != null)
                {
                    Debug.Log("WorkstationManager trouvé, initialisation...");
                    DontDestroyOnLoad(controleur);
                    workstationManager.Initialize();
                }
            }
        }
        if (conveyorSystem == null)
            Debug.LogWarning("ClientWebSocket: ConveyorSystem non trouvé!");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"ClientWebSocket: Nouvelle scène chargée: {scene.name}");
        if (workstationManager == null)
            workstationManager = WorkstationManager.Instance;
        if (conveyorSystem == null)
            conveyorSystem = FindObjectOfType<ConveyorSystem>();
    }

    private void HandleWebSocketMessage(byte[] bytes)
    {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        if (message.Contains("heartrate"))
        {
            return;
        }
        
        Debug.Log($"ClientWebSocket: Message reçu: {message}");
        if (message.Contains("startGame"))
        {
            // Vérifier si on est déjà sur la scène Cuisine
            if (SceneManager.GetActiveScene().name == "Cuisine")
            {
                Debug.Log("ClientWebSocket: Déjà sur la scène Cuisine, pas besoin de recharger");
                return;
            }
            
            // Ignorer le message si le jeu est déjà démarré
            if (isGameStarted)
            {
                Debug.Log("ClientWebSocket: Ignorer la commande de démarrage - jeu déjà en cours");
                return;
            }

            Debug.Log("ClientWebSocket: Commande de démarrage du jeu reçue");
            isGameStarted = true;
            StartCoroutine(LoadGameScene());
            return;
        }

        if (message.Contains("cooksList"))
        {
            Debug.Log("ClientWebSocket: Liste de cuisiniers reçue");
            var cooksList = JsonUtility.FromJson<WebSocketCooksListMessage>(message);
            if (cooksList.cooksList != null)
            {
                players = cooksList.cooksList;
                OnPlayersUpdated?.Invoke(players);
            }
            return;
        }
        
        if (message.Contains("tasksList"))
        {
            var tasksList = JsonUtility.FromJson<WebSocketTasksListMessage>(message);
            if (tasksList.tasks != null)
            {
                Debug.Log("ClientWebSocket: Liste de tâches reçue - Nombre: " + tasksList.tasks.Length);
                tasks = tasksList.tasks;
                OnTasksLiskUpdated?.Invoke(tasksList);
            }
            return;
        }

        try 
        {
            // Vérifier d'abord si c'est un message unassign
            if (message.Contains("unactiveTask"))
            {
                var unassignMessage = JsonUtility.FromJson<WebSocketMessage>(message);
                if (unassignMessage.type == "unactiveTask" && unassignMessage.to == "all")
                {
                    Debug.Log($"ClientWebSocket: Message de désactivation de tâche reçu de {unassignMessage.from}");
                    HandleUnassignTask(unassignMessage);
                    return;
                }
            }
            
            // Ensuite essayer comme message de produit
            WebSocketMessage productMessage = JsonUtility.FromJson<WebSocketMessage>(message);
            if (productMessage.type == "add_product")
            {
                Debug.Log("ClientWebSocket: Message de produit détecté");
                HandleProductMessage(productMessage);
                return;
            }
            
            // Essayer comme message de tâche de table
            WebSocketTaskTableMessage tableMessage = JsonUtility.FromJson<WebSocketTaskTableMessage>(message);
            if (tableMessage.type == "table_task")
            {
                Debug.Log($"ClientWebSocket: Message post-it détecté");
                OnTaskTableMessageReceived?.Invoke(tableMessage);
                return;
            }
        
            // Si ce n'est pas un produit, essayer comme message de tâche
            WebSocketTaskMessage taskMessage = JsonUtility.FromJson<WebSocketTaskMessage>(message);
            if (!string.IsNullOrEmpty(taskMessage.type))
            {
                Debug.Log($"ClientWebSocket: Message de tâche détecté - Type: {taskMessage.type}");
                HandleTaskMessage(taskMessage);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"ClientWebSocket: Erreur de parsing: {e.Message}");
        }
    }
    
    private void HandleUnassignTask(WebSocketMessage message)
    {
        Debug.Log($"Traitement du unassignTask: {JsonUtility.ToJson(message)}");
    
        if (workstationManager == null)
        {
            Debug.LogError("WorkstationManager manquant!");
            return;
        }

        if (message.from == "angular")
        {
            // Si le message vient d'Angular, on notifie le WorkstationManager
            workstationManager.HandleUnactiveTaskBroadcast();
        }
    }
    
    private IEnumerator LoadGameScene()
    {
        // Désactiver tous les scripts qui utilisent la caméra
        DisableAllCameraUsers();

        // Charger la scène Cuisine
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Cuisine", LoadSceneMode.Single);
        
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        // Attendre deux frames pour s'assurer que tout est initialisé
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        Debug.Log("Recherche du Controleur dans la scène...");
        GameObject controleur = GameObject.Find("Controleur");
        if (controleur == null)
        {
            Debug.LogError("Controleur non trouvé dans la scène!");
            yield break;
        }

        WorkstationManager manager = controleur.GetComponent<WorkstationManager>();
        if (manager == null)
        {
            Debug.LogError("WorkstationManager non trouvé sur le Controleur!");
            yield break;
        }

        Debug.Log("WorkstationManager trouvé, initialisation...");
        DontDestroyOnLoad(controleur);
        manager.Initialize();

        // Réinitialiser les références
        InitializeManagers();

        // Réactiver tous les scripts qui utilisent la caméra
        EnableAllCameraUsers();
    }

    private void DisableAllCameraUsers()
    {
        var touchManagers = FindObjectsOfType<MultiTouchDragDrop>();
        var sliceDetectors = FindObjectsOfType<SliceDetector>();
        
        foreach (var manager in touchManagers)
        {
            manager.enabled = false;
        }
        
        foreach (var detector in sliceDetectors)
        {
            detector.enabled = false;
        }
    }

    private void EnableAllCameraUsers()
    {
        var touchManagers = FindObjectsOfType<MultiTouchDragDrop>();
        var sliceDetectors = FindObjectsOfType<SliceDetector>();
        
        foreach (var manager in touchManagers)
        {
            manager.enabled = true;
        }
        
        foreach (var detector in sliceDetectors)
        {
            detector.enabled = true;
        }
    }
    
    public void SendMessage(string message)
    {
        if (_websocket != null && _websocket.State == WebSocketState.Open)
        {
            Debug.Log($"Envoi du message WebSocket : {message}");
            _websocket.SendText(message);
        }
        else
        {
            Debug.LogError("Tentative d'envoi de message avec une connexion WebSocket fermée");
        }
    }

    private void HandleTaskMessage(WebSocketTaskMessage message)
    {
        // Vérifier si le post-it existe dans le board
        PostItManager postItManagerI = PostItManager.Instance;
        if (postItManagerI != null)
        {
            // Si le post-it n'existe pas, le créer
            if (!postItManagerI.activePostIts.ContainsKey(message.assignedTask.taskId))
            {
                postItManagerI.CreatePostIt(message.assignedTask.taskId, message.assignedTask.taskIcons);
            }
            postItManagerI.OutlinePostIt(message.assignedTask.taskId, message.assignedTask.cook.color);
        }
        
        // Attendre que le WorkstationManager soit disponible
        WorkstationManager manager = FindObjectOfType<WorkstationManager>();
        if (manager == null)
        {
            StartCoroutine(WaitForWorkstationManager(message));
            return;
        }
        
        // Notifier les abonnés
        OnTaskMessageReceived?.Invoke(message);

        if (workstationManager == null)
        {
            Debug.LogError("ClientWebSocket: WorkstationManager manquant pour traiter la tâche!");
            return;
        }

        switch (message.type)
        {
            case "activeTask" when message.to == "table":
                Debug.Log("ClientWebSocket: Traitement d'une tâche active");
                workstationManager.HandleActiveTask(message);
                break;

            case "unactiveTask":
                Debug.Log($"ClientWebSocket: Traitement d'une désactivation de tâche");
                workstationManager.HandleUnactiveTask(message.from);
                break;
        }
    }

    private IEnumerator WaitForWorkstationManager(WebSocketTaskMessage message)
    {
        WorkstationManager manager = null;
        float timeout = 5f; // Timeout de 5 secondes
        float elapsed = 0f;
        
        while (manager == null && elapsed < timeout)
        {
            manager = FindObjectOfType<WorkstationManager>();
            if (manager == null)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }
        
        if (manager != null)
        {
            // Réessayer le traitement du message
            HandleTaskMessage(message);
        }
        else
        {
            Debug.LogError("Impossible de trouver le WorkstationManager après le timeout");
        }
    }

    private void HandleProductMessage(WebSocketMessage message)
    {
        if (message.product == null)
        {
            Debug.LogError("ClientWebSocket: Message de produit invalide");
            return;
        }

        // Conversion en Product pour le ConveyorSystem
        var product = new Product
        {
            id = message.product.id,
            name = message.product.name,
            icon = message.product.icon
        };

        // Notifier les abonnés
        OnProductMessageReceived?.Invoke(product);

        // Traiter avec le ConveyorSystem
        if (conveyorSystem != null)
        {
            Debug.Log($"ClientWebSocket: Transmission du produit au ConveyorSystem - ID: {product.id}");
            conveyorSystem.SpawnProduct(product);
        }
        else
        {
            Debug.LogError("ClientWebSocket: ConveyorSystem manquant pour traiter le produit!");
        }
    }

    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
            _websocket?.DispatchMessageQueue();
        #endif
    }

    private async void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        if (_websocket != null && _websocket.State == WebSocketState.Open)
        {
            Debug.Log("ClientWebSocket: Fermeture de la connexion");
            await _websocket.Close();
        }
    }
}