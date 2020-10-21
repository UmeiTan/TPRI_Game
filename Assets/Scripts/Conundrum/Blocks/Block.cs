using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    enum DirectionDrag { Horizontal, Vertical };
    [SerializeField] DirectionDrag _direction = DirectionDrag.Horizontal;
    [SerializeField] Rigidbody _rigidbody;
    [SerializeField] BlocksGame _blocksGame;

    Vector3 _pos;
    private void Start()
    {
        _pos = transform.position;
        //rigidbody.velocity = new Vector3(0, 10, 0);
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("Drag");

        if (_direction == DirectionDrag.Horizontal)
        {
            transform.position = new Vector3(eventData.pointerCurrentRaycast.worldPosition.x, _pos.y, _pos.z);
            Vector3 vector3 = (transform.position.x < _blocksGame.Minimum.x || transform.position.x > _blocksGame.Maximum.x) ? 
                Vector3.zero : transform.position - _rigidbody.position;
            _rigidbody.velocity = new Vector3(vector3.x * 10, 0, 0);
        }
        else
        {
            transform.position = new Vector3(_pos.x, eventData.pointerCurrentRaycast.worldPosition.y, _pos.z);
            Vector3 vector3 = (transform.position.y < _blocksGame.Minimum.y || transform.position.y > _blocksGame.Maximum.y) ? 
                Vector3.zero : transform.position - _rigidbody.position;
            _rigidbody.velocity = new Vector3(0, vector3.y * 10, 0);
        }
    }
    public void OnBeginDrag(PointerEventData eventData)
    {

    }
    public void OnEndDrag(PointerEventData eventData)
    {
        _rigidbody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
    }
}
