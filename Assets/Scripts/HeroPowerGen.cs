using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroPowerGen : BasicGen
{
    private TextMeshProUGUI mana;

    public GameObject TargetingPrefab;

    private float _targetingTimerInitial;

    private float _targetingTimer;

    private Vector3 _targetingStart;

    private GameObject _targeting;

    private GameObject _targetGameObject;

    private Vector3 _scale;

    public void Start()
    {
        _targetingTimerInitial = 1.5f;
        _targetingTimer = _targetingTimerInitial;
        _scale = new Vector3(4f, 4f, 4f);
    }

    // Update is called once per frame
    public void Update()
    {
        if (AnimState == AnimationState.TARGETING)
        {
            _targetingTimer -= Time.deltaTime;

            _targeting.transform.position = Vector3.Lerp(_targeting.transform.position, _targetGameObject.transform.position, 0.25f);
            _targeting.transform.localScale = Vector3.Lerp(_targeting.transform.localScale, _scale, 0.25f);

            var distance = Vector3.Distance(_targeting.transform.position, _targetGameObject.transform.position);
            Debug.Log(Vector3.Distance(_targeting.transform.position, _targetGameObject.transform.position));

            if (distance < 10)
            {
                _targeting.transform.position = _targetingStart;
                _targeting.transform.localScale = new Vector3(1f, 1f, 1f);
            }


            if (_targetingTimer <= 0)
            {
                Destroy(_targeting);
                _targetingTimer = _targetingTimerInitial;
                AnimState = AnimationState.NONE;
            }
        }
    }

    public override void UpdateEntity(EntityExt entity)
    {
        base.UpdateEntity(entity);

        var front = transform.Find("Front");
        var frame = front.Find("Frame");
        mana = frame.Find("Mana").GetComponent<TextMeshProUGUI>();
        mana.text = entity.Tags[GameTag.COST].ToString();

        var exhausted = front.Find("Exhausted");
        exhausted.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.EXHAUSTED) && entity.Tags[GameTag.EXHAUSTED] == 1);
    }

    internal void Generate(EntityExt entity)
    {
        var front = transform.Find("Front");
        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/weapon_mask");

        var art = artMask.Find("Art");

        if (TexturesUtil.GetArtFromResource(entity.CardId, out Texture2D artTexture))
        {
            art.GetComponent<Image>().sprite = Sprite.Create(artTexture, new Rect(0, 0, artTexture.width, artTexture.height), new Vector2(0, 0));
        }
        else
        {
            StartCoroutine(TexturesUtil.GetTexture(entity.CardId, art));
        }

        UpdateEntity(entity);
    }

    internal void TargetingAnim(GameObject targetGameObject)
    {
        _targeting = Instantiate(TargetingPrefab, gameObject.transform).gameObject;
        _targeting.transform.rotation = Quaternion.FromToRotation(_targeting.transform.position, targetGameObject.transform.position);
        _targetingStart = _targeting.transform.position;
        _targetGameObject = targetGameObject;
        AnimState = AnimationState.TARGETING;
    }
}
