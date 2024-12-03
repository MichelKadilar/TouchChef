using System.Collections.Generic;
using UnityEngine;

public class DraggingManager : MonoBehaviour
{
    // Singleton instance
    public static DraggingManager Instance { get; private set; }

    private readonly HashSet<IPickable> activeDrags = new HashSet<IPickable>();

    // Ensure only one instance of DraggingManager exists in the scene
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogError("Multiple instances of DraggingManager detected. Destroying duplicate.");
            Destroy(gameObject); // Ensure only one instance is active
        }
    }

    // Start dragging the pickable object by adding it to the activeDrags set
    public void StartDragging(IPickable pickable)
    {
        activeDrags.Add(pickable);
    }

    // Stop dragging the pickable object by removing it from the activeDrags set
    public void StopDragging(IPickable pickable)
    {
        activeDrags.Remove(pickable);
    }

    // Check if the pickable object is currently being dragged
    public bool IsBeingDragged(IPickable pickable)
    {
        return activeDrags.Contains(pickable);
    }
}