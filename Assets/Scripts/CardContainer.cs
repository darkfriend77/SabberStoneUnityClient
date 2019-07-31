using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using UnityEngine;

public class CardContainer : MonoBehaviour
{
    public List<GameObject> Cards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    internal void Add(GameObject card)
    {
        card.transform.SetParent(transform, false);
        card.transform.SetAsFirstSibling();
        Cards.Add(card);
    }

    internal void Remove(GameObject card)
    {
        Cards.Remove(card);
    }
}
