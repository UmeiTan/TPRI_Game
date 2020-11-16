using UnityEngine;

public class ItemSlot : MonoBehaviour
{
    [SerializeField] bool _taken = false;
    GameObject _item;

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
