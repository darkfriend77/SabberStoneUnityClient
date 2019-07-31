using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinionAnimation : MonoBehaviour
{
    private BasicGen minionGen;

    private Vector3 cachedScale;
    private Vector3 cachedPosition;
    private Quaternion cachedRotation;
    private Transform cachedParent;
    private int cachedSiblingIndex;

    private GameObject target;

    int counter;

    // Start is called before the first frame update
    void Start()
    {
        minionGen = transform.GetComponent<MinionGen>();
        minionGen.AnimState = AnimationState.NONE;
    }

    // Update is called once per frame
    void Update()
    {
        switch (minionGen.AnimState)
        {
            case AnimationState.ATTACK:
                transform.position = Vector3.Lerp(transform.position, target.transform.position, 0.05f);

                counter++;

                if (counter > 20)
                {
                    minionGen.AnimState = AnimationState.DONE;
                }
                break;
            case AnimationState.DEAD:
                counter++;

                if (counter > 20)
                {
                    minionGen.AnimState = AnimationState.DONE;
                }
                break;
            case AnimationState.NONE:
            case AnimationState.DONE:
                break;
        }
    }

    public void AnimAttack(GameObject attackTarget)
    {
        cachedScale = transform.localScale;
        cachedPosition = transform.position;
        cachedRotation = transform.rotation;
        cachedParent = transform.parent;
        cachedSiblingIndex = transform.GetSiblingIndex();

        target = attackTarget;

        StartCoroutine(StartAnimAttack());
    }

    IEnumerator StartAnimAttack()
    {
        transform.SetParent(transform.parent.parent, true);
        minionGen.AnimState = AnimationState.ATTACK;
        counter = 0;
        yield return new WaitUntil(() => minionGen.AnimState == AnimationState.DONE);

        target = null;
        transform.localScale = cachedScale;
        transform.SetPositionAndRotation(cachedPosition, Quaternion.identity);
        transform.SetParent(cachedParent, false);
        transform.SetSiblingIndex(cachedSiblingIndex);

        minionGen.AnimState = AnimationState.NONE;
    }

    public void AnimDead()
    {
        StartCoroutine(StartAnimDead());
    }

    IEnumerator StartAnimDead()
    {
        minionGen.AnimState = AnimationState.DEAD;
        counter = 0;
        yield return new WaitUntil(() => minionGen.AnimState == AnimationState.DONE);

        minionGen.AnimState = AnimationState.NONE;
    }
}
