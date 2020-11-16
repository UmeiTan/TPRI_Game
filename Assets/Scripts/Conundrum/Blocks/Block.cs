using UnityEngine;
using UnityEngine.EventSystems;

public class Block : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private enum DirectionDrag { Horizontal, Vertical };
    [SerializeField] private DirectionDrag _direction = DirectionDrag.Horizontal;
    [SerializeField] private Rigidbody _rigidbody;
    [SerializeField] private BlocksGame _blocksGame;

    private Vector3 _pos;
    private bool _drag;
    private void Start()
    {
        _pos = transform.position;
        //rigidbody.velocity = new Vector3(0, 10, 0);
    }

    private void Update()
    {
        if (!_drag)
        {
            _rigidbody.velocity = Vector3.zero;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        _drag = true;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.localPosition = new Vector3(0, 0, transform.localPosition.z);
        _drag = false;
    }
}
