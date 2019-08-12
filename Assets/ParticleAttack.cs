using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleAttack : MonoBehaviour
{

    [SerializeField]
    Animator controller;

    [SerializeField]
    GameObject missilePrefab;

    GameObject missile;
    Animator missileAnimator;

    private float time;

    public float attackInterval = 3f;
    public float speed = 1f;

    void Start() {
        time = attackInterval;
    }

    void Update()
    {
        Spawn();
        missileAnimator = missile.GetComponent<Animator>();
        missileAnimator.speed = speed;
    }

    void Spawn()
    {
        float newTime = (time + Time.deltaTime * speed) % attackInterval;
        if (newTime < time)
        {
            if(missile != null) Destroy(missile);
            missile = Instantiate(missilePrefab);
        }
        time = newTime;
    }

    void OnDrawGizmos()
    {
    }
}
