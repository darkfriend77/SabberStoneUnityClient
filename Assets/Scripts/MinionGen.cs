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
    private float _destroyTimer;
    private float _colorFade;

    //// Start is called before the first frame update
    new void Start()
    {
        base.Start();

        _destroyTimer = 0.5f;
        _colorFade = 1;

        AnimState = AnimationState.NONE;
    }

    // Update is called once per frame
    new void Update()
    {
        base.Update();

        if (AnimState == AnimationState.DESTROY)
        {
            _destroyTimer -= Time.deltaTime;

            if (_destroyTimer <= 0)
            {
                _colorFade -= 0.01f;

                foreach (var image in GetComponentsInChildren<Image>())
                {
                    image.color = new Color(image.color.r, image.color.g, image.color.b, _colorFade);
                }

                foreach (var textMesh in GetComponentsInChildren<TextMeshProUGUI>())
                {
                    textMesh.color = new Color(textMesh.color.r, textMesh.color.g, textMesh.color.b, _colorFade);
                }

                if (_colorFade <= 0)
                {
                    Destroy(gameObject);
                    AnimState = AnimationState.NONE;
                }

            }
        }
    }

    public override void UpdateEntity(EntityExt entity)
    {
        base.UpdateEntity(entity);

        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        // TODO add effect for buff and debuff
        var isBuffed = false;
        var isDeBuffed = false;

        var attack = frame.Find("Attack");
        attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
        attack.GetComponent<TextMeshProUGUI>().color = entity.Origin != null && entity.Origin[GameTag.ATK] < entity.Tags[GameTag.ATK] ? Color.green : Color.white;

        var health = frame.Find("Health");
        health.GetComponent<TextMeshProUGUI>().text = entity.Health.ToString();
        health.GetComponent<TextMeshProUGUI>().color = entity.Tags[GameTag.DAMAGE] > 0 ? Color.red : entity.Origin != null && entity.Origin[GameTag.HEALTH] < entity.Tags[GameTag.HEALTH] ? Color.green : Color.white;
        //health.GetComponent<TextMeshProUGUI>().color = entity.HealthColor;

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

        var untargetable = front.Find("Untargetable");
        untargetable.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.UNTOUCHABLE) && entity.Tags[GameTag.UNTOUCHABLE] == 1);

        var exhausted = front.Find("Exhausted");
        exhausted.gameObject.SetActive(entity.Tags.ContainsKey(GameTag.EXHAUSTED) && entity.Tags[GameTag.EXHAUSTED] == 1);

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

    internal void DestroyAnim()
    {
        AnimState = AnimationState.DESTROY;
    }
}
