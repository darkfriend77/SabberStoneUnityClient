using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HeroGen : CharacterGen
{
    private Hero hero;

    private TextMeshProUGUI health;

    private GameObject attackFrame;

    private TextMeshProUGUI attack;

    private GameObject armorFrame;

    private TextMeshProUGUI armor;

    private GameObject frozen;

    private GameObject immune;

    private GameObject quest;

    private GameObject dead;

    private PowerHistoryEntity heroEntity;

    new void Start()
    {
        base.Start();
    }

    new void Update()
    {
        base.Update();
    }

    public override void UpdateEntity(EntityExt entity)
    {
        base.UpdateEntity(entity);

        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        var attackFrameTrans = frame.Find("AttackFrame");
        attackFrame = attackFrameTrans.gameObject;
        attack = attackFrameTrans.Find("Attack").GetComponent<TextMeshProUGUI>();
        attackFrame.SetActive(entity.Tags[GameTag.ATK] > 0);
        attack.text = entity.Tags[GameTag.ATK].ToString();

        var health = frame.Find("Health");
        health.GetComponent<TextMeshProUGUI>().text = entity.Health.ToString();
        health.GetComponent<TextMeshProUGUI>().color = entity.HealthColor;

        var armorFrameTrans = frame.Find("ArmorFrame");
        armorFrame = armorFrameTrans.gameObject;
        armor = armorFrameTrans.Find("Armor").GetComponent<TextMeshProUGUI>();
        if (entity.Tags.ContainsKey(GameTag.ARMOR) && entity.Tags[GameTag.ARMOR] > 0)
        {
            armor.text = entity.Tags[GameTag.ARMOR].ToString();
            armorFrame.SetActive(true);
        }
        else
        {
            armorFrame.SetActive(false);
        }

        frozen = frame.Find("Frozen").gameObject;
        frozen.SetActive(entity.Tags.ContainsKey(GameTag.FROZEN) && entity.Tags[GameTag.FROZEN] > 0);

        immune = frame.Find("Immune").gameObject;
        immune.SetActive(entity.Tags.ContainsKey(GameTag.IMMUNE) && entity.Tags[GameTag.IMMUNE] > 0);

        quest = frame.Find("Quest").gameObject;
        quest.SetActive(false);

        dead = frame.Find("Dead").gameObject;
        dead.SetActive(entity.Tags[GameTag.TO_BE_DESTROYED] == 1);
    }

    public void Generate(EntityExt entity)
    {
        var front = transform.Find("Front");
        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = Resources.Load<Sprite>("Sprites/hero_mask");

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
