using UnityEngine;

public class LookOnlySubject : ActiveObject
{
    [SerializeField] GameObject _parent;

    Vector3 _originalPosition;
    [SerializeField] Vector3 _positionOffset;

    Quaternion _originalRotation;
    [SerializeField] Vector3 _newRotation;

    bool _active = false;

    void Start()
    {
        _originalPosition = transform.position;
        _originalRotation = transform.rotation;
    }

    override public void UseObject(PlayerInteraction playerInteraction)
    {
        if(!_active)
        {
            transform.parent = playerInteraction.ObjectPoint.transform;
            transform.position = playerInteraction.ObjectPoint.transform.position + _positionOffset;
            transform.localEulerAngles = _newRotation;
            _active = true;
        }
        else
        {        
            transform.position = _originalPosition;
            transform.rotation = _originalRotation;
            transform.parent = _parent.transform;
            _active = false;
        }
    }
}