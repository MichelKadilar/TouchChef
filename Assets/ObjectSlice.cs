using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ObjectSlice : MonoBehaviour, IPointerClickHandler
{
    public GameObject ObjectState1;
    public GameObject ObjectState2Prefab;
    
    public int sliceProgress = 0; 

    private GameObject instantiatedObjectState2;
    private int sliceState = 0;
    private int counterSlice = 0;
    
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
        if (pickableObject.isReadyToBeClickable)
        {
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