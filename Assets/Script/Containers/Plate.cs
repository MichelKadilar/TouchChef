using UnityEngine;

public class Plate : BaseContainer
{
    [SerializeField] private int maxIngredientsInPlate = 4;
    [SerializeField] private float ingredientStackOffset = 0.1f;
    
    protected void Awake()
    {
        base.Awake();
        maxIngredients = maxIngredientsInPlate;
    }

    public override bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        // Vérifie si l'assiette peut accepter plus d'ingrédients
        if (!base.CanAcceptIngredient(ingredient)) return false;
        
        // Vérifie si l'ingrédient est dans un état approprié (cuit, coupé, etc.)
        switch (ingredient.CurrentState)
        {
            case IngredientState.Cut:
            case IngredientState.Cooked:
                return true;
            default:
                Debug.Log($"L'ingrédient {ingredient.name} n'est pas dans un état approprié pour être ajouté à l'assiette");
                return false;
        }
    }

    public override bool AddIngredient(BaseIngredient ingredient)
    {
        if (!CanAcceptIngredient(ingredient)) return false;

        // Obtient la position d'attachement et applique un décalage vertical pour empiler
        Vector3 attachPosition = ingredientAttachPoint.position;
        attachPosition.y += contents.Count * ingredientStackOffset;
        
        // Ajoute l'ingrédient à la liste
        contents.Add(ingredient);
        
        // Configure la transformation de l'ingrédient
        ingredient.transform.SetParent(ingredientAttachPoint);
        ingredient.transform.position = attachPosition;
        ingredient.transform.rotation = ingredientAttachPoint.rotation;

        Debug.Log($"Ingrédient {ingredient.name} ajouté à l'assiette");
        return true;
    }

    protected override bool TryDropObject(Vector2 screenPosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);
        RaycastHit[] hits = Physics.RaycastAll(ray, 100f);

        foreach (var hit in hits)
        {
            TableStation tableStation = hit.collider.GetComponent<TableStation>();
            if (tableStation != null && tableStation.TryPlaceIngredient(gameObject))
            {
                Debug.Log($"Assiette placée sur la table avec succès");
                return true;
            }
        }

        Debug.Log("Impossible de placer l'assiette ici");
        return false;
    }
}