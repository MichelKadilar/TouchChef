using UnityEngine;
using UnityEngine.EventSystems;  // Required for OnPointerClick interface

public class ObjectSlice : MonoBehaviour, IPointerClickHandler
{
    public GameObject ObjectState1;
    public GameObject ObjectState2;

    private int sliceState = 0;

    void Start()
    {
        Debug.Log("CarrotSlice script attached to: " + name);
        ObjectState1.SetActive(true);
        ObjectState2.SetActive(false);
    }

    public void isClicked()
    {
        Debug.Log("ObjectSlice script attached to: " + name);
    }


    // This method will detect a click or tap
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(name + " Game Object Clicked!");
        SliceObject();
    }

    void SliceObject()
    {
        if (sliceState == 0)
        {
            ObjectState1.SetActive(false);
            ObjectState2.SetActive(true);
            sliceState = 1;
        }else {
            Debug.Log("Object is fully sliced!");
        }
    }

}
