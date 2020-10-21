using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Puzzle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] PuzzleGame _puzzleGame;
    bool _drag = false;


    void Update()
    {
       // if (_drag && Input.GetAxis("Mouse ScrollWheel") != 0)
        if (_drag)
        {
            transform.eulerAngles += (Input.GetAxis("Mouse ScrollWheel") > 0) ? 
                new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")*30f) : 
                new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel")*30f);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.pointerCurrentRaycast.worldPosition;
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0f);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _drag = true;
        transform.SetAsLastSibling();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _drag = false;
    }

}