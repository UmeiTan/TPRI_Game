using System;
using UnityEngine;
using UnityEngine.Events;

public class BlocksGame : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;
    [SerializeField] private GameObject _key;
    [SerializeField] private Vector2 _minimum;
    [SerializeField] private Vector2 _maximum;
    [SerializeField] private UnityEvent _wiEvent;

    public Vector2 Minimum => _minimum;

    public Vector2 Maximum => _maximum;


    public void StartGame()
    {
        _canvas.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Finish"))
        {
            _canvas.enabled = false;
            _wiEvent.Invoke();
        }
        else
        {
            Debug.LogError("в коллайдер попал не тот объект = " + other.gameObject);
        }
    }
}
