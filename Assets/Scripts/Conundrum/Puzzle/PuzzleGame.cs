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

    public Puzzle[] Puzzles => _pazzles;
    public Vector2 Minimum => _minimum;
    public Vector2 Maximum => _maximum;
    public GameObject PuzzleGroup => _puzzleGroup;

    private List<PuzzlePoint> _puzzlePoints = new List<PuzzlePoint>();
    private GameObject _temp;
    public void StartGame(Transform parent)
    {
        _temp = parent.gameObject;
        _puzzlePoints.AddRange(parent.GetComponentsInChildren<PuzzlePoint>());
        //foreach (var puzzle in _pazzles)
        //{
        //    puzzle.GetComponent<SpriteRenderer>().enabled = false;
        //}

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
            _temp.transform.GetChild(0).transform.localEulerAngles = Vector3.zero;
            _temp.transform.GetChild(0).FindChild("puzzle 0").GetChild(0).GetComponent<Image>().color = Color.white;
        }
    }

    private bool PointNull(PuzzlePoint point)
    {
        return point == null;
    }
}
