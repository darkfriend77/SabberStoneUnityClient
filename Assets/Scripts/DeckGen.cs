using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Zones;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckGen : MonoBehaviour
{
    public GameObject CardPrefab;

    public List<GameObject> DeckCards = new List<GameObject>();

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Remove(GameObject card)
    {
        DeckCards.Remove(card);
    }

    internal void Add(GameObject card)
    {
        card.transform.SetParent(transform, false);
        card.transform.SetAsFirstSibling();
        DeckCards.Add(card);
    }
}
