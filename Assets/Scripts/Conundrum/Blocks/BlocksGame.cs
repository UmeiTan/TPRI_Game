using UnityEngine;

public class BlocksGame : MonoBehaviour
{
    [SerializeField] private GameObject _playerCamera;
    [SerializeField] private GameObject _gameCamera;
    [SerializeField] private Collider _winPoint;
    [SerializeField] private Vector2 _minimum;
    [SerializeField] private Vector2 _maximum;

    public Vector2 Minimum => _minimum;

    public Vector2 Maximum => _maximum;


    public void StartGame()
    {
        _gameCamera.SetActive(true);
        _playerCamera.SetActive(false);
    }

    public void EndGame()
    {
        _gameCamera.SetActive(false);
        _playerCamera.SetActive(true);
    }

    public void FinishGame()
    {
        _gameCamera.SetActive(false);
        _playerCamera.SetActive(true);
    }

}
