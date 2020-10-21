using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlocksGame : MonoBehaviour
{
    [SerializeField] GameObject _playerCamera;
    [SerializeField] GameObject _gameCamera;
    [SerializeField] Collider _winPoint;
    [SerializeField] Vector2 _minimum;
    [SerializeField] Vector2 _maximum;

    public Vector2 Minimum { get => _minimum;
        private set => _minimum = value;
    }
    public Vector2 Maximum { get => _maximum;
        private set => _maximum = value;
    }

    

    public void StartGame()
    {
        _gameCamera.SetActive(true);
        _playerCamera.SetActive(false);
    }

    public void EndGame()
    {
        _gameCamera.SetActive(false);
        _playerCamera.SetActive(true);
    }

    public void FinishGame()
    {
        _gameCamera.SetActive(false);
        _playerCamera.SetActive(true);
    }

}
