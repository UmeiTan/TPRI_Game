using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    #region Переменные
    [SerializeField] Texture2D _cursorStandart; //_stateCursor = 0
    [SerializeField] Texture2D _cursorTake;     //_stateCursor = 1
    [SerializeField] Texture2D _cursorUse;      //_stateCursor = 2
    [SerializeField] Texture2D _cursorLoupe;    //_stateCursor = 3
    [SerializeField] Camera _playerCamera;
    [SerializeField] GameObject _objectPoint;
    public GameObject ObjectPoint => _objectPoint;
    [SerializeField] float _maxRange; //расстояние с которого можно взаимодействовать с предметами
    [SerializeField] Inventory _inventory;
    public Inventory Inventory => _inventory;
    [SerializeField] GameObject _textBoard;
    [SerializeField] Text _text;


    PlayerMove _playerMove;
    bool _stateMove = true; //персонаж может двигаться
    int _stateCursor = 0;
    GameObject _usedObject;
    ActiveObject _activeObject;
    bool _textBoardActive = false;
    bool _textBoardCoroutine = false;
    #endregion

    #region Доступ к переменным

    public void SetUsedObject(GameObject item)
    {
        if (!_usedObject)
        {
            _usedObject = item;
            _activeObject = item.GetComponent<ActiveObject>();
            Texture2D texture = _usedObject.GetComponent<UsedSubject>().Image.sprite.texture;      
            Cursor.SetCursor(texture, Vector2.zero, CursorMode.ForceSoftware);
            _stateCursor = 5;   //иконка объекта вместо курсора
        }
        else if (item == null)
        {
            _activeObject.UseObject(this);
            _usedObject = null;
            _activeObject = null;
            Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
            _stateCursor = 0;
        }
    }
    public UsedSubject GetUsedSubject()
    {
        return _usedObject == null ? null : _usedObject.GetComponent<UsedSubject>();
    }
    public void TextOutput(string text, float delay)
    {
        this._text.text = text;
        if (_textBoardCoroutine)
        {
            _textBoardActive = true;
        }
        else
        {
            StartCoroutine(TextBoard(delay));
        }
        
    }
    public Camera GetCamera() { return _playerCamera; }
    
    #endregion

    void Start()
    {
        Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
        _stateCursor = 0;
        _playerMove = gameObject.GetComponent<PlayerMove>();
    }

    void Update()
    {
        if (_stateMove) //режим движения
        {
            if (Input.GetKeyDown(KeyCode.Mouse0)) //переход в режим выбора объектов
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                _playerMove.SwitchState(false);
                _stateMove = false;
                Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                _stateCursor = 0;
            }
        }
        else //режим выбора объектов
        {
            if (!_usedObject) //игрок ни с чем не взаимодействует
            {
                _activeObject = null;
                if (Input.GetKeyDown(KeyCode.Mouse1)) //переход в режим движения
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    _playerMove.SwitchState(true);
                    _stateMove = true;
                    _stateCursor = 0;
                }

                RaycastHit hit;
                Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hit) && hit.transform.GetComponent<ActiveObject>()) 
                {
                    Transform objectHit = hit.transform;
                    Vector3 heading = objectHit.position - transform.position;
                    if (heading.sqrMagnitude < _maxRange * _maxRange)
                    {
                        _activeObject = objectHit.GetComponent<ActiveObject>();
                        TypeObject type = _activeObject.Type;

                        #region Изменение курсора
                        if (type == TypeObject.ActiveSubject && _stateCursor != 1)
                        {
                            Cursor.SetCursor(_cursorUse, Vector2.zero, CursorMode.ForceSoftware);
                            _stateCursor = 1;
                        }
                        else if (type == TypeObject.LookOnlySubject && _stateCursor != 2)
                        {
                            Cursor.SetCursor(_cursorLoupe, Vector2.zero, CursorMode.ForceSoftware);
                            _stateCursor = 2;
                        }
                        else if (type == TypeObject.UsedSubject || type == TypeObject.Take && _stateCursor != 3)
                        {
                            Cursor.SetCursor(_cursorTake, Vector2.zero, CursorMode.ForceSoftware);
                            _stateCursor = 3;
                        }

                        #endregion

                        if (Input.GetKeyDown(KeyCode.Mouse0) && type != TypeObject.None)
                        {
                            if (type == TypeObject.ActiveSubject)
                            {
                                _activeObject = objectHit.GetComponent<ActiveSubject>();
                            }
                            else if (type == TypeObject.Take || type == TypeObject.UsedSubject)
                            {
                                _activeObject = objectHit.GetComponent<UsedSubject>();
                                _usedObject = (type == TypeObject.Take) ? objectHit.gameObject : null;
                            }
                            else if (type == TypeObject.LookOnlySubject)
                            {
                                _activeObject = objectHit.GetComponent<LookOnlySubject>();
                                _usedObject = objectHit.gameObject;
                                Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                                _stateCursor = 0;
                            }
                            _activeObject.UseObject(this);
                        }

                    }
                    else if (_stateCursor != 0)
                    {
                        Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                        _stateCursor = 0;
                    }

                }
                else if (_stateCursor != 0)
                {
                    Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                    _stateCursor = 0;
                }
            }
            else //игрок взаимодействует с объектом
            {
                TypeObject type = _activeObject.Type;

                if (Input.GetKeyDown(KeyCode.Mouse0))
                {
                    if (type == TypeObject.LookOnlySubject)
                    {
                        _activeObject.UseObject(this);
                        _usedObject = null;
                    }
                    else if (type == TypeObject.Take)
                    {
                        _activeObject.UseObject(this);
                        _usedObject = null;
                        Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                        _stateCursor = 0;
                    }
                    else if (type == TypeObject.UsedSubject)
                    {
                        RaycastHit hit;
                        Ray ray = _playerCamera.ScreenPointToRay(Input.mousePosition);

                        if (Physics.Raycast(ray, out hit) && hit.transform.GetComponent<ActiveObject>())
                        {
                            ActiveObject active = hit.transform.GetComponent<ActiveObject>();
                            if (active.Type == TypeObject.ActiveSubject)
                            {
                                active.UseObject(this);
                            }
                        }
                    }

                }
                else if (Input.GetKeyDown(KeyCode.Mouse1))
                {
                    if (type == TypeObject.LookOnlySubject || type == TypeObject.Take)
                    {
                        _activeObject.UseObject(this);
                        _usedObject = null;
                        _activeObject = null;

                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                        _playerMove.SwitchState(true);
                        _stateMove = true;
                        _stateCursor = 0;
                    }
                    else if (type == TypeObject.UsedSubject)
                    {
                        _usedObject = null;
                        _activeObject = null;
                        Cursor.SetCursor(_cursorStandart, Vector2.zero, CursorMode.ForceSoftware);
                        _stateCursor = 0;
                        
                    }
                }
            }
        }

    }

    private IEnumerator TextBoard(float delay)
    {
        Vector3 pos = _textBoard.transform.localPosition;
        _textBoardActive = true;
        _textBoardCoroutine = true;
        while (_textBoardActive)
        {
            for (; pos.y < 1.5f; pos.y += 0.1f)
            {
                _textBoard.transform.localPosition = pos;
                yield return new WaitForSeconds(0.03f);
            }
            yield return new WaitForSeconds(delay);
            _textBoardActive = false;
            for (; pos.y > 0 && !_textBoardActive; pos.y -= 0.1f)
            {
                _textBoard.transform.localPosition = pos;
                yield return new WaitForSeconds(0.05f);
            }
        }
        _textBoardCoroutine = false;
        yield break;
    }

}
