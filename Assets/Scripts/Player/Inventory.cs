using UnityEngine;

public class Inventory : MonoBehaviour
{
    [SerializeField] ItemSlot[] _slots;

    ItemSlot SearchFreeSlot(GameObject item)
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            if (!_slots[i].Taken)
            {
                _slots[i].Item = item;
                return _slots[i];
            }
        }

        return null;
    }

    public void AddItem(GameObject item)
    {
        item.GetComponent<UsedSubject>().SetImage(SearchFreeSlot(item));
    }

    public void RemoveItem(GameObject item)
    {
        int i = 0;
        for (; i < _slots.Length; i++)
        {
            if (_slots[i].Item == item) break;
        }

        for (; i + 1 < _slots.Length && _slots[i + 1].Taken == true; i++)
        {
           // _slots[i].Item = _slots[i + 1].Item;
            _slots[i + 1].Item.GetComponent<UsedSubject>().SetImage(_slots[i]);
        }

        _slots[i].Remove();

    }
}