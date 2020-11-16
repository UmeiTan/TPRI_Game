using System.Collections;
using UnityEngine;

public class ActiveSubject : ActiveObject
{
    [SerializeField] private GameObject[] _activeObjects;
    [SerializeField] private int _activeFunction = -1;
    [SerializeField] private bool _active;
    [SerializeField] private bool _locked;
    [SerializeField] private int _lockNumber = -1;
    [SerializeField] private string _help = "?";
    [SerializeField] private float _delay = 2;
    [SerializeField] private bool _moveCamera;
    [SerializeField] private Vector3 _newPos;
    [SerializeField] private Vector3 _newRot;
    //[SerializeField] private bool _changeCamera = false;
    //[SerializeField] private GameObject _newCamera;

    private Vector3 _pos;
    private Vector3 _rot;
    private int _counter = 0;

    public override void UseObject(PlayerInteraction playerInteraction)
    {
        if (_locked)
        {
            if (playerInteraction.GetUsedSubject() != null)
            {
                if (playerInteraction.GetUsedSubject().KeyNumber == _lockNumber && _lockNumber != -1)
                {

                    ActiveFunction(playerInteraction);
                    playerInteraction.SetUsedObject(null);
                }
            }
            else
            {
                if (_help != "?") playerInteraction.TextOutput(_help, _delay);
            }
        }
        else
        {
            if (_active)
            {
                ActiveFunction(playerInteraction);
            }
            if (_moveCamera)
            {
                StartCoroutine(MoveCamera(playerInteraction));
            }

        }
    }

    private IEnumerator MoveCamera(PlayerInteraction playerInteraction)
    {
        playerInteraction.enabled = false;
        Transform camera = playerInteraction.GetCamera().transform;
        _pos = camera.position;
        _rot = camera.eulerAngles;
        Vector3 shiftPos = new Vector3((_newPos.x - camera.position.x) / 100, (_newPos.y - camera.position.y) / 100,
            (_newPos.z - camera.position.z) / 100);
        Vector3 shiftRot = new Vector3((_newRot.x - camera.eulerAngles.x) / 100,
            (_newRot.y - camera.eulerAngles.y) / 100, (_newRot.z - camera.eulerAngles.z) / 100);
        ;
        for (int i = 0; i < 100; i++)
        {
            camera.position += shiftPos;
            camera.eulerAngles += shiftRot;
            yield return new WaitForSeconds(0.01f);
        }

        StartCoroutine(Waiting(playerInteraction));
        yield break;
    }

    private IEnumerator Waiting(PlayerInteraction playerInteraction)
    {
        //if (_changeCamera)
        //{
        //    _newCamera.SetActive(true);
        //    playerInteraction.GetCamera().gameObject.SetActive(false);
        //}
        while (true)
        {
            if (Input.GetKey(KeyCode.Mouse1)) break;
            yield return null;
        }
        //if (_changeCamera)
        //{
        //    playerInteraction.GetCamera().gameObject.SetActive(true);
        //    _newCamera.SetActive(false);
        //}
        if (_active) ActiveFunction(playerInteraction);
        StartCoroutine(ReturnCamera(playerInteraction));
        yield break;
    }

    private IEnumerator ReturnCamera(PlayerInteraction playerInteraction)
    {
        Transform camera = playerInteraction.GetCamera().transform;
        Vector3 shiftPos = new Vector3((_pos.x - camera.position.x) / 100, (_pos.y - camera.position.y) / 100,
            (_pos.z - camera.position.z) / 100);
        Vector3 shiftRot = new Vector3((_rot.x - camera.eulerAngles.x) / 100, (_rot.y - camera.eulerAngles.y) / 100,
            (_rot.z - camera.eulerAngles.z) / 100);
        ;
        for (int i = 0; i < 100; i++)
        {
            camera.position += shiftPos;
            camera.eulerAngles += shiftRot;
            yield return new WaitForSeconds(0.01f);
        }

        playerInteraction.enabled = true;
        yield break;
    }

    void ActiveFunction(PlayerInteraction playerInteraction)
    {
        switch (_activeFunction)
        {
            case 0: //дверь0
            {
                GetComponent<Animator>().SetBool("open", true);
                break;
            }
            case 1: //камин
            {
                _activeObjects[0].SetActive(true); //огонь
                _activeObjects[1].SetActive(true); //порванная записка
                _activeObjects[2].SetActive(true); //новый коллайдер
                StartCoroutine(ActiveFunction_0());
                GetComponent<MeshCollider>().enabled = false;
                _type = TypeObject.None;
                _locked = false;
                break;
            }
            case 2: //свеча
            {
                _activeObjects[0].SetActive(true); //огонь
                GetComponent<CapsuleCollider>().enabled = false;
                _type = TypeObject.None;
                _locked = false;
                break;
            }
            case 3: //свободное место на столе
            {
                if (_locked)
                {
                    UsedSubject subject = playerInteraction.GetUsedSubject();
                    subject.transform.parent = transform;
                    subject.transform.localPosition = Vector3.zero;
                    subject.transform.localEulerAngles = Vector3.zero;
                    subject.transform.localScale = Vector3.one;

                    if (subject.gameObject == _activeObjects[1]) //пазл 0
                    {
                        _activeObjects[2].SetActive(true); //пазл0 (спрайт)
                        _activeObjects[2].transform.parent = _activeObjects[0].transform; //_activeObjects[0] - canvas
                        _activeObjects[2].transform.localPosition = Vector3.left / 2;
                        _activeObjects[2].transform.localEulerAngles = Vector3.zero;
                    }

                    if (subject.gameObject == _activeObjects[3]) //порванная записка
                    {
                        _activeObjects[4].SetActive(true); //пазлы
                        foreach (var sprite in _activeObjects[4].GetComponentsInChildren<SpriteRenderer>())
                        {
                            sprite.color = Color.white;
                            sprite.transform.parent = _activeObjects[0].transform;
                            sprite.transform.localEulerAngles = Vector3.zero;
                            sprite.gameObject.GetComponent<Puzzle>().RandomPosition();
                        }
                    }

                    if (++_counter == 2)
                    {
                        _locked = false;
                        _active = true;
                        _activeObjects[3].GetComponent<PuzzleGame>().StartGame(_activeObjects[0].transform);
                    }
                }
                else
                {
                    _activeObjects[0].GetComponent<Canvas>().enabled = !_activeObjects[0].GetComponent<Canvas>().enabled;
                    Debug.Log("*");
                }

                break;
            }
        }


    }


    private IEnumerator ActiveFunction_0()
    {
        ParticleSystem.EmissionModule emission = _activeObjects[0].GetComponent<ParticleSystem>().emission;
        Light light = _activeObjects[0].GetComponent<Light>();
        Puzzle[] puzzles = _activeObjects[1].GetComponent<PuzzleGame>().Puzzles;
        SpriteRenderer[] sprites = new SpriteRenderer[puzzles.Length - 1];
        for (int k = 0; k < sprites.Length; k++)
        {
            sprites[k] = puzzles[k + 1].GetComponent<SpriteRenderer>();
        }

        for (float rate = 0, intensity = 0, r = 0, g = 0, b = 0; rate <= 400;
            rate += 4, intensity += 0.05f, r += 0.0025f, g += 0.0025f, b += 0.0025f)
        {
            emission.rateOverTime = rate;
            light.intensity = intensity;
            for (int k = 0; k < sprites.Length && sprites[0] != null && sprites[0].color != Color.white; k++)
            {
                sprites[k].color = new Color(r, g, b);
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }
}