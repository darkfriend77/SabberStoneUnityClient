using SabberStoneCore.Enums;
using SabberStoneCore.Model;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using System.Linq;
using SabberStoneCore.Model.Entities;
using System;
using SabberStoneCore.Kettle;

public class CardGen : BasicGen
{
    private float _destroyTimer;
    private Color _color;
    private float _colorFade;

    // Start is called before the first frame update
    void Start()
    {
        _destroyTimer = 0.5f;
        _color.a = 1f;
        _colorFade = 1;

        AnimState = AnimationState.NONE;
    }

    internal void Show(bool showFlag)
    {
        transform.Find("Front").gameObject.SetActive(showFlag);
        transform.Find("Back").gameObject.SetActive(!showFlag);
    }

    // Update is called once per frame
    void Update()
    {
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
        CardType cardType = (CardType)entity.Tags[GameTag.CARDTYPE];

        var front = transform.Find("Front");
        var frame = front.Find("Frame");

        var mana = frame.Find("Mana");
        var attack = frame.Find("Attack");
        var health = frame.Find("Health");

        switch (cardType)
        {
            case CardType.INVALID:
                break;
            case CardType.GAME:
                break;
            case CardType.PLAYER:
                break;
            case CardType.HERO:
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.HEALTH].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                break;
            case CardType.MINION:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.HEALTH].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                break;
            case CardType.SPELL:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.gameObject.SetActive(false);
                attack.gameObject.SetActive(false);
                break;
            case CardType.ENCHANTMENT:
                break;
            case CardType.WEAPON:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.DURABILITY].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                break;
            case CardType.ITEM:
                break;
            case CardType.TOKEN:
                break;
            case CardType.HERO_POWER:
                break;
        }
    }

    private Sprite GetLegendarySprite(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.INVALID:
                return null;
            case CardType.GAME:
                return null;
            case CardType.PLAYER:
                return null;
            case CardType.HERO:
                return null;
            case CardType.MINION:
                return Resources.Load<Sprite>($"Sprites/inhand_minion_legendary");
            case CardType.SPELL:
                return Resources.Load<Sprite>($"Sprites/inhand_spell_legendary");
            case CardType.ENCHANTMENT:
                return null;
            case CardType.WEAPON:
                return null;
            case CardType.ITEM:
                return null;
            case CardType.TOKEN:
                return null;
            case CardType.HERO_POWER:
                return null;
            default:
                return null;
        }
    }

    private Sprite GetFrameSprite(CardType cardType, CardClass cardClass)
    {
        switch (cardType)
        {
            case CardType.INVALID:
                return null;
            case CardType.GAME:
                return null;
            case CardType.PLAYER:
                return null;
            case CardType.HERO:
                return null;
            case CardType.MINION:
                return Resources.Load<Sprite>($"Sprites/inhand_minion_{cardClass.ToString().ToLower()}");
            case CardType.SPELL:
                return Resources.Load<Sprite>($"Sprites/inhand_spell_{cardClass.ToString().ToLower()}");
            case CardType.ENCHANTMENT:
                return null;
            case CardType.WEAPON:
                return Resources.Load<Sprite>("Sprites/inhand_weapon_neutral");
            case CardType.ITEM:
                return null;
            case CardType.TOKEN:
                return null;
            case CardType.HERO_POWER:
                return null;
            default:
                return null;
        }
    }

    private Sprite GetArtMaskSpirite(CardType cardType)
    {
        switch (cardType)
        {
            case CardType.INVALID:
                return null;
            case CardType.GAME:
                return null;
            case CardType.PLAYER:
                return null;
            case CardType.HERO:
                return Resources.Load<Sprite>("Sprites/hero_mask");
            case CardType.MINION:
                return Resources.Load<Sprite>($"Sprites/minion_mask");
            case CardType.SPELL:
                return Resources.Load<Sprite>($"Sprites/spell_mask");
            case CardType.ENCHANTMENT:
                return null;
            case CardType.WEAPON:
                return Resources.Load<Sprite>("Sprites/weapon_mask");
            case CardType.ITEM:
                return null;
            case CardType.TOKEN:
                return null;
            case CardType.HERO_POWER:
                return null;
            default:
                return null;
        }
    }

    internal void Generate(EntityExt entity)
    {
        Zone zone = (Zone)entity.Tags[GameTag.ZONE];

        if (zone == Zone.DECK)
        {
            Show(false);
        }
        else
        {
            ShowEntity(entity);
        }
    }

    internal void ShowEntity(EntityExt entity)
    {
        CardType cardType = (CardType)entity.Tags[GameTag.CARDTYPE];
        CardClass cardClass = entity.Tags.ContainsKey(GameTag.CLASS) ? (CardClass)entity.Tags[GameTag.CLASS] : CardClass.NEUTRAL;

        var front = transform.Find("Front");

        var artMask = front.Find("ArtMask");
        artMask.GetComponent<Image>().sprite = GetArtMaskSpirite(cardType);

        var art = artMask.Find("Art");

        if (TexturesUtil.GetArtFromResource(entity.CardId, out Texture2D artTexture))
        {
            art.GetComponent<Image>().sprite = Sprite.Create(artTexture, new Rect(0, 0, artTexture.width, artTexture.height), new Vector2(0, 0));
        }
        else
        {
            StartCoroutine(TexturesUtil.GetTexture(entity.CardId, art));
        }

        var legendary = front.Find("Legendary");
        var frame = front.Find("Frame");
        frame.GetComponent<Image>().sprite = GetFrameSprite(cardType, cardClass);
        legendary.gameObject.SetActive((Rarity)entity.Tags[GameTag.RARITY] == Rarity.LEGENDARY);
        legendary.GetComponent<Image>().sprite = GetLegendarySprite(cardType);

        var name = frame.Find("Name");
        name.GetComponent<TextMeshProUGUI>().text = entity.Name;

        var description = frame.Find("Description");
        description.GetComponent<TextMeshProUGUI>().text = entity.Description;

        var mana = frame.Find("Mana");
        var attack = frame.Find("Attack");
        var health = frame.Find("Health");

        switch (cardType)
        {
            case CardType.INVALID:
                break;
            case CardType.GAME:
                break;
            case CardType.PLAYER:
                break;
            case CardType.HERO:
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.HEALTH].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                break;
            case CardType.MINION:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.HEALTH].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                break;
            case CardType.SPELL:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.gameObject.SetActive(false);
                attack.gameObject.SetActive(false);
                break;
            case CardType.ENCHANTMENT:
                break;
            case CardType.WEAPON:
                mana.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.COST].ToString();
                health.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.DURABILITY].ToString();
                attack.GetComponent<TextMeshProUGUI>().text = entity.Tags[GameTag.ATK].ToString();
                description.GetComponent<TextMeshProUGUI>().color = Color.white;
                break;
            case CardType.ITEM:
                break;
            case CardType.TOKEN:
                break;
            case CardType.HERO_POWER:
                break;
        }

        // set to visible
        Show(true);
    }

    internal void DestroyAnim()
    {
        Debug.Log("DestroyAnim called ...");
        AnimState = AnimationState.DESTROY;
    }
}
