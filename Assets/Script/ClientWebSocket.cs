using UnityEngine;
using NativeWebSocket;
using UnityEngine.SceneManagement;
using Script.Conveyor;
using System;

public class ClientWebSocket : MonoBehaviour
{
    public static ClientWebSocket Instance { get; private set; }

    // Event pour les systèmes qui ont besoin d'être notifiés des messages
    public event Action<WebSocketTaskMessage> OnTaskMessageReceived;
    public event Action<Product> OnProductMessageReceived;

    private WebSocket _websocket;
    private WorkstationManager workstationManager;
    private ConveyorSystem conveyorSystem;

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
            return;
        }
    }

    async void Start()
    {
        Debug.Log("ClientWebSocket: Démarrage...");
        InitializeManagers();
        
        _websocket = new WebSocket("ws://websocket.chhilif.com/ws");

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
        workstationManager = WorkstationManager.Instance;
        conveyorSystem = FindObjectOfType<ConveyorSystem>();
        
        if (workstationManager == null)
            Debug.LogWarning("ClientWebSocket: WorkstationManager non trouvé!");
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
        Debug.Log($"ClientWebSocket: Message reçu: {message}");

        if (message.Contains("startGame"))
        {
            Debug.Log("ClientWebSocket: Commande de démarrage du jeu reçue");
            SceneManager.LoadScene("Cuisine");
            return;
        }

        try 
        {
            // D'abord essayer comme message de produit
            WebSocketMessage productMessage = JsonUtility.FromJson<WebSocketMessage>(message);
            if (productMessage.type == "add_product")
            {
                Debug.Log("ClientWebSocket: Message de produit détecté");
                HandleProductMessage(productMessage);
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

    private void HandleTaskMessage(WebSocketTaskMessage message)
    {
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