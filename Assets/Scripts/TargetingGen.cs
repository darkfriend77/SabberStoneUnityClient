using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TargetingGen : BasicGen
{
    private TextMeshProUGUI mana;

    public GameObject TargetingPrefab;

    private float _targetingTimerInitial;

    private float _targetingTimer;

    private Vector3 _targetingStart;

    private GameObject _targeting;

    private GameObject _targetGameObject;

    private Vector3 _baseScale;

    private Vector3 _scale;

    private Transform _mainGame;

    public void Start()
    {
        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        var boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");
        _mainGame = boardCanvas.transform.Find("MainGame");

        _targetingTimerInitial = 1.5f;
        _targetingTimer = _targetingTimerInitial;
        _baseScale = new Vector3(0.5f, 0.5f, 0.5f);
        _scale = new Vector3(2f, 2f, 2f);
    }

    public void Update()
    {
        if (AnimState == AnimationState.TARGETING)
        {
            _targetingTimer -= Time.deltaTime;

            _targeting.transform.position = Vector3.Lerp(_targeting.transform.position, _targetGameObject.transform.position, 0.1f);
            _targeting.transform.localScale = Vector3.Lerp(_targeting.transform.localScale, _scale, 0.1f);

            var distance = Vector3.Distance(_targeting.transform.position, _targetGameObject.transform.position);
            Debug.Log(Vector3.Distance(_targeting.transform.position, _targetGameObject.transform.position));

            if (distance < 5)
            {
                _targeting.transform.position = _targetingStart;
                _targeting.transform.localScale = _baseScale;
            }


            if (_targetingTimer <= 0)
            {
                Destroy(_targeting);
                _targetingTimer = _targetingTimerInitial;
                AnimState = AnimationState.NONE;
            }
        }
    }

    internal void TargetingAnim(GameObject targetGameObject)
    {
        _targeting = Instantiate(TargetingPrefab, _mainGame.transform).gameObject;
        _targeting.transform.localScale = _baseScale;
        _targeting.transform.position = gameObject.transform.position;
        _targeting.transform.rotation = Quaternion.FromToRotation(_targeting.transform.position, targetGameObject.transform.position);
        _targetingStart = _targeting.transform.position;
        _targetGameObject = targetGameObject;
        AnimState = AnimationState.TARGETING;
    }

}

