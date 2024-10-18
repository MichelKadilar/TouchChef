using UnityEngine;

public interface IPickable
{
    void OnPick();
    void OnDrop();
    void OnMove(Vector3 newPosition);
}
public interface ICuttable
{
    void Cut();
}

public interface ICookable
{
    void Cook();
}

public interface IMixable
{
    void Mix();
}
