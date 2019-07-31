using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroWeaponGen : BasicGen
{

    public override void UpdateEntity(EntityExt entity)
    {
        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        var attack = frame.Find("Attack");
        attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();

        var health = frame.Find("Health");
        health.GetComponent<TextMeshProUGUI>().text = entity.Durability.ToString();
        health.GetComponent<TextMeshProUGUI>().color = entity.DurabilityColor;
    }

    internal void Generate(EntityExt entity)
    {
        var front = transform.Find("Front");

        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/weapon_mask");

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
