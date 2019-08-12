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

    private GameObject _damageOrHeal;

    public void Start()
    {
        _damageTimerInitial = 0.5f;
        _damageTimer = _damageTimerInitial;
    }

    // Update is called once per frame
    public void Update()
    {
        if (AnimState == AnimationState.HEALTHCHANGE)
        {
            _damageTimer -= Time.deltaTime;

            if (_damageTimer <= 0)
            {
                Destroy(_damageOrHeal);
                _damageTimer = _damageTimerInitial;
                AnimState = AnimationState.NONE;
            }
        }
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

