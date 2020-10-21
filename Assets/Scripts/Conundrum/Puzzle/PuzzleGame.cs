using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PuzzleGame : MonoBehaviour
{
    [SerializeField] Puzzle[] _pazzles;
    public Puzzle[] Puzzles => _pazzles;

    [SerializeField] Vector2 _minimum;
    [SerializeField] Vector2 _maximum;

    public Vector2 Minimum { get => _minimum;
        private set => _minimum = value;
    }
    public Vector2 Maximum { get => _maximum;
        private set => _maximum = value;
    }








}
