using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class PuzzleGame : MonoBehaviour
{
    [SerializeField] private Puzzle[] _pazzles;
    [SerializeField] private Vector3[] _pazzlesPos;
    [SerializeField] private Vector2 _minimum;
    [SerializeField] private Vector2 _maximum;
    [SerializeField] private GameObject _puzzleGroup;
    [SerializeField] private float shiftX;
    public Puzzle[] Puzzles => _pazzles;
    public Vector2 Minimum => _minimum;
    public Vector2 Maximum => _maximum;
    public GameObject PuzzleGroup => _puzzleGroup;

    private List<PuzzlePoint> _puzzlePoints = new List<PuzzlePoint>();
    private GameObject _temp;
    public void StartGame(Transform parent)
    {
        _temp = parent.gameObject; //canvas
        _puzzlePoints.AddRange(parent.GetComponentsInChildren<PuzzlePoint>());
    }

    public Vector3 GetDeltaPosition(Puzzle onePuzzle, Puzzle twoPuzzle)
    {
        return _pazzlesPos[Array.IndexOf(_pazzles, twoPuzzle)] - _pazzlesPos[Array.IndexOf(_pazzles, onePuzzle)];
    }

    public void CheckAllPuzzlePoints()
    {
        _puzzlePoints.RemoveAll(PointNull);
        if (_puzzlePoints.Count == 0 || _temp.transform.childCount == 1)
        {
            Transform temp = _temp.transform.GetChild(0);
            temp.transform.localEulerAngles = Vector3.zero;
            temp = temp.Find("puzzle 0").GetChild(0);
            temp.gameObject.SetActive(true);
            temp.GetComponent<Image>().color = Color.white;
            temp.GetComponent<SpriteRenderer>().enabled = true;
            for(int i = 0; i < _pazzles.Length; i++)
            {
                _pazzles[i].transform.localPosition = new Vector3(_pazzlesPos[i].x + shiftX, _pazzlesPos[i].y, _pazzlesPos[i].z);
            }
            _temp.GetComponentInParent<ActiveSubject>().Inactive();
            _temp.GetComponent<Canvas>().enabled = false;
        }
    }

    private bool PointNull(PuzzlePoint point)
    {
        return point == null;
    }
}
