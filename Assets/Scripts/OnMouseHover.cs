using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class OnMouseHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector3 cachedScale;
    Vector3 cachedPosition;
    Quaternion cachedRotation;

    void Start()
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        cachedScale = transform.localScale;
        cachedPosition = transform.position;
        cachedRotation = transform.rotation;

        transform.localScale = new Vector3(1.7f, 1.7f, 1.7f);
        transform.SetPositionAndRotation(new Vector3(cachedPosition.x, cachedPosition.y + ((cachedPosition.y > 450 ? -1 : 1) * 100), cachedPosition.z), Quaternion.identity);
    }

    public void OnPointerExit(PointerEventData eventData)
    {

        transform.localScale = cachedScale;
        transform.SetPositionAndRotation(cachedPosition, Quaternion.identity);
    }
}