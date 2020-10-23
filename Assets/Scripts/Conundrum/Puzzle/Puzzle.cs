using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

// ReSharper disable once CheckNamespace
public class Puzzle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private PuzzleGame _puzzleGame;

    [Serializable] public class Point
    {
        [SerializeField] private Collider _collider;
        [SerializeField] private Collider _collider2;
        private bool _conect;
    }

    [SerializeField] private Point[] _points;
    private bool _drag = false;


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