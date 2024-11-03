using UnityEngine;

public class TableStation : WorkStation
{
    [Header("Table Configuration")]
    [SerializeField] private bool acceptAllObjects = true;
    
    public override bool TryPlaceIngredient(GameObject obj)
    {
        if (!acceptAllObjects) return base.TryPlaceIngredient(obj);

        Debug.Log($"Table attempting to accept object: {obj.name}");

        // Vérifier si l'emplacement est déjà occupé
        if (HasIngredient() && currentObject != obj)
        {
            Debug.Log($"Table is occupied by different object: {currentObject.name}");
            ShowInvalidPlacement();
            return false;
        }

        // Accepter n'importe quel objet qui est soit un BaseIngredient soit un IContainer
        var isIngredient = obj.GetComponent<BaseIngredient>();
        var isContainer = obj.GetComponent<IContainer>();

        if (isIngredient == null && isContainer == null)
        {
            Debug.Log($"Object {obj.name} is neither ingredient nor container");
            ShowInvalidPlacement();
            return false;
        }

        // Placer l'objet à la position de la table
        obj.transform.position = ingredientPosition.position;
        obj.transform.rotation = ingredientPosition.rotation;

        // Mettre à jour les références
        currentObject = obj;
        currentProcessable = isIngredient;
        currentContainer = isContainer;
        isOccupied = true;
        
        // Configurer l'objet avec sa nouvelle workstation
        var pickable = obj.GetComponent<PickableObject>();
        if (pickable != null)
        {
            pickable.SetCurrentWorkStation(this);
        }

        OnIngredientPlaced?.Invoke();
        UpdateVisuals();
        
        Debug.Log($"Successfully placed {obj.name} on table");
        return true;
    }

    public override bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        if (!acceptAllObjects) return base.CanAcceptIngredient(ingredient);
        return !HasIngredient() || currentObject == ingredient.gameObject;
    }

    protected override void UpdateVisuals()
    {
        if (availableVisual != null)
        {
            availableVisual.SetActive(!HasIngredient());
        }
        if (processingVisual != null)
        {
            processingVisual.SetActive(false); // Tables never process
        }
    }

    protected override void ShowInvalidPlacement()
    {
        base.ShowInvalidPlacement();
        Debug.Log("Invalid placement on table");
    }
}