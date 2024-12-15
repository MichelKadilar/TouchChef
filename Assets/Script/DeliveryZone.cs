using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    [Header("Audio")]
    [SerializeField] private AudioClip deliverySuccessSound;
    [SerializeField] private AudioSource audioSource;

    private void Awake()
    {
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private bool IsValidDelivery(BaseIngredient ingredient)
    {
        if (ingredient == null) return false;

        // Vérification des états valides selon le type d'ingrédient
        switch (ingredient)
        {
            case Meat meat:
                return meat.CurrentState == IngredientState.Cooked || 
                       meat.CurrentState == IngredientState.Cut;
                
            case Lettuce lettuce:
                return lettuce.CurrentState == IngredientState.Cut || 
                       lettuce.CurrentState == IngredientState.Washed;
                
            case Tomato tomato:
                return tomato.CurrentState == IngredientState.Cut;
                
            case Cheese cheese:
                return cheese.CurrentState == IngredientState.Cut;
                
            default:
                return false;
        }
    }

    
    public bool TryDeliverIngredient(BaseIngredient ingredient)
    {
        if (!IsValidDelivery(ingredient)) 
        {
            Debug.Log($"Invalid delivery for {ingredient.gameObject.name}");
            return false;
        }

        // Jouer le son
        if (audioSource != null && deliverySuccessSound != null)
        {
            audioSource.PlayOneShot(deliverySuccessSound);
        }

        // Créer et envoyer le message
        var message = new DeliveryScoreMessage
        {
            ingredientState = GetIngredientType(ingredient)
        };

        ClientWebSocket.Instance?.SendMessage(JsonUtility.ToJson(message));
        Debug.Log($"Delivery successful: {message.ingredientState}");

        Destroy(ingredient.gameObject);
        return true;
    }

    private string GetIngredientType(BaseIngredient ingredient)
    {
        if (ingredient is Meat meat)
        {
            return meat.CurrentState == IngredientState.Cooked ? "cookedSteak" : "cutSteak";
        }
        if (ingredient is Lettuce lettuce)
        {
            return lettuce.CurrentState == IngredientState.Cut ? "cutLettuce" : "washedLettuce";
        }
        if (ingredient is Tomato)
        {
            return "cutTomato";
        }
        if (ingredient is Cheese)
        {
            return "cutCheese";
        }
        return string.Empty;
    }

    private void OnTriggerEnter(Collider other)
    {
        BaseIngredient ingredient = other.GetComponent<BaseIngredient>();
        if (ingredient != null)
        {
            TryDeliverIngredient(ingredient);
        }
    }
}