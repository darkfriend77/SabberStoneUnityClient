using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class CharacterGen : BasicGen
{
    public GameObject DamagePrefab;

    private float _damageTimerInitial;

    private float _damageTimer;

    private GameObject _damage;

    void Start()
    {
        _damageTimerInitial = 0.5f;
        _damageTimer = _damageTimerInitial;
    }

    // Update is called once per frame
    void Update()
    {
        if (AnimState == AnimationState.DAMAGE)
        {
            _damageTimer -= Time.deltaTime;

            if (_damageTimer <= 0)
            {
                Destroy(_damage);
                _damageTimer = _damageTimerInitial;
                AnimState = AnimationState.NONE;
            }
        }
    }

    internal void DamageAnim(int value)
    {
        _damage = Instantiate(DamagePrefab, gameObject.transform).gameObject;
        var damageValue = _damage.transform.Find("DamageValue").gameObject;
        damageValue.GetComponent<TextMeshProUGUI>().text = value.ToString();
        AnimState = AnimationState.DAMAGE;
    }
}

