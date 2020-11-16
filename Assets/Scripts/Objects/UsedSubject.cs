using UnityEngine;
using UnityEngine.UI;

public class UsedSubject : ActiveObject
{
    [SerializeField] private Vector3 _positionOffset;
    [SerializeField] private Vector3 _newRotation;
    [SerializeField] private Image _image;
    [SerializeField] private GameObject _mesh;
    [SerializeField] private Vector3 _newScale;
    [SerializeField] private Collider _firstCollider;
    [SerializeField] private int _keyNumber = -1;
    [SerializeField] private int _actionСounter = 1;

    public Image Image => _image;
    public int KeyNumber => _keyNumber;

    enum State { NotFind, Take, InInventory}
    [SerializeField] private State _state = State.NotFind;


    public void SetImage(ItemSlot slot)
    {
        _firstCollider.enabled = false;
        _image.enabled = true;
        transform.parent = slot.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _newScale;
    }

    public override void UseObject(PlayerInteraction playerInteraction)
    {
        if (_state == State.NotFind)
        {
            transform.parent = playerInteraction.ObjectPoint.transform;
            transform.position = playerInteraction.ObjectPoint.transform.position + _positionOffset;
            transform.localEulerAngles = _newRotation;
            _state = State.Take;
        }
        else if (_state == State.Take)
        {
            playerInteraction.Inventory.AddItem(this.gameObject);
            if (_mesh) _mesh.SetActive(false);
            //_image.gameObject.SetActive(true);
            _type = TypeObject.UsedSubject;
            _state = State.InInventory;
        }
        else if (_state == State.InInventory)
        {
            if (--_actionСounter == 0)
            {
                playerInteraction.Inventory.RemoveItem(gameObject);
                Destroy(gameObject);
            }
        }
    }

}
