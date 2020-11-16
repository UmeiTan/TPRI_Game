using System;
using UnityEngine;

public class PuzzlePoint : MonoBehaviour
{
    [SerializeField] private int _id;
    //public int Id => _id;
    private Collider _collider;
    private bool _conect;
    private Collider _conectCollider;

    public Collider GetConect()
    {
        if (_conect)
        {
            return _conectCollider;
        }
        else return null;
    }

    private void Start()
    {
        _collider = GetComponent<Collider>();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<PuzzlePoint>() && other.GetComponent<PuzzlePoint>()._id == _id)
        {
            _conect = true;
            _conectCollider = other;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other == _conectCollider)
        {
            _conect = false;
            _conectCollider = null;
        }
    }
}