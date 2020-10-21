using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TypeObject { ActiveSubject, LookOnlySubject, UsedSubject, Take, None }

public class ActiveObject : MonoBehaviour
{
    #region Variables  
    [SerializeField] protected TypeObject _type = TypeObject.None;
    public TypeObject Type => _type;

    #endregion

    virtual public void UseObject(PlayerInteraction playerInteraction)
    {
        Debug.Log("UseObject");
    }
}