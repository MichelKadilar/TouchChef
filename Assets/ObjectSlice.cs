using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectSlice : MonoBehaviour, IPointerClickHandler
{
    public GameObject ObjectState1;
    public GameObject ObjectState2Prefab; // Prefab for the second state

    private GameObject instantiatedObjectState2; // To hold the instantiated second state
    private int sliceState = 0;
    
    public PickableObject pickableObject;

    void Start()
    {
        Debug.Log("CarrotSlice script attached to: " + name);
        ObjectState1.SetActive(true);
        
    }

    public void isClicked()
    {
        Debug.Log("ObjectSlice script attached to: " + name);
    }

    // This method will detect a click or tap
    public void OnPointerClick(PointerEventData eventData)
    {
        // Check if the object is ready to be sliced
        if (pickableObject.isReadyToBeClickable)
        {
            Debug.Log(name + " Game Object Clicked and is ready to slice!");
            SliceObject();
        }
        else
        {
            Debug.Log("Object is not ready to be sliced.");
        }
    }

    void SliceObject()
    {
        if (sliceState == 0)
        {
            // Instantiate ObjectState2 at the same position, rotation, and scale as ObjectState1
            instantiatedObjectState2 = Instantiate(ObjectState2Prefab, ObjectState1.transform.position, ObjectState1.transform.rotation);
            //instantiatedObjectState2.transform.localScale = ObjectState1.transform.localScale;

            // Disable the first state
            ObjectState1.SetActive(false);
            Destroy(ObjectState1);
            
            sliceState = 1;
            Debug.Log("Object sliced!");
        }
        else
        {
            Debug.Log("Object is already fully sliced!");
        }
    }
}