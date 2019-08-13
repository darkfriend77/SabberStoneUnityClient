using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroPowerGen : AnimationGen
{
    private TextMeshProUGUI mana;

    public override void UpdateEntity(EntityExt entity)
    {
        base.UpdateEntity(entity);

        var front = transform.Find("Front");
        var frame = front.Find("Frame");
        mana = frame.Find("Mana").GetComponent<TextMeshProUGUI>();
        mana.text = entity.Tags[GameTag.COST].ToString();

        var exhausted = front.Find("Exhausted");
        exhausted.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.EXHAUSTED) && entity.Tags[GameTag.EXHAUSTED] == 1);
    }

    internal void Generate(EntityExt entity)
    {
        var front = transform.Find("Front");
        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/weapon_mask");

        var art = artMask.Find("Art");

        if (TexturesUtil.GetArtFromResource(entity.CardId, out Texture2D artTexture))
        {
            art.GetComponent<Image>().sprite = Sprite.Create(artTexture, new Rect(0, 0, artTexture.width, artTexture.height), new Vector2(0, 0));
        }
        else
        {
            StartCoroutine(TexturesUtil.GetTexture(entity.CardId, art));
        }

        UpdateEntity(entity);
    }
}
