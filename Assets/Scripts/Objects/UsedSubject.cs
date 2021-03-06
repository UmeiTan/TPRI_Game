﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UsedSubject : ActiveObject
{
    [SerializeField] Vector3 _positionOffset;
    [SerializeField] Vector3 _newRotation;
    [SerializeField] Image _image;
    public Image Image => _image;
    [SerializeField] GameObject _mesh;
    [SerializeField] Vector3 _newScale;
    [SerializeField] Collider _firstCollider;
    [SerializeField] int _keyNumber = -1;
    public int KeyNumber => _keyNumber;
    [SerializeField] int _actionСounter = 1;
    enum State { NotFind, Take, InInventory}
    [SerializeField] State _state = State.NotFind;


    public void SetImage(ItemSlot slot)
    {
        _firstCollider.enabled = false;
        _image.enabled = true;
        transform.parent = slot.transform;
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = _newScale;
    }

    override public void UseObject(PlayerInteraction playerInteraction)
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
