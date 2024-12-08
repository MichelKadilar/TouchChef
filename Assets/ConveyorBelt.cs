using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;
using System;

[System.Serializable]
public class Product
{
    public int id;
    public string name;
    public string icon;
}

[System.Serializable]
public class WebSocketMessage
{
    public string type;
    public Product product;
    public string from;
    public string to;
}

public class ConveyorBelt : MonoBehaviour
{
    public float speed = 1f;
    private Material material;
    private WebSocket websocket;
    public GameObject productPrefab; // Préfab pour représenter les produits
    private List<GameObject> activeProducts = new List<GameObject>();
    public Vector3 spawnPosition = new Vector3(0, 0.5f, 0); // Position de départ des produits
    
    async void Start()
    {
        material = GetComponent<Renderer>().material;
        
        // Initialisation WebSocket
        websocket = new WebSocket("ws://websocket.chhilif.com:8080");

        websocket.OnMessage += HandleMessage;
        
        // Connexion au serveur WebSocket
        await websocket.Connect();
    }

    void Update()
    {
        // Mise à jour WebSocket
        #if !UNITY_WEBGL || UNITY_EDITOR
            websocket.DispatchMessageQueue();
        #endif

        // Animation du tapis
        Vector2 offset = material.mainTextureOffset;
        offset.x -= Time.deltaTime * speed;
        material.mainTextureOffset = offset;

        // Déplacement des produits
        foreach (var product in activeProducts)
        {
            product.transform.Translate(Vector3.right * speed * Time.deltaTime);
            
            // Si le produit atteint la fin du tapis, le replacer au début
            if (product.transform.position.x > transform.position.x + transform.localScale.x/2)
            {
                product.transform.position = new Vector3(
                    transform.position.x - transform.localScale.x/2,
                    product.transform.position.y,
                    product.transform.position.z
                );
            }
        }
    }

    private void HandleMessage(byte[] bytes)
    {
        string message = System.Text.Encoding.UTF8.GetString(bytes);
        WebSocketMessage socketMessage = JsonUtility.FromJson<WebSocketMessage>(message);

        Debug.Log("Received OnMessage! (" + bytes.Length + " bytes) " + message);
        if (socketMessage.type == "add_product")
        {
            SpawnProduct(socketMessage.product);
        }
    }

    private void SpawnProduct(Product product)
    {
        Vector3 spawn = transform.position + spawnPosition + new Vector3(-transform.localScale.x/2, 0, 0);
        GameObject newProduct = Instantiate(productPrefab, spawn, Quaternion.identity);
        
        activeProducts.Add(newProduct);
    }

    private async void OnDestroy()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    private async void OnApplicationQuit()
    {
        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }
}