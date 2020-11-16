using UnityEngine;
using UnityEngine.Events;

public class TriggerEvent : MonoBehaviour
{
    [SerializeField] private UnityEvent _onTriggerEnterEvent;

    private void OnTriggerEnter(Collider other)
    {
        _onTriggerEnterEvent.Invoke();
    }
}
