using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMousePosition : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"OnPointerDown: {eventData.position}");
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // 312 1248 , 58.5

        var value = (eventData.position.x - 312) / 58.6;

        Debug.Log($"OnPointerUp: {eventData.position} [{eventData.position.x} => {value}]");
    }
}
