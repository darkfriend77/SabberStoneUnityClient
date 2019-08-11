using System;
using System.Collections;
using System.Collections.Generic;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model.Entities;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MinionGen : CharacterGen
{
    public override void UpdateEntity(EntityExt entity)
    {
        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        var attack = frame.Find("Attack");
        attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();

        var health = frame.Find("Health");
        health.GetComponent<TextMeshProUGUI>().text = entity.Health.ToString();
        health.GetComponent<TextMeshProUGUI>().color = entity.HealthColor;

        var taunt = front.Find("Taunt");
        taunt.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.TAUNT) && entity.Tags[GameTag.TAUNT] == 1);

        var legendary = front.Find("Legendary");
        legendary.gameObject.SetActive((Rarity)entity.Tags[GameTag.RARITY] == Rarity.LEGENDARY);

        var buffed = front.Find("Buffed");
        buffed.gameObject.SetActive(false);

        var divineShield = front.Find("DivineShield");
        divineShield.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.DIVINE_SHIELD) && entity.Tags[GameTag.DIVINE_SHIELD] == 1);

        var enraged = front.Find("Enraged");
        enraged.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.ENRAGED) && entity.Tags[GameTag.ENRAGED] == 1);

        var frozen = front.Find("Frozen");
        frozen.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.FROZEN) && entity.Tags[GameTag.FROZEN] == 1);

        var immune = front.Find("Immune");
        immune.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.IMMUNE) && entity.Tags[GameTag.IMMUNE] == 1);

        var silenced = front.Find("Silenced");
        silenced.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.SILENCED) && entity.Tags[GameTag.SILENCED] == 1);

        var stealth = front.Find("Stealth");
        stealth.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.STEALTH) && entity.Tags[GameTag.STEALTH] == 1);

        //var deathrattle = frame.Find("Untargetable");

        var dead = front.Find("Dead");
        dead.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.TO_BE_DESTROYED) && entity.Tags[GameTag.TO_BE_DESTROYED] == 1);

        var deathrattle = frame.Find("Deathrattle");
        deathrattle.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.DEATHRATTLE) && entity.Tags[GameTag.DEATHRATTLE] == 1);

        var inspire = frame.Find("Inspire");
        inspire.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.INSPIRE) && entity.Tags[GameTag.INSPIRE] == 1);

        var poisonous = frame.Find("Poisonous");
        poisonous.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.POISONOUS) && entity.Tags[GameTag.POISONOUS] == 1);

        var trigger = frame.Find("Trigger");
        trigger.gameObject.SetActive(false);

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
