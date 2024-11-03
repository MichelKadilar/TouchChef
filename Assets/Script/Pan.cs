using UnityEngine;

public class Pan : BaseContainer
{
    [SerializeField] private ProcessType supportedCookingType = ProcessType.Cook;
    private Vector3 lastValidPosition;
    private Quaternion lastValidRotation;
    private WorkStation lastWorkStation;

    protected override void Awake()
    {
        base.Awake();
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;
    }

    public override void OnTouchPick(int touchId)
    {
        if (!holdDetector.IsHolding)
        {
            return;
        }
        
        lastValidPosition = transform.position;
        lastValidRotation = transform.rotation;
        lastWorkStation = GetCurrentWorkStation();
        base.OnTouchPick(touchId);
    }

    public override void OnPickFailed()
    {
        if (lastWorkStation != null)
        {
            transform.position = lastValidPosition;
            transform.rotation = lastValidRotation;
            lastWorkStation.TryPlaceIngredient(gameObject);
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