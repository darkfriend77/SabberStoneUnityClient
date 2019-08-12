using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AniamteInEditor : MonoBehaviour
{

    public Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        animator.Update(Time.deltaTime);
    }

}
