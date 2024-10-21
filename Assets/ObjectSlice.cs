using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; // Ensure you're using UnityEngine.UI for the Slider

public class ObjectSlice : MonoBehaviour, IPointerClickHandler
{
    public GameObject ObjectState1;
    public GameObject ObjectState2Prefab;
    public GameObject sliderPrefab; 

    public int sliceProgress = 5;  // Set a default slice progress

    private GameObject instantiatedObjectState2;
    private Slider slider;  
    private int sliceState = 0;
    private int counterSlice = 0;

    public PickableObject pickableObject;
    private GameObject sliderInstance;  // Reference to the instantiated slider

    void Start()
    {
        Debug.Log("CarrotSlice script attached to: " + name);
        ObjectState1.SetActive(true);
        
        
    }

    // This method will detect a click or tap
    public void OnPointerClick(PointerEventData eventData)
    {
        if (pickableObject.isReadyToBeClickable)
        {
            // Instantiate the slider only when it's ready to be sliced
            if (sliderInstance == null)
            {
                Vector3 sliderPosition = ObjectState1.transform.position;
                sliderInstance = Instantiate(sliderPrefab, sliderPosition, Quaternion.identity, transform);
                slider = sliderInstance.GetComponentInChildren<Slider>();
                Debug.Log("Slider instantiated! "+ slider);
                slider.maxValue = sliceProgress;
                slider.value = counterSlice; 
                
                Debug.Log("Max Value: " + slider.maxValue + " | CounterSlice: " + counterSlice);
            }

            IncrementCounterSlice();

            if (!IsReadyToSlice()) return;
            
            Debug.Log(name + " Game Object Clicked and is ready to slice!");
            SliceObject();
        }
        else
        {
            Debug.Log("Object is not ready to be sliced.");
        }
    }

    private void IncrementCounterSlice()
    {
        counterSlice += 1;
        slider.value = counterSlice;  // Update the slider UI
        Debug.Log("Object Clicked! CounterSlice: " + counterSlice);
    }

    private bool IsReadyToSlice()
    {
        return counterSlice == sliceProgress;
    }

    void SliceObject()
    {
        if (sliceState == 0)
        {
            instantiatedObjectState2 = Instantiate(ObjectState2Prefab, ObjectState1.transform.position, ObjectState1.transform.rotation);

            // Disable and destroy the first state
            ObjectState1.SetActive(false);
            Destroy(ObjectState1);

            sliceState = 1;
            Debug.Log("Object sliced!");

            // After slicing is done, hide or destroy the slider
            if (sliderInstance != null)
            {
                Destroy(sliderInstance);  // You can also disable it if you don't want to destroy it
            }
        }
        else
        {
            Debug.Log("Object is already fully sliced!");
        }
    }
}
