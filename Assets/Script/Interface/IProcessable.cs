public interface IProcessable
{
    bool CanProcess(ProcessType processType);
    void Process(ProcessType processType);
    IngredientState CurrentState { get; }
}