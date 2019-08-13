using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackAnimation : MonoBehaviour
{

    public Transform start;
    public Transform target;
    public Transform attackScriptedTransform;
    public GameObject attackAnimation;

    public float speed = 1.0f;
    public Transform camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }
     
    // Update is called once per frame
    void Update()
    {
        UpdateAttackObjectTransform();

        attackAnimation.GetComponent<Animator>().Update(Time.deltaTime * speed);
    }

    private void OnDrawGizmos()
    {
        UpdateAttackObjectTransform();

        attackAnimation.GetComponent<Animator>().Update(Time.deltaTime * speed);

        Gizmos.color = Color.red;
        Gizmos.DrawCube(start.position, new Vector3(1f, 1f, 1f) * .2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawCube(target.position, new Vector3(1f, 1f, 1f) * .2f);
    }

    void UpdateAttackObjectTransform()
    {
        var scale = (start.position - target.position).magnitude;
        attackScriptedTransform.localScale = new Vector3(1f, 1f, 1f) * scale;
        attackScriptedTransform.LookAt(target);
    }

    void CameraShake()
    {
    }
}
