using UnityEngine;

public class Pan : BaseContainer
{
    [SerializeField] private ProcessType supportedCookingType = ProcessType.Cook;
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;

    protected override void Awake()
    {
        base.Awake();
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;
    }

    public override void OnTouchPick(int touchId)
    {
        // Sauvegarder la position avant le déplacement
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;
        base.OnTouchPick(touchId);
    }

    public override void OnPickFailed()
    {
        var currentStation = GetCurrentWorkStation();
        // Si la poêle était sur une workstation, on la remet à sa dernière position valide
        if (currentStation != null)
        {
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
        }
        else
        {
            base.OnPickFailed();
        }
    }

    public ProcessType GetSupportedCookingType()
    {
        return supportedCookingType;
    }

    public override bool CanAcceptIngredient(BaseIngredient ingredient)
    {
        if (!base.CanAcceptIngredient(ingredient)) return false;
        return ingredient.CanProcess(supportedCookingType);
    }

    public bool StartCooking()
    {
        if (contents.Count == 0) return false;
        
        var ingredient = contents[0];
        if (ingredient.CanProcess(supportedCookingType))
        {
            ingredient.Process(supportedCookingType);
            return true;
        }
        return false;
    }
}