using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PinCode : MonoBehaviour
{
    [SerializeField] private Material[] _materials;
    [Serializable] private class Combination
    {
        [SerializeField] private GameObject _down;
        [SerializeField] private GameObject _checkPoint;
        [SerializeField] private GameObject _up;
        [SerializeField] private int _idWinMaterial;
        private MeshRenderer _meshRendererDown;
        private MeshRenderer _meshRenderer;
        private MeshRenderer _meshRendererUp;
        private int _number = 0;
        public bool Win { get; private set; }

        
        public void combination()
        {
            _meshRendererDown = _down.GetComponent<MeshRenderer>();
            _meshRenderer = _checkPoint.GetComponent<MeshRenderer>();
            _meshRendererUp = _up.GetComponent<MeshRenderer>();
            if (_number == _idWinMaterial) Win = true;
        }

        public void Up(PinCode pinCode)
        {
            _number = (_number + 1 == pinCode._materials.Length) ? 0 : _number + 1;
            _meshRendererDown.material = _meshRenderer.material;
            _meshRenderer.material = _meshRendererUp.material;
            _meshRendererUp.material = pinCode._materials[(_number + 1 == pinCode._materials.Length) ? 0 : _number + 1];
            if (_number == _idWinMaterial) Win = true;
        }
        public void Down(PinCode pinCode)
        {
            _number = (_number == 0) ? pinCode._materials.Length - 1 : _number - 1;
            _meshRendererUp.material = _meshRenderer.material;
            _meshRenderer.material = _meshRendererDown.material;
            _meshRendererDown.material = pinCode._materials[(_number == 0) ? pinCode._materials.Length - 1 : _number - 1];
            if (_number == _idWinMaterial) Win = true;
        }
    }

    [SerializeField] private Combination[] _combinations;
    [SerializeField] private UnityEvent _WinEvent;
    private bool _check;
    private void Start()
    {
        foreach (var point in _combinations)
        {
            point.combination();
        }
    }

    public void Up(int idRing)
    {
        _combinations[idRing].Up(this);
        if (!_check) StartCoroutine(Check());
    }
    public void Down(int idRing)
    {
        _combinations[idRing].Down(this);
        if (!_check) StartCoroutine(Check());
    }

    private IEnumerator Check()
    {
        _check = true;
        while (true)
        {
            if (Input.GetKeyUp(KeyCode.Mouse1)) break;
            yield return null;
        }
        foreach (var point in _combinations)
        {
            if (!point.Win)
            {
                _check = false;
                yield break;
            }
        }
        _WinEvent.Invoke();
        _check = false;
    }

}