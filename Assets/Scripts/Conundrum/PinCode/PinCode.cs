using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinCode : MonoBehaviour
{
    [Serializable] private class Combination
    {
        [SerializeField] private GameObject _checkPoint;
        [SerializeField] private Material _winMaterial;
        private MeshRenderer _meshRenderer;
        public bool Win { get; private set; }
        Combination()
        {
            _meshRenderer = _checkPoint.GetComponent<MeshRenderer>();
            if (_meshRenderer.material == _winMaterial) Win = true;
        }

        public void ChangeMaterial(Material material)
        {
            _meshRenderer.material = material;
            if (material == _winMaterial) Win = true;
        }
    }




}
