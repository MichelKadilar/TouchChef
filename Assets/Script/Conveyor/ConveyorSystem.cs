using UnityEngine;
using System.Collections.Generic;

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
    public class ProductPrefab
    {
        public int id;
        public GameObject prefab;
    }
    
    public class ConveyorSystem : MonoBehaviour
    {
        public Transform[] conveyorPoints;
        public float speed = 1f;
        public ProductPrefab[] productPrefabs;
        public ConveyorBelt[] conveyorBelts;

        private List<GameObject> activeProducts = new List<GameObject>();
        private Dictionary<GameObject, int> productTargetPoints = new Dictionary<GameObject, int>();
        private Dictionary<GameObject, bool> pausedProducts = new Dictionary<GameObject, bool>();

        void Start()
        {
            Debug.Log("ConveyorSystem: Initialisation");
        }

        void Update()
        {
            foreach (var product in activeProducts)
            {
                MoveProductAlongPath(product);
            }
        }

        private void MoveProductAlongPath(GameObject product)
        {
            if (pausedProducts[product]) return;
            
            if (!productTargetPoints.ContainsKey(product))
            {
                productTargetPoints[product] = 1;
            }

            int targetIndex = productTargetPoints[product];
            Vector3 targetPos = conveyorPoints[targetIndex].position;
            Vector3 currentPos = product.transform.position;

            Vector3 direction = (targetPos - currentPos).normalized;
            product.transform.position += direction * speed * Time.deltaTime;

            float distanceToTarget = Vector3.Distance(currentPos, targetPos);
            if (distanceToTarget < 0.1f)
            {
                int nextPoint = (targetIndex + 1) % conveyorPoints.Length;
                productTargetPoints[product] = nextPoint;
                product.transform.position = targetPos;
            }
        }

        public void SpawnProduct(Product product)
        {
            Debug.Log($"ConveyorSystem: Création d'un nouveau produit - ID: {product.id}, Nom: {product.name}");
            GameObject prefab = GetPrefabById(product.id);
            if (prefab != null)
            {
                GameObject newProduct = Instantiate(prefab, conveyorPoints[0].position, Quaternion.identity);
                Debug.Log($"ConveyorSystem: Produit créé à la position {conveyorPoints[0].position}");
    
                var pickableObject = newProduct.GetComponent<PickableObject>();
                if (pickableObject != null)
                {
                    pickableObject.InitializeConveyor(this);
                    Debug.Log("ConveyorSystem: Composant PickableObject initialisé");
                }
    
                activeProducts.Add(newProduct);
                productTargetPoints[newProduct] = 1;
                pausedProducts[newProduct] = false;

                // Play spawn sound effect
                AudioManager.Instance.PlayItemSpawnSound();
            }
            else
            {
                Debug.LogError($"ConveyorSystem: Prefab non trouvé pour l'ID {product.id}");
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
            Debug.LogWarning($"ConveyorSystem: Prefab avec l'ID {id} non trouvé!");
            return null;
        }
        
        public void PauseProduct(GameObject product)
        {
            if (pausedProducts.ContainsKey(product))
            {
                Debug.Log($"ConveyorSystem: Mise en pause du produit {product.name}");
                pausedProducts[product] = true;
            }
        }

        public void RemoveProduct(GameObject product)
        {
            Debug.Log($"ConveyorSystem: Suppression du produit {product.name}");
            activeProducts.Remove(product);
            productTargetPoints.Remove(product);
            pausedProducts.Remove(product);
        }

        public void ReturnProductToNearestPoint(GameObject product)
        {
            if (!productTargetPoints.ContainsKey(product))
            {
                Debug.Log($"ConveyorSystem: Tentative de retour d'un produit non suivi");
                return;
            }

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

            Debug.Log($"ConveyorSystem: Retour du produit {product.name} au point {nearestPointIndex}");
            product.transform.position = conveyorPoints[nearestPointIndex].position;
            productTargetPoints[product] = (nearestPointIndex + 1) % conveyorPoints.Length;
            pausedProducts[product] = false;
        }
        
        public bool AddExistingProduct(GameObject product)
        {
            Debug.Log($"ConveyorSystem: Tentative d'ajout d'un produit existant {product.name}");
            
            if (activeProducts.Contains(product))
            {
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

                product.transform.position = conveyorPoints[nearestPointIndex].position;
                productTargetPoints[product] = (nearestPointIndex + 1) % conveyorPoints.Length;
                pausedProducts[product] = false;
                Debug.Log($"ConveyorSystem: Produit existant replacé au point {nearestPointIndex}");
                return true;
            }

            // Ajout d'un nouveau produit
            float newMinDistance = float.MaxValue;
            int newNearestPointIndex = 0;
    
            for (int i = 0; i < conveyorPoints.Length; i++)
            {
                float distance = Vector3.Distance(product.transform.position, conveyorPoints[i].position);
                if (distance < newMinDistance)
                {
                    newMinDistance = distance;
                    newNearestPointIndex = i;
                }
            }

            product.transform.position = conveyorPoints[newNearestPointIndex].position;
            activeProducts.Add(product);
            productTargetPoints[product] = (newNearestPointIndex + 1) % conveyorPoints.Length;
            pausedProducts[product] = false;

            var pickableObject = product.GetComponent<PickableObject>();
            if (pickableObject != null)
            {
                pickableObject.InitializeConveyor(this);
                Debug.Log($"ConveyorSystem: Nouveau produit initialisé au point {newNearestPointIndex}");
            }

            return true;
        }
    }
}