using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

using Random = UnityEngine.Random;

public class Puzzle : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private PuzzleGame _puzzleGame;
    [SerializeField] private List<PuzzlePoint> _points;
    
    private bool _drag = false;
    private bool _dragOneObject = true;
    private Collider _connectCollider;
    private Vector3 _beginDrag;

    void Update()
    {
        if (_drag && Input.GetAxis("Mouse ScrollWheel") != 0)
        {
            if (_dragOneObject)
            {
                transform.eulerAngles += new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 30f);
            }
            else
            {
                transform.parent.eulerAngles += new Vector3(0, 0, Input.GetAxis("Mouse ScrollWheel") * 30f);
            }
        }
        else _points.RemoveAll(PointNull);
    }

    private void MovePuzzle(Vector3 worldPosition)
    {
        if(worldPosition == Vector3.zero) return;

        if (_dragOneObject)
        {
            Vector3 pos = transform.position;
            Transform newPos = transform;
            newPos.position += (worldPosition - _beginDrag);

            if (newPos.localPosition.x < _puzzleGame.Minimum.x || newPos.localPosition.x > _puzzleGame.Maximum.x)
            {
                newPos.position = new Vector3(newPos.position.x, pos.y, pos.z);
            }
            if (newPos.localPosition.y < _puzzleGame.Minimum.y || newPos.localPosition.y > _puzzleGame.Maximum.y)
            {
                newPos.position = new Vector3(pos.x, pos.y, newPos.position.z);
            }

           // transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, -0.005f);
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }
        else
        {
            Vector3 pos;
            Vector3 delta = (worldPosition - _beginDrag);
            bool move = true;
            foreach (Transform puzzle in transform.parent)
            {
                pos = puzzle.position;
                puzzle.position += delta;

                if ((puzzle.localPosition.x < _puzzleGame.Minimum.x || puzzle.localPosition.x > _puzzleGame.Maximum.x) 
                    || (puzzle.localPosition.y < _puzzleGame.Minimum.y || puzzle.localPosition.y > _puzzleGame.Maximum.y))
                {
                    move = false;
                    puzzle.position = pos;
                    break;
                }

                puzzle.position = pos;
            }

            if (move)
                foreach (Transform puzzle in transform.parent)
                {
                    puzzle.position += delta;
                    puzzle.localPosition = new Vector3(puzzle.localPosition.x, puzzle.localPosition.y, 0);
                }
        }


        _beginDrag = worldPosition;

    
    }

    public void OnDrag(PointerEventData eventData)
    {
        MovePuzzle(eventData.pointerCurrentRaycast.worldPosition);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _drag = true;
        if (_dragOneObject)
        {
            transform.SetAsLastSibling();
        }
        else
        {
            transform.parent.SetAsLastSibling();
        }
        _beginDrag = eventData.pointerCurrentRaycast.worldPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_dragOneObject)
        {
            transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, 0);
        }
        else
        {
            transform.parent.localPosition = new Vector3(transform.parent.localPosition.x, transform.parent.localPosition.y, 0);
        }
        Connection();
        _drag = false;
    }

    private void Connection(bool rek = false)
    {
        foreach (var point in _points) //проверяем все поинты взятого пазла
        {
            _connectCollider = point.GetConect();
            if (_connectCollider) //получен коллайдер поинта лежащего пазла 
            {
                if (ValidateConnect()) //повороты пазлов совпадают
                {
                    Destroy(point.gameObject);
                    Destroy(_connectCollider.gameObject);
                    //Debug.Log("-point   " + point);
                    _connectCollider.GetComponentInParent<Puzzle>()._points.Remove(_connectCollider.GetComponent<PuzzlePoint>());

                    if (transform.parent.GetComponent<Canvas>()) //взятый пазл один
                    {
                        Vector3 delta = _puzzleGame.GetDeltaPosition(this, _connectCollider.GetComponentInParent<Puzzle>());
                        if (_connectCollider.transform.parent.parent.GetComponent<Canvas>()) //лежащий пазл один
                        {
                            Debug.Log("1.1-point   " + point);
                            var puzzleGroup = Instantiate(_puzzleGame.PuzzleGroup, transform.parent.transform);
                            puzzleGroup.transform.localEulerAngles = transform.localEulerAngles;

                            transform.parent = puzzleGroup.transform;
                            _connectCollider.transform.parent.parent = puzzleGroup.transform;

                            _dragOneObject = false;
                            _connectCollider.GetComponentInParent<Puzzle>()._dragOneObject = false;

                            _connectCollider.transform.parent.transform.localEulerAngles = Vector3.zero;
                            transform.localEulerAngles = Vector3.zero;

                            _connectCollider.transform.parent.transform.localPosition =
                                new Vector3(transform.localPosition.x + delta.x, transform.localPosition.y + delta.y, 0);
                        }
                        else //лежащий пазл в группе
                        {
                            Debug.Log("1.2-point   " + point);
                            Transform temp = _connectCollider.transform.parent;
                            transform.parent = temp.parent;
                            _dragOneObject = false;
                            transform.localEulerAngles = Vector3.zero;
                            transform.localPosition = new Vector3(temp.localPosition.x - delta.x, temp.localPosition.y - delta.y, 0);
                        }
                    }
                    else //взятый пазл в группе
                    {
                        if (_connectCollider.transform.parent.parent.GetComponent<Canvas>()) //лежащий пазл один
                        {
                            Debug.Log("2.1-point   " + point);
                            Vector3 delta = _puzzleGame.GetDeltaPosition(this, _connectCollider.GetComponentInParent<Puzzle>());
                            Transform temp = _connectCollider.transform.parent;
                            temp.parent = transform.parent.transform;
                            temp.localEulerAngles = Vector3.zero;
                            _connectCollider.GetComponentInParent<Puzzle>()._dragOneObject = false;

                            temp.transform.localPosition =
                                new Vector3(transform.localPosition.x + delta.x, transform.localPosition.y + delta.y, 0);
                            
                            if(!rek) temp.GetComponent<Puzzle>().Connection(true);

                        }
                        else //лежащий пазл в группе
                        {
                            if (point.transform.parent.parent == _connectCollider.transform.parent.parent)
                            {
                                Debug.Log("2.0-point   " + point);
                                continue; //пазлы в одной группе
                            }

                            Transform puzzles = _connectCollider.transform.parent.parent;
                            if (transform.parent.childCount >= puzzles.childCount) //взятых пазлов в группе больше
                            {
                                Debug.Log("2.2-point   " + point);
                                while (puzzles.childCount != 0) //перекидываем лежащие пазлы в группу взятых
                                {
                                    Transform puzzle = puzzles.GetChild(0);
                                    Vector3 delta = _puzzleGame.GetDeltaPosition(this, puzzle.GetComponent<Puzzle>());
                                    puzzle.parent = transform.parent.transform;
                                    puzzle.localEulerAngles = Vector3.zero;

                                    puzzle.transform.localPosition =
                                        new Vector3(transform.localPosition.x + delta.x, transform.localPosition.y + delta.y, 0);
                                    
                                    if (!rek) puzzle.GetComponent<Puzzle>().Connection(true);
                                }
                                Destroy(puzzles.gameObject);
                            }
                            else //лежащих пазлов в группе больше
                            {
                                Debug.Log("2.3-point   " + point);
                                puzzles = transform.parent;
                                while (puzzles.childCount != 0) //перекидываем взятые пазлы в группу лежащих
                                {
                                    Transform puzzle = transform.parent.GetChild(0);
                                    Vector3 delta = _puzzleGame.GetDeltaPosition(_connectCollider.GetComponentInParent<Puzzle>(), 
                                                                                 puzzle.GetComponent<Puzzle>());
                                    puzzle.parent = _connectCollider.transform.parent.parent.transform;
                                    puzzle.localEulerAngles = Vector3.zero;

                                    puzzle.transform.localPosition =
                                        new Vector3(_connectCollider.transform.parent.localPosition.x + delta.x,
                                            _connectCollider.transform.parent.localPosition.y + delta.y, 0);
                                   
                                    if (!rek) puzzle.GetComponent<Puzzle>().Connection(true);
                                }
                                Destroy(puzzles.gameObject);
                            }
                        }
                    }
                    _puzzleGame.CheckAllPuzzlePoints();
                }

                _connectCollider = null;
            }
        }
        
    }

    private bool PointNull(PuzzlePoint point)
    {
        return point == null;
    }

    private bool ValidateConnect()
    {
        Vector3 onePuzzle = (transform.parent.GetComponent<Canvas>())
            ? transform.localEulerAngles
            : transform.parent.localEulerAngles;
        Vector3 twoPuzzle = (_connectCollider.transform.parent.parent.GetComponent<Canvas>())
            ? _connectCollider.transform.parent.localEulerAngles
            : _connectCollider.transform.parent.parent.localEulerAngles;
        return twoPuzzle.z - onePuzzle.z < 4 && twoPuzzle.z - onePuzzle.z > -4;
    }

    public void RandomPosition()
    {
        transform.localPosition = new Vector3(Random.Range(0, _puzzleGame.Maximum.x),
            Random.Range(_puzzleGame.Minimum.y, _puzzleGame.Maximum.y), 0);
        transform.localEulerAngles = new Vector3(0,0, Random.Range(-180, 180));
    }
}