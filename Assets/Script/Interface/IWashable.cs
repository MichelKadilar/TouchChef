
public interface IWashable 
{
    void StartWash(); // Pour démarrer l'effet visuel de l'eau
    void StopWash(); // Pour arrêter l'effet visuel de l'eau
    void DoWash(); // Appelé à chaque passage de lavage valide
    float GetCleanliness(); // Retourne le niveau de propreté actuel
    bool IsClean(); // Vérifie si l'objet est complètement propre
}
