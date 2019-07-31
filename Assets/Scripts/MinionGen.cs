using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionGen : BasicGen
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public override void UpdateEntity(EntityExt entity)
    {
        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        var attack = frame.Find("Attack");
        attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();

        var health = frame.Find("Health");
        health.GetComponent<TextMeshProUGUI>().text = entity.Health.ToString();
        health.GetComponent<TextMeshProUGUI>().color = entity.HealthColor;

        var deathrattle = frame.Find("Deathrattle");
        deathrattle.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.DEATHRATTLE) && entity.Tags[GameTag.DEATHRATTLE] == 1);

        var inspire = frame.Find("Inspire");
        inspire.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.INSPIRE) && entity.Tags[GameTag.INSPIRE] == 1);

        var poisonous = frame.Find("Poisonous");
        poisonous.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.POISONOUS) && entity.Tags[GameTag.POISONOUS] == 1);

        var trigger = frame.Find("Trigger");
        trigger.gameObject.SetActive(false);

        var legendary = front.Find("Legendary");
        legendary.gameObject.SetActive((Rarity)entity.Tags[GameTag.RARITY] == Rarity.LEGENDARY);

        var buffed = front.Find("Buffed");
        buffed.gameObject.SetActive(false);

        var divineShield = front.Find("DivineShield");
        divineShield.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.DIVINE_SHIELD) && entity.Tags[GameTag.DIVINE_SHIELD] == 1);

        var dead = front.Find("Dead");
        dead.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.TO_BE_DESTROYED) && entity.Tags[GameTag.TO_BE_DESTROYED] == 1);
    }

    internal void Generate(EntityExt entity)
    {
        var front = transform.Find("Front");

        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = Resources.Load<Sprite>($"Sprites/minion_mask");

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
