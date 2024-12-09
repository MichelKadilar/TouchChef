using UnityEngine;
using System.Collections.Generic;
using NativeWebSocket;

namespace Script.Conveyor
{
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
    
    [System.Serializable]
    public class ProductPrefab
    {
        public int id;
        public GameObject prefab;
    }
    
    public class ConveyorSystem : MonoBehaviour
    {
        public Transform[] conveyorPoints; // Points de passage pour le chemin
        public float speed = 1f;
        private WebSocket websocket;
        public ProductPrefab[] productPrefabs;
        private List<GameObject> activeProducts = new List<GameObject>();
        public ConveyorBelt[] conveyorBelts; // Références aux 4 tapis
        private Dictionary<GameObject, int> productTargetPoints = new Dictionary<GameObject, int>();
        private Dictionary<GameObject, bool> pausedProducts = new Dictionary<GameObject, bool>();

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

            // Déplacer chaque produit le long du chemin
            foreach (var product in activeProducts)
            {
                MoveProductAlongPath(product);
            }
        }

        private void MoveProductAlongPath(GameObject product)
        {
            if (pausedProducts[product]) return; // Ne pas bouger si en pause
            
            if (!productTargetPoints.ContainsKey(product))
            {
                productTargetPoints[product] = 1; // Start moving towards point 1
            }

            int targetIndex = productTargetPoints[product];
            Vector3 targetPos = conveyorPoints[targetIndex].position;
            Vector3 currentPos = product.transform.position;

            // Calculer la direction et déplacer le produit
            Vector3 direction = (targetPos - currentPos).normalized;
            product.transform.position += direction * speed * Time.deltaTime;

            // Vérifier si on est arrivé au point cible
            float distanceToTarget = Vector3.Distance(currentPos, targetPos);
            if (distanceToTarget < 0.1f)
            {
                // Passer au point suivant
                int nextPoint = (targetIndex + 1) % conveyorPoints.Length;
                productTargetPoints[product] = nextPoint;
            
                // Snap à la position exacte pour éviter l'accumulation d'erreurs
                product.transform.position = targetPos;
            }
        }

        private int FindNextPointIndex(Vector3 currentPos)
        {
            float minDistance = float.MaxValue;
            int closestIndex = 0;

            for (int i = 0; i < conveyorPoints.Length; i++)
            {
                float distance = Vector3.Distance(currentPos, conveyorPoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    closestIndex = i;
                }
            }

            return (closestIndex + 1) % conveyorPoints.Length;
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
        
        private GameObject GetPrefabById(int id)
        {
            foreach (var productPrefab in productPrefabs)
            {
                if (productPrefab.id == id)
                {
                    return productPrefab.prefab;
                }
            }
            Debug.LogWarning($"Prefab with id {id} not found!");
            return null;
        }
        
        private void SpawnProduct(Product product)
        {
            GameObject prefab = GetPrefabById(product.id);
            if (prefab != null)
            {
                GameObject newProduct = Instantiate(prefab, conveyorPoints[0].position, Quaternion.identity);
            
                // Ajouter et initialiser le composant ConveyorProduct
                var pickableObject = newProduct.GetComponent<PickableObject>();
                if (pickableObject != null)
                {
                    pickableObject.InitializeConveyor(this);
                }
            
                activeProducts.Add(newProduct);
                productTargetPoints[newProduct] = 1;
                pausedProducts[newProduct] = false;
            }
        }
        
        public void PauseProduct(GameObject product)
        {
            if (pausedProducts.ContainsKey(product))
            {
                pausedProducts[product] = true;
            }
        }

        public void RemoveProduct(GameObject product)
        {
            activeProducts.Remove(product);
            productTargetPoints.Remove(product);
            pausedProducts.Remove(product);
        }

        public void ReturnProductToNearestPoint(GameObject product)
        {
            if (!productTargetPoints.ContainsKey(product)) return;

            // Trouver le point le plus proche
            float minDistance = float.MaxValue;
            int nearestPointIndex = 0;
        
            for (int i = 0; i < conveyorPoints.Length; i++)
            {
                float distance = Vector3.Distance(product.transform.position, conveyorPoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPointIndex = i;
                }
            }

            // Replacer le produit au point le plus proche
            product.transform.position = conveyorPoints[nearestPointIndex].position;
            productTargetPoints[product] = (nearestPointIndex + 1) % conveyorPoints.Length;
            pausedProducts[product] = false;
        }

        private async void OnDestroy()
        {
            if (websocket != null && websocket.State == WebSocketState.Open)
                await websocket.Close();
        }
        
        public bool AddExistingProduct(GameObject product)
        {
            if (activeProducts.Contains(product))
                return false;

            // Trouver le point le plus proche pour placer l'objet
            float minDistance = float.MaxValue;
            int nearestPointIndex = 0;
    
            for (int i = 0; i < conveyorPoints.Length; i++)
            {
                float distance = Vector3.Distance(product.transform.position, conveyorPoints[i].position);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestPointIndex = i;
                }
            }

            // Placer l'objet sur le convoyeur
            product.transform.position = conveyorPoints[nearestPointIndex].position;
            activeProducts.Add(product);
            productTargetPoints[product] = (nearestPointIndex + 1) % conveyorPoints.Length;
            pausedProducts[product] = false;

            // Initialiser l'objet pour le convoyeur
            var pickableObject = product.GetComponent<PickableObject>();
            if (pickableObject != null)
            {
                pickableObject.InitializeConveyor(this);
            }

            return true;
        }
    }
}