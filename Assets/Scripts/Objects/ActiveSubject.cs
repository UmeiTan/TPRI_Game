using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActiveSubject : ActiveObject
{
    [SerializeField] GameObject[] _active;
    [SerializeField] int _activeFunction = -1;
    [SerializeField] bool _locked;
    [SerializeField] int _lockNumber = -1;
    [SerializeField] string _help = "?";
    [SerializeField] float _delay = 2;
    [SerializeField] bool _moveCamera = false;
    [SerializeField] Vector3 _newPos;
    [SerializeField] Vector3 _newRot;

    Vector3 _pos;
    Vector3 _rot;
    int _counter = 0;

    override public void UseObject(PlayerInteraction playerInteraction)
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

        if (_moveCamera)
        {
            StartCoroutine(MoveCamera(playerInteraction));
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
        while (true)
        {
            if (Input.GetKey(KeyCode.Mouse1)) break;
            yield return null;
        }

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
            case 0: //камин
            {
                _active[0].SetActive(true); //огонь
                _active[1].SetActive(true); //порванная записка
                _active[2].SetActive(true); //новый коллайдер
                StartCoroutine(ActiveFunction_0());
                GetComponent<MeshCollider>().enabled = false;
                _type = TypeObject.None;
                _locked = false;
                break;
            }
            case 1: //свеча
            {
                _active[0].SetActive(true); //огонь
                GetComponent<CapsuleCollider>().enabled = false;
                _type = TypeObject.None;
                _locked = false;
                break;
            }
            case 2: //свободное место на столе
            {
                UsedSubject subject = playerInteraction.GetUsedSubject();
                subject.transform.parent = transform;
                subject.transform.localPosition = Vector3.zero;
                subject.transform.localEulerAngles = Vector3.zero;
                subject.transform.localScale = Vector3.one;
                if (subject.gameObject == _active[1]) //пазл 0
                {
                    _active[2].SetActive(true); //пазл0 (спрайт)
                    _active[3].SetActive(false); //пазл0 (инвентарь)
                }

                if (subject.gameObject == _active[4]) //порванная записка
                {
                    _active[5].SetActive(true); //пазлы
                    foreach (var sprite in _active[5].GetComponentsInChildren<SpriteRenderer>())
                    {
                        sprite.color = new Color(1,1,1);
                        sprite.gameObject.transform.parent = _active[0].transform;
                    }
                }

                if (++_counter == 2)
                {
                    _active[0].SetActive(true); //канвас
                    _active[1].transform.parent = _active[0].transform;
                    _active[2].transform.parent = _active[0].transform;
                }

                break;
            }
        }


    }


    private IEnumerator ActiveFunction_0()
    {
        ParticleSystem.EmissionModule emission = _active[0].GetComponent<ParticleSystem>().emission;
        Light light = _active[0].GetComponent<Light>();
        Puzzle[] puzzles = _active[1].GetComponent<PuzzleGame>().Puzzles;
        SpriteRenderer[] sprites = new SpriteRenderer[puzzles.Length - 1];
        for (int k = 0; k < sprites.Length; k++)
        {
            sprites[k] = puzzles[k + 1].GetComponent<SpriteRenderer>();
        }

        for (float rate = 0, intensity = 0, r = 0, g = 0, b = 0;
            rate <= 400;
            rate += 4, intensity += 0.05f, r += 0.0025f, g += 0.0025f, b += 0.0025f)
        {
            emission.rateOverTime = rate;
            light.intensity = intensity;
            for (int k = 0; k < sprites.Length && sprites[0] != null; k++)
            {
                sprites[k].color = new Color(r, g, b);
            }

            yield return new WaitForSeconds(0.1f);
        }

        yield break;
    }
}