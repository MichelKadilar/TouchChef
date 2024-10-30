using UnityEngine;

public interface IPickable
{
    int? CurrentTouchId { get; }
    bool IsBeingDragged { get; }
    Vector3 OriginalPosition { get; }
    
    void OnTouchPick(int touchId);
    void OnTouchMove(int touchId, Vector3 position);
    void OnTouchDrop(int touchId, Vector2 screenPosition);
    void OnPickFailed();  // Nouveau : appelé quand le drop échoue
}