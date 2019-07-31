using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using SabberStoneCore.Model.Zones;
using UnityEngine;

public class BoardGen : MonoBehaviour
{
    public GameObject MinionPrefab;

    private Dictionary<int, GameObject> minions = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void UpdateGame()
    {

    }

    internal void GenerateMinion(PowerHistoryEntity entity)
    {
        //var cardEntity = entity.GetComponent<CardGen>().CardEntity;

        //card.GetComponent<CardGen>().GenerateEntity(entity);
        var minion = Instantiate(MinionPrefab, transform).gameObject;
        //card.transform.SetAsFirstSibling();
        //minion.GetComponent<MinionGen>().GenerateEntity(entity);
        minions.Add(entity.Id, minion);
    }

}
