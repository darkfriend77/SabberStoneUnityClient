using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ManaCrystalGen : MonoBehaviour
{
    public enum ManaCrystalState
    {
        FULL, LOCKED, EMPTY
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetState(ManaCrystalState state)
    {
        transform.Find("Full").gameObject.SetActive(state == ManaCrystalState.FULL);
        transform.Find("Empty").gameObject.SetActive(state == ManaCrystalState.EMPTY);
        transform.Find("Locked").gameObject.SetActive(state == ManaCrystalState.LOCKED);
    }
}
