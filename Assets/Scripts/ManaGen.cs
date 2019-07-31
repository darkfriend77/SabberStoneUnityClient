using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using static ManaCrystalGen;

public class ManaGen : MonoBehaviour
{
    public GameObject ManaCrystalPrefab;

    private Transform manaBar;

    private TextMeshProUGUI manaCurrent;

    private TextMeshProUGUI manaMax;

    private List<GameObject> manaCrystals;

    // Start is called before the first frame update
    void Start()
    {
        manaBar = transform.Find("ManaBar");
        manaCurrent = transform.Find("ManaInfo").Find("ManaCurrent").GetComponent<TextMeshProUGUI>();
        manaMax = transform.Find("ManaInfo").Find("ManaMax").GetComponent<TextMeshProUGUI>();

        manaCrystals = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    internal void UpdateGame(PowerPlayer powerPlayer)
    {
        int resources = powerPlayer.PowerEntity.Tags[GameTag.RESOURCES];
        int resourcesTemp = powerPlayer.PowerEntity.Tags.ContainsKey(GameTag.TEMP_RESOURCES) ? powerPlayer.PowerEntity.Tags[GameTag.TEMP_RESOURCES] : 0;
        int resourcesUsed = powerPlayer.PowerEntity.Tags.ContainsKey(GameTag.RESOURCES_USED) ? powerPlayer.PowerEntity.Tags[GameTag.RESOURCES_USED] : 0;

        manaCurrent.text = (resources + resourcesTemp - resourcesUsed).ToString();
        manaMax.text = (resources + resourcesTemp).ToString();

        //while (manaCrystals.Count != resources + resourcesTemp)
        //{
        //    if (manaCrystals.Count < resources + resourcesTemp)
        //    {
        //        var crystal = Instantiate(ManaCrystalPrefab, manaBar);
        //        manaCrystals.Add(crystal);
        //        crystal.transform.GetComponent<ManaCrystalGen>().SetState(ManaCrystalState.EMPTY);
        //    }
        //    else if (manaCrystals.Count > resources + resourcesTemp)
        //    {
        //        var crystal = manaCrystals[manaCrystals.Count - 1];
        //        manaCrystals.Remove(crystal);
        //        Destroy(crystal);
        //    }

        //}

        //for(int i = 0; i < manaCrystals.Count - 1 && i < resources + resourcesTemp - resourcesUsed; i++)
        //{
        //    manaCrystals[i].transform.GetComponent<ManaCrystalGen>().SetState(ManaCrystalState.FULL);
        //}

    }

    internal void Update(EntityExt entityExt)
    {
        int res = entityExt.Tags[GameTag.RESOURCES];
        int tmp = entityExt.Tags.ContainsKey(GameTag.TEMP_RESOURCES) ? entityExt.Tags[GameTag.TEMP_RESOURCES] : 0;
        int use = entityExt.Tags.ContainsKey(GameTag.RESOURCES_USED) ? entityExt.Tags[GameTag.RESOURCES_USED] : 0;

        manaCurrent.text = (res + tmp - use).ToString();
        manaMax.text = (res + tmp).ToString();
    }
}
