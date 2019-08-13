using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnimationGen : BasicGen
{
    private Transform _mainGame;

    public GameObject DamagePrefab;
    private float _damageTimerInitial;
    private float _damageTimer;
    private GameObject _damageOrHeal;

    private float _destroyTimer;
    private float _colorFade;

    public GameObject TargetingPrefab;
    private float _targetingTimerInitial;
    private float _targetingTimer;
    private Vector3 _targetingStart;
    private GameObject _targeting;
    private GameObject _targetGameObject;
    private Vector3 _baseScale;
    private Vector3 _scale;

    public void Start()
    {
        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        var boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");
        _mainGame = boardCanvas.transform.Find("MainGame");

        _damageTimerInitial = 0.5f;
        _damageTimer = _damageTimerInitial;

        _destroyTimer = 0.5f;
        _colorFade = 1;

        _targetingTimerInitial = 1.5f;
        _targetingTimer = _targetingTimerInitial;
        _baseScale = new Vector3(0.5f, 0.5f, 0.5f);
        _scale = new Vector3(2f, 2f, 2f);
    }

    // Update is called once per frame
    public void Update()
    {
        switch(AnimState)
        {
            case AnimationState.TARGETING:
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
                break;

            case AnimationState.HEALTHCHANGE:
                _damageTimer -= Time.deltaTime;

                if (_damageTimer <= 0)
                {
                    Destroy(_damageOrHeal);
                    _damageTimer = _damageTimerInitial;
                    AnimState = AnimationState.NONE;
                }
                break;

            case AnimationState.DESTROY:
                _destroyTimer -= Time.deltaTime;

                if (_destroyTimer <= 0)
                {
                    _colorFade -= 0.01f;

                    foreach (var image in GetComponentsInChildren<Image>())
                    {
                        image.color = new Color(image.color.r, image.color.g, image.color.b, _colorFade);
                    }

                    foreach (var textMesh in GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, _colorFade);
                    }

                    if (_colorFade <= 0)
                    {
                        Destroy(gameObject);
                        AnimState = AnimationState.NONE;
                    }

                }
                break;
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

    internal void DestroyAnim()
    {
        AnimState = AnimationState.DESTROY;
    }

    internal void DamageOrHealAnim(int value)
    {
        _damageOrHeal = Instantiate(DamagePrefab, gameObject.transform).gameObject;
        var healthChangeValue = _damageOrHeal.transform.Find("HealthChangeValue").gameObject;
        healthChangeValue.GetComponent<TextMeshProUGUI>().text = value.ToString();
        var damage = _damageOrHeal.transform.Find("Damage").gameObject;
        var heal = _damageOrHeal.transform.Find("Heal").gameObject;
        if (value < 0)
        {
            damage.SetActive(true);
            heal.SetActive(false);
            healthChangeValue.GetComponent<TextMeshProUGUI>().color = Color.red;
        }
        else
        {
            damage.SetActive(false);
            heal.SetActive(true);
            healthChangeValue.GetComponent<TextMeshProUGUI>().color = Color.green;
        }
        AnimState = AnimationState.HEALTHCHANGE;
    }

    //IEnumerator StartAnimDead()
    //{
    //    minionGen.AnimState = AnimationState.DEAD;
    //    counter = 0;
    //    yield return new WaitUntil(() => minionGen.AnimState == AnimationState.DONE);

    //    minionGen.AnimState = AnimationState.NONE;
    //}

}

