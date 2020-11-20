using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] private bool _taken = false;
    [SerializeField] private GameObject _item;

    public bool Taken => _taken;

    public GameObject Item
    {
        get => _item;
        set
        {
            _item = value;
            _taken = true;
        }
    }

    public void Remove()
    {
        Item = null;
        _taken = false;
    }
}
