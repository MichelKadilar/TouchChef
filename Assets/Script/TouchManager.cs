﻿using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TouchManager : MonoBehaviour
{
    private Camera mainCamera;
    private TouchInputActions touchActions;
    [SerializeField] private float tapThreshold = 0.2f;
    [SerializeField] private float touchRadius = 0.5f;
    [SerializeField] private LayerMask interactableLayer;
    
    private Dictionary<int, TouchInfo> touchInfos = new Dictionary<int, TouchInfo>();
    private Dictionary<int, bool> activeTouches = new Dictionary<int, bool>();

    private class TouchInfo
    {
        public Vector2 startPosition;
        public float startTime;
        public bool isHolding;
        public GameObject targetObject;
    }

    private void Awake()
    {
        Debug.Log("TouchManager: Awake");
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("TouchManager: Main Camera not found!");
        }

        InitializeTouchActions();
    }

    private void InitializeTouchActions()
    {
        if (touchActions == null)
        {
            touchActions = new TouchInputActions();
            SetupInputActions();
        }
    }

    private void SetupInputActions()
    {
        // Gestion du début du touch/clic
        touchActions.Touch.PrimaryTouch.started += ctx =>
        {
            var touchId = GetTouchId(ctx);
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            Debug.Log($"Touch/Click started with ID {touchId} at position {position}");
            HandleTouchStart(touchId, position);
        };

        // Gestion de la fin du touch/clic
        touchActions.Touch.PrimaryTouch.canceled += ctx =>
        {
            var touchId = GetTouchId(ctx);
            Vector2 position = touchActions.Touch.PrimaryTouchPosition.ReadValue<Vector2>();
            Debug.Log($"Touch/Click ended with ID {touchId} at position {position}");
            HandleTouchEnd(touchId, position);
        };

        // Gestion du mouvement
        touchActions.Touch.PrimaryTouchPosition.performed += ctx =>
        {
            var touchId = GetTouchId(ctx);
            if (activeTouches.ContainsKey(touchId) && activeTouches[touchId])
            {
                Vector2 position = ctx.ReadValue<Vector2>();
                HandleTouchMove(touchId, position);
            }
        };
    }

    private void HandleTouchStart(int touchId, Vector2 position)
    {
        Debug.Log($"HandleTouchStart - TouchID: {touchId}, Position: {position}");
        activeTouches[touchId] = true;
    
        touchInfos[touchId] = new TouchInfo
        {
            startPosition = position,
            startTime = Time.time,
            isHolding = false,
            targetObject = GetTouchedObject(position)
        };

        // Vérifier si un objet est déjà en cours de drag
        bool isAnyObjectDragged = FindObjectsOfType<MonoBehaviour>()
            .OfType<IPickable>()
            .Any(p => p.IsBeingDragged);

        if (isAnyObjectDragged)
        {
            Debug.Log("An object is already being dragged");
            return;
        }

        // Gestion du raycast et des interactions
        Ray ray = mainCamera.ScreenPointToRay(position);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f, interactableLayer);

        if (hits.Length > 0)
        {
            System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
            
            foreach (var hit in hits)
            {
                Debug.Log($"Processing hit on: {hit.collider.gameObject.name}");

                // 1. D'abord vérifier le Basket
                var basket = hit.collider.GetComponent<Basket>();
                if (basket != null)
                {
                    Debug.Log($"Basket touched: {basket.name}");
                    basket.OnTouchDown(touchId, position);
                    return;
                }

                // 2. Ensuite vérifier les ingrédients
                var ingredient = hit.collider.GetComponent<BaseIngredient>();
                var pickable = hit.collider.GetComponent<IPickable>();
                if (ingredient != null)
                {
                    // Si l'ingrédient est sur une workstation, utiliser le hold
                    if (ingredient.GetCurrentWorkStation() != null)
                    {
                        var holdDetector = ingredient.GetComponent<HoldDetector>();
                        if (holdDetector != null)
                        {
                            Debug.Log($"Starting hold on ingredient: {ingredient.name}");
                            holdDetector.StartHolding(touchId, position);
                            touchInfos[touchId].isHolding = true;
                            return;
                        }
                    }
                    // Si l'ingrédient n'est pas sur une workstation, permettre le drag
                    else
                    {
                        Debug.Log($"Picking up ingredient: {ingredient.name}");
                        pickable = ingredient.GetComponent<IPickable>();
                        if (pickable != null)
                        {
                            pickable.OnTouchPick(touchId);
                            return;
                        }
                    }
                }
                
                // 3. Finalement, vérifier les autres pickables
                pickable = hit.collider.GetComponent<IPickable>();
                if (pickable != null && !pickable.IsBeingDragged)
                {
                    Debug.Log($"Picking up object: {hit.collider.name}");
                    pickable.OnTouchPick(touchId);
                    return;
                }
            }
        }
    }

    private void HandleTouchMove(int touchId, Vector2 position)
    {
        if (!activeTouches.ContainsKey(touchId)) return;

        // Vérifier d'abord si un objet est en cours de drag
        bool objectMoved = false;
        Vector3 worldPosition = GetWorldPosition(position);
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == touchId && pickable.IsBeingDragged)
            {
                pickable.OnTouchMove(touchId, worldPosition);
                objectMoved = true;
                Debug.Log($"Moving dragged object {(pickable as MonoBehaviour).gameObject.name}");
            }
        }

        // Si aucun objet n'est en cours de drag, mettre à jour les hold detectors
        if (!objectMoved && touchInfos.TryGetValue(touchId, out TouchInfo info) && info.isHolding)
        {
            var ingredients = FindObjectsOfType<BaseIngredient>();
            foreach (var ingredient in ingredients)
            {
                var holdDetector = ingredient.GetComponent<HoldDetector>();
                if (holdDetector != null)
                {
                    holdDetector.UpdateHolding(touchId, position);
                }
            }
        }
    }

    private void HandleTouchEnd(int touchId, Vector2 position)
    {
        Debug.Log($"HandleTouchEnd - TouchID: {touchId}, Position: {position}");

        if (touchInfos.TryGetValue(touchId, out TouchInfo info))
        {
            if (Time.time - info.startTime < tapThreshold && !info.isHolding)
            {
                // C'était un tap rapide
                HandleTapProcess(info.targetObject);
            }
        }

        // Gérer les drops AVANT d'arrêter les holds
        foreach (var pickable in FindObjectsOfType<MonoBehaviour>().OfType<IPickable>())
        {
            if (pickable.CurrentTouchId == touchId && pickable.IsBeingDragged)
            {
                Debug.Log($"Dropping pickable object: {(pickable as MonoBehaviour).gameObject.name}");
                pickable.OnTouchDrop(touchId, position);
            }
        }

        // Arrêter les holds après avoir géré les drops
        var ingredients = FindObjectsOfType<BaseIngredient>();
        foreach (var ingredient in ingredients)
        {
            var holdDetector = ingredient.GetComponent<HoldDetector>();
            if (holdDetector != null)
            {
                holdDetector.StopHolding();
            }
        }

        activeTouches.Remove(touchId);
        touchInfos.Remove(touchId);
    }

    private GameObject GetTouchedObject(Vector2 position)
    {
        Ray ray = mainCamera.ScreenPointToRay(position);
        if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactableLayer))
        {
            return hit.collider.gameObject;
        }
        return null;
    }

    private Vector3 GetWorldPosition(Vector2 screenPosition)
    {
        float distanceFromCamera = Mathf.Abs(mainCamera.transform.position.z);
        Vector3 worldPos = mainCamera.ScreenToWorldPoint(new Vector3(
            screenPosition.x,
            screenPosition.y,
            distanceFromCamera
        ));
        worldPos.z = -10f;
        return worldPos;
    }

    private int GetTouchId(InputAction.CallbackContext ctx)
    {
        if (ctx.control?.device is Touchscreen touchScreen)
        {
            return touchScreen.deviceId;
        }
        return 0; // Pour la souris et autres dispositifs
    }

    private void HandleTapProcess(GameObject targetObject)
    {
        if (targetObject == null) return;

        var workStation = targetObject.GetComponent<WorkStation>();
        if (workStation != null && workStation.HasIngredient())
        {
            Debug.Log($"Processing at workstation: {workStation.name}");
            workStation.StartProcessing();
        }
    }

    private void OnEnable()
    {
        InitializeTouchActions();
        touchActions?.Enable();
    }

    private void OnDisable()
    {
        touchActions?.Disable();
    }

    private void OnDestroy()
    {
        if (touchActions != null)
        {
            touchActions.Disable();
            touchActions.Dispose();
        }
    }

    private void Start()
    {
        if (interactableLayer == 0)
        {
            interactableLayer = 1 << 3;
            Debug.Log($"Using default layer mask: {interactableLayer.value}");
        }
        Debug.Log($"Configured layer mask: {interactableLayer.value}");
        
        var interactables = FindObjectsOfType<MonoBehaviour>().OfType<IPickable>();
        foreach (var interactable in interactables)
        {
            var go = (interactable as MonoBehaviour).gameObject;
            Debug.Log($"Interactable object: {go.name} on layer {go.layer}");
        }
    }
}