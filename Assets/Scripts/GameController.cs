﻿using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    private Game game;

    private Dictionary<int, EntityExt> EntitiesExt = new Dictionary<int, EntityExt>();

    public GameObject HeroPrefab;

    public GameObject HeroPowerPrefab;

    public GameObject HeroWeaponPrefab;

    public GameObject CardPrefab;

    public GameObject MinionPrefab;

    public GameObject ManaPrefab;

    //public GameObject ConsoleEntryPrefab;

    private EntityExt attackingEntity;

    private EntityExt defendingEntity;

    private Transform gridParent, _mainGame, _myChoices, _myChoicesPanel;

    //private ScrollRect consoleScrollRect;

    public LogLevel logLevel = LogLevel.INFO;

    public bool AllAnimStatesAreNone => EntitiesExt.Values.ToList().TrueForAll(p => p.GameObjectScript == null || p.GameObjectScript.AnimState == AnimationState.NONE);

    private int _playerId;

    public int PlayerId => _playerId;

    private Button _endTurnButton;

    private Game _game;

    private bool DoUpdatedOptions;

    private int _updatingIndex;

    private System.Random _random;

    private int _stepper;

    private Queue<IPowerHistoryEntry> HistoryEntries { get; set; }

    //public PowerChoices PowerChoices { get; private set; }

    public List<PowerOption> PowerOptionList { get; private set; }

    public Text PowerHistoryText, PowerOptionsText, PowerChoicesText, PlayerStateText;

    public enum PlayerClientState
    {
        None,
        Choice,
        Option,
        Wait,
        Finish
    }

    private PlayerClientState _playerState;

    public PlayerClientState PlayerState
    {
        get => _playerState;
        private set
        {
            _playerState = value;
            PlayerStateText.text = _playerState.ToString();
        }
    }

    private void OnEnable()
    {
        Debug.Log("GameController enabled, event handled!");
    }

    private void OnDisable()
    {
        Debug.Log("GameController disabled, event not handled!");
    }

    public void Start()
    {
        _random = new System.Random();
        PlayerState = PlayerClientState.None;

        //consoleScrollRect = transform.parent.Find("Console").GetComponent<ScrollRect>();
        //gridParent = transform.parent.Find("Console").Find("Viewport").Find("Grid");
        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        var boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");
        _mainGame = boardCanvas.transform.Find("MainGame");
        _myChoices = _mainGame.transform.Find("MyChoices");
        _myChoicesPanel = _myChoices.transform.Find("MyChoicesPanel");
        _myChoices.gameObject.SetActive(false);
        _endTurnButton = _mainGame.transform.Find("EndTurnButton").GetComponent<Button>();
        _endTurnButton.interactable = false;

        _updatingIndex = 0;

        _stepper = 0;

        HistoryEntries = new Queue<IPowerHistoryEntry>();

        CreateReferenceGame();
    }

    private void CreateReferenceGame()
    {

        Debug.Log($"Game creation is happening in a few seconds!!!");

        var gameConfig = new GameConfig
        {
            //StartPlayer = 1,
            FormatType = FormatType.FT_STANDARD,
            Player1HeroClass = CardClass.MAGE,
            Player1Deck = new List<Card>() {
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Fireball"),
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Fireball"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Boulderfist Ogre"),
                Cards.FromName("Boulderfist Ogre")
            },
            Player2HeroClass = CardClass.MAGE,
            Player2Deck = new List<Card>() {
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Fireball"),
                Cards.FromName("Arcane Missiles"),
                Cards.FromName("Frostbolt"),
                Cards.FromName("Fireball"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Polymorph"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Water Elemental"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Flamestrike"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Arcane Intellect"),
                Cards.FromName("Goldshire Footman"),
                Cards.FromName("Frostwolf Grunt"),
                Cards.FromName("Kobold Geomancer"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Dalaran Mage"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Sen'jin Shieldmasta"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Darkscale Healer"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Gurubashi Berserker"),
                Cards.FromName("Boulderfist Ogre"),
                Cards.FromName("Boulderfist Ogre")
            },
            SkipMulligan = true,
            Shuffle = false,
            FillDecks = false,
            Logging = true,
            History = true
        };
        var newGame = new Game(gameConfig);

        // don't start when game is null
        if (_game != null)
        {
            return;
        }

        _game = newGame;

        Debug.Log($"Game creation done!");

        _playerId = 1;

        Debug.Log($"Watched playeyrId = {_playerId}!");

    }

    public void OnClickStepByStep()
    {
        switch (_stepper)
        {
            case 0:
                _game.StartGame();
                break;
        }

        _game.PowerHistory.Last.ForEach(p => HistoryEntries.Enqueue(p));
    }

    public void Update()
    {
        _updatingIndex++;
        if (_updatingIndex > 1)
        {
            ReadHistory();
            _updatingIndex = 0;
        }
    }

    public void OnClickEndTurn()
    {

    }

    //public void ReadHistory(List<IPowerHistoryEntry> powerHistoryEntries)
    public void ReadHistory()
    {
        IPowerHistoryEntry historyEntry;
        if (AllAnimStatesAreNone)
        {
            if (HistoryEntries.Count > 0)
            {
                PlayerState = PlayerClientState.Wait;
                historyEntry = HistoryEntries.Dequeue();
                ReadHistoryEntry(historyEntry);
            }

            if (HistoryEntries.Count == 0 && PlayerState == PlayerClientState.Wait)
            {
                if (ReadPowerChoices())
                {
                    PlayerState = PlayerClientState.Choice;
                }
                else if (ReadPowerOptions())
                {
                    PlayerState = PlayerClientState.Option;
                }
                else
                {
                    PlayerState = PlayerClientState.Wait;
                    PowerChoicesText.text = "0";
                    PowerOptionsText.text = "0";
                }
            }
        }
    }

    public bool ReadPowerChoices()
    {
        //if (PowerChoices == null)
        //{
        //    _myChoices.gameObject.SetActive(false);
        //    return false;
        //}

        //Debug.Log($"Current PowerChoices: {PowerChoices.ChoiceType} with {PowerChoices.Entities.Count} entities");

        //PowerChoicesText.text = $"{PowerChoices.Entities.Count}{PowerChoices.ChoiceType.ToString().Substring(0, 1)}";

        //_myChoices.gameObject.SetActive(true);

        //PowerChoices.Entities.ForEach(p => {

        //    if (!EntitiesExt.TryGetValue(p, out EntityExt entityExt))
        //    {
        //        throw new Exception($"Can't find entity with the id {p} in our dictionary!");
        //    }

        //    createCardIn(_myChoicesPanel, CardPrefab, entityExt);
        //});

        return true;
    }

    //public void ReadPowerOptions(List<PowerOption> powerOptions)
    public bool ReadPowerOptions()
    {
        if (PowerOptionList.Count() == 0)
        {
            _endTurnButton.interactable = false;
            return false;
        }

        //Debug.Log($"Current PowerOptions: {PowerOptionList.Count()}");

        PowerOptionsText.text = PowerOptionList.Count().ToString();

        var endTurnOption = PowerOptionList.Find(p => p.OptionType == OptionType.END_TURN);
        _endTurnButton.interactable = endTurnOption != null;

        // display the other power options ...
        foreach (var powerOption in PowerOptionList)
        {

        }

        return true;
    }

    public void OnClickRandomMove()
    {

    }

    private void ReadHistoryEntry(IPowerHistoryEntry historyEntry)
    {
        //Debug.Log(historyEntry.Print());

        switch (historyEntry.PowerType)
        {
            case PowerType.CREATE_GAME:
                UpdateCreateGame(historyEntry as PowerHistoryCreateGame);
                break;

            case PowerType.FULL_ENTITY:
                UpdateFullEntity(historyEntry as PowerHistoryFullEntity);
                break;

            case PowerType.SHOW_ENTITY:
                UpdateShowEntity(historyEntry as PowerHistoryShowEntity);
                break;

            case PowerType.HIDE_ENTITY:
                UpdateHideEntity(historyEntry as PowerHistoryHideEntity);
                break;

            case PowerType.TAG_CHANGE:
                UpdateTagChange(historyEntry as PowerHistoryTagChange);
                break;

            case PowerType.BLOCK_START:
                UpdateBlockStart(historyEntry as PowerHistoryBlockStart);
                break;

            case PowerType.BLOCK_END:
                UpdateBlockEnd(historyEntry as PowerHistoryBlockEnd);
                break;

            case PowerType.META_DATA:
                UpdateMetaData(historyEntry as PowerHistoryMetaData);
                break;

            case PowerType.RESET_GAME:
            case PowerType.CHANGE_ENTITY:
            default:
                throw new Exception();
        }
    }

    private void UpdateCreateGame(PowerHistoryCreateGame createGame)
    {

        EntitiesExt.Add(createGame.Game.Id, new EntityExt()
        {
            Id = createGame.Game.Id,
            CardId = "GAME",
            Description = "Sabberstone Game",
            Tags = createGame.Game.Tags
        });

        EntitiesExt.Add(createGame.Players[0].PowerEntity.Id, new EntityExt()
        {
            Id = createGame.Players[0].PowerEntity.Id,
            CardId = "PLAYER1",
            Description = "Sabberstone Player 1",
            Tags = createGame.Players[0].PowerEntity.Tags
        });

        EntitiesExt.Add(createGame.Players[1].PowerEntity.Id, new EntityExt()
        {
            Id = createGame.Players[1].PowerEntity.Id,
            CardId = "PLAYER2",
            Description = "Sabberstone Player 2",
            Tags = createGame.Players[1].PowerEntity.Tags
        });
    }

    //private void UpdateChangeEntity(PowerHistoryEntity entity)
    //{
    //    if (EntitiesExt.TryGetValue(entity.Id, out EntityExt value))
    //    {
    //        UpdateTags(value, entity.Name, entity.Tags);
    //    }
    //    else
    //    {
    //        Debug.Log($"Can't find entity with the id {entity.Id} in our dictionary!");
    //    }
    //}

    private void UpdateMetaData(PowerHistoryMetaData metaData)
    {
        Debug.Log($"{metaData.Print()}");
    }

    private void UpdateBlockEnd(PowerHistoryBlockEnd blockEnd)
    {
    }

    private void UpdateBlockStart(PowerHistoryBlockStart blockStart)
    {
        //Debug.Log(blockStart.BlockType);
    }

    private void UpdateTagChange(PowerHistoryTagChange tagChange)
    {

        if (!EntitiesExt.TryGetValue(tagChange.EntityId, out EntityExt entityExt))
        {
            throw new Exception($"Can't find entity with the id {tagChange.EntityId} in our dictionary!");
        }

        int? preValue = entityExt.Tags.ContainsKey(tagChange.Tag) ? entityExt.Tags[tagChange.Tag] : (int?)null;

        if (entityExt.Tags.TryGetValue(tagChange.Tag, out int oldValue))
        {
            if (oldValue != tagChange.Value)
            {
                //Debug.Log($"[CHANGE_TAG] {tagChange.Tag}: {oldValue} => {tagChange.Value}");
                entityExt.Tags[tagChange.Tag] = tagChange.Value;
            }
        }
        else
        {
            //Debug.Log($"[ADD_TAG] {tagChange.Tag}: {tagChange.Value}");
            entityExt.Tags[tagChange.Tag] = tagChange.Value;
        }

        switch (tagChange.Tag)
        {
            case GameTag.MULLIGAN_STATE:
                Debug.Log($"MULLIGAN_STATE = {(Mulligan)tagChange.Value}");
                break;

            case GameTag.PLAYSTATE:
                Debug.Log($"PLAYSTATE = {(PlayState)tagChange.Value}");
                break;

            //case GameTag.HERO_ENTITY:
            //    var heroParent = transform.Find(GetParentObject("Hero", entityExt));
            //    var hero = Instantiate(HeroPrefab, heroParent).gameObject.GetComponent<HeroGen>();
            //    hero.Generate(EntitiesExt[tagChange.Value]);
            //    EntitiesExt[tagChange.Value].GameObjectScript = hero;
            //    break;

            case GameTag.ZONE:
                DoZoneChange(entityExt, preValue != null ? (Zone)preValue : Zone.INVALID, (Zone)tagChange.Value);
                break;

            case GameTag.ZONE_POSITION:
                //Debug.Log("UPDATE_ENTITY_POSITION");
                break;

            case GameTag.RESOURCES:
            case GameTag.RESOURCES_USED:
            case GameTag.TEMP_RESOURCES:
                //Debug.Log("UPDATE_RESOURCES");
                _mainGame.transform.Find(GetParentObject("Mana", entityExt)).transform.Find("Mana").GetComponent<ManaGen>().Update(entityExt);
                break;

            case GameTag.CURRENT_PLAYER:
                // Debug.Log(tagChange.Print());
                break;

            case GameTag.TO_BE_DESTROYED:
                entityExt.GameObjectScript.UpdateEntity(entityExt);
                if (entityExt.Zone == Zone.PLAY && entityExt.CardType == CardType.MINION)
                {
                    entityExt.GameObjectScript.transform.GetComponent<MinionAnimation>().AnimDead();
                }
                break;

            case GameTag.ATTACKING:
                attackingEntity = tagChange.Value == 1 ? entityExt : null;
                break;
            case GameTag.DEFENDING:
                defendingEntity = tagChange.Value == 1 ? entityExt : null;
                break;

            case GameTag.DAMAGE:
                Debug.Log($"Doing damage ... to {entityExt.Name}");
                if (entityExt == defendingEntity && attackingEntity.Zone == Zone.PLAY && attackingEntity.CardType == CardType.MINION)
                {
                    Debug.Log("with animation now !!!");
                    attackingEntity.GameObjectScript.transform.GetComponent<MinionAnimation>().AnimAttack(defendingEntity.GameObjectScript.gameObject);
                }
                entityExt.GameObjectScript.UpdateEntity(entityExt);
                break;

            // updateing entities ... with those tags, visible stats
            case GameTag.ATK:
            case GameTag.HEALTH:
            case GameTag.ENRAGED:
            case GameTag.FROZEN:
            case GameTag.IMMUNE:
            case GameTag.SILENCED:
            case GameTag.STEALTH:
            case GameTag.ARMOR:
            case GameTag.DURABILITY:
            case GameTag.DIVINE_SHIELD:
            case GameTag.DEATHRATTLE:
            case GameTag.POISONOUS:
            case GameTag.INSPIRE:
            case GameTag.AURA:
            case GameTag.COST:
                entityExt.GameObjectScript.UpdateEntity(entityExt);
                break;

            // ignoring this for now
            case GameTag.FIRST_PLAYER:
            case GameTag.TIMEOUT:
            case GameTag.NUM_TURNS_LEFT:
            case GameTag.NUM_TURNS_IN_PLAY:
            case GameTag.NUM_CARDS_DRAWN_THIS_TURN:
            case GameTag.HERO_ENTITY:
                break;

            default:
                Debug.Log(tagChange.Print());
                break;
        }

    }

    private void DoZoneChange(EntityExt entityExt, Zone prevZone, Zone nextZone)
    {
        if (entityExt.CardType == CardType.HERO && nextZone == Zone.PLAY)
        {
            var heroParent = _mainGame.transform.Find(GetParentObject("Hero", entityExt));
            var hero = Instantiate(HeroPrefab, heroParent).gameObject.GetComponent<HeroGen>();
            hero.Generate(entityExt);
            entityExt.GameObjectScript = hero;
            return;
        }

        //only interpret minion, spell & weapons, no hero & hero power
        if (entityExt.Tags.ContainsKey(GameTag.CARDTYPE)
            && entityExt.CardType != CardType.MINION
            && entityExt.CardType != CardType.SPELL
            && entityExt.CardType != CardType.WEAPON)
        {
            throw new Exception($"No zone changes currently implemented for {entityExt.CardType} from {prevZone} to {nextZone}!");
        }

        switch (prevZone)
        {
            case Zone.INVALID:

                GameObject gameObject;
                //CardGen cardGen;
                MinionGen minionGen;
                HeroWeaponGen heroWeaponGen;

                switch (nextZone)
                {
                    case Zone.DECK:
                        entityExt.GameObjectScript = createCardIn("Deck", CardPrefab, entityExt);
                        break;

                    case Zone.HAND:
                        entityExt.GameObjectScript = createCardIn("Hand", CardPrefab, entityExt);
                        break;

                    case Zone.PLAY:
                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                gameObject = Instantiate(MinionPrefab, _mainGame.transform).gameObject;
                                minionGen = gameObject.GetComponent<MinionGen>();
                                minionGen.Generate(entityExt);
                                entityExt.GameObjectScript = minionGen;

                                _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Add(gameObject);
                                break;

                            case CardType.WEAPON:
                                var heroWeaponParent = _mainGame.transform.Find(GetParentObject("HeroWeapon", entityExt));
                                gameObject = Instantiate(HeroWeaponPrefab, heroWeaponParent.transform).gameObject;
                                heroWeaponGen = gameObject.GetComponent<HeroWeaponGen>();
                                heroWeaponGen.Generate(entityExt);
                                entityExt.GameObjectScript = heroWeaponGen;
                                break;

                            default:
                                Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                                break;
                        }
                        break;

                    case Zone.SETASIDE:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;

                    default:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;
                }
                break;

            case Zone.DECK:
                _mainGame.transform.Find(GetParentObject("Deck", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                switch (nextZone)
                {
                    case Zone.HAND:
                        _mainGame.transform.Find(GetParentObject("Hand", entityExt)).GetComponent<CardContainer>().Add(entityExt.GameObjectScript.gameObject);
                        break;

                    default:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;
                }
                break;

            case Zone.HAND:
                _mainGame.transform.Find(GetParentObject("Hand", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                switch (nextZone)
                {
                    case Zone.DECK:
                        // mostly from mulligan back to deck
                        _mainGame.transform.Find(GetParentObject("Hand", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                        _mainGame.transform.Find(GetParentObject("Deck", entityExt)).GetComponent<CardContainer>().Add(entityExt.GameObjectScript.gameObject);
                        break;

                    case Zone.PLAY:

                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                // destroying old object, as we are building a new one.
                                Destroy(entityExt.GameObjectScript.gameObject);

                                gameObject = Instantiate(MinionPrefab, _mainGame.transform).gameObject;
                                minionGen = gameObject.GetComponent<MinionGen>();
                                minionGen.Generate(entityExt);
                                entityExt.GameObjectScript = minionGen;

                                _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Add(gameObject);
                                break;

                            case CardType.SPELL:
                                // destroying old object, as we are building a new one.
                                Destroy(entityExt.GameObjectScript.gameObject);
                                break;

                            case CardType.WEAPON:
                                Destroy(entityExt.GameObjectScript.gameObject);

                                var heroWeaponParent = _mainGame.transform.Find(GetParentObject("HeroWeapon", entityExt));
                                gameObject = Instantiate(HeroWeaponPrefab, heroWeaponParent.transform).gameObject;
                                heroWeaponGen = gameObject.GetComponent<HeroWeaponGen>();
                                heroWeaponGen.Generate(entityExt);
                                entityExt.GameObjectScript = heroWeaponGen;
                                break;

                            default:
                                Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                                break;
                        }
                        break;

                    default:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;
                }
                break;

            case Zone.PLAY:

                switch (nextZone)
                {
                    case Zone.GRAVEYARD:
                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                                Destroy(entityExt.GameObjectScript.gameObject);
                                break;

                            case CardType.WEAPON:
                                Destroy(entityExt.GameObjectScript.gameObject);
                                break;

                            default:
                                Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                                break;
                        }
                        break;
                    default:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;
                }
                break;

            case Zone.GRAVEYARD:
            case Zone.REMOVEDFROMGAME:
            case Zone.SETASIDE:
            case Zone.SECRET:
            default:
                Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                break;
        }

    }

    private BasicGen createCardIn(Transform location, GameObject cardPrefab, EntityExt entityExt)
    {
        var gameObject = Instantiate(CardPrefab, _mainGame.transform).gameObject;
        var cardGen = gameObject.GetComponent<CardGen>();
        cardGen.Generate(entityExt);
        entityExt.GameObjectScript = cardGen;
        location.GetComponent<CardContainer>().Add(gameObject);
        return cardGen;
    }

    private BasicGen createCardIn(string location, GameObject cardPrefab, EntityExt entityExt)
    {
        var gameObject = Instantiate(CardPrefab, _mainGame.transform).gameObject;
        var cardGen = gameObject.GetComponent<CardGen>();
        cardGen.Generate(entityExt);
        entityExt.GameObjectScript = cardGen;
        _mainGame.transform.Find(GetParentObject(location, entityExt)).GetComponent<CardContainer>().Add(gameObject);
        return cardGen;
    }

    private string GetParentObject(string parentObjectName, EntityExt entityExt)
    {
        return GetParentObject(parentObjectName, entityExt.Tags[GameTag.CONTROLLER]);
    }

    private string GetParentObject(string parentObjectName, int playerId)
    {
        if (PlayerId == playerId)
        {
            return $"My{parentObjectName}";
        }
        else
        {
            return $"Op{parentObjectName}";
        }
    }

    private void UpdateHideEntity(PowerHistoryHideEntity entity)
    {
        // Debug.Log(showEntity.Print());
        if (!EntitiesExt.TryGetValue(entity.EntityID, out EntityExt value))
        {
            throw new Exception($"Can't find entity with the id {entity.EntityID} in our dictionary!");
        }

    }

    private void UpdateShowEntity(PowerHistoryShowEntity showEntity)
    {
        //Debug.Log(showEntity.Print());
        if (!EntitiesExt.TryGetValue(showEntity.Entity.Id, out EntityExt entity))
        {
            throw new Exception($"Can't find entity with the id {showEntity.Entity.Id} in our dictionary!");
        }

        // checking for same same
        UpdateTags(entity, showEntity.Entity.Name, showEntity.Entity.Tags);

        var card = GetCardFromName(showEntity.Entity);
        if (card != null)
        {
            entity.Name = card.Name;
            entity.Description = card.Text;
        }

        if (entity.GameObjectScript is CardGen cardGen)
        {
            cardGen.ShowEntity(entity);
        }

    }

    private void UpdateTags(EntityExt value, string name, Dictionary<GameTag, int> tags)
    {
        var oldTags = value.Tags;
        if (value.CardId != name)
        {
            //Debug.Log($"{value.CardId} => {name}");
            value.CardId = name;
        }

        foreach (var keyValue in tags)
        {
            if (oldTags.TryGetValue(keyValue.Key, out int oldValue))
            {
                if (oldValue != keyValue.Value)
                {
                    //Debug.Log($"[CHANGE_TAG] {keyValue.Key}: {oldValue} => {keyValue.Value}");
                    oldTags[keyValue.Key] = keyValue.Value;
                }
            }
            else
            {
                //Debug.Log($"[ADD_TAG] {keyValue.Key}: {keyValue.Value}");
                oldTags[keyValue.Key] = keyValue.Value;
            }
        }

    }

    private void UpdateFullEntity(PowerHistoryFullEntity fullEntity)
    {
        string name = "missing";
        string description = "missing";

        var card = GetCardFromName(fullEntity.Entity);
        if (card != null)
        {
            name = card.Name;
            description = card.Text;
        }

        var entityExt = new EntityExt()
        {
            Id = fullEntity.Entity.Id,
            CardId = fullEntity.Entity.Name,
            Name = name,
            Description = description,
            Tags = fullEntity.Entity.Tags
        };
        EntitiesExt.Add(fullEntity.Entity.Id, entityExt);

        CardType cardType = entityExt.Tags.ContainsKey(GameTag.CARDTYPE) ? (CardType)entityExt.Tags[GameTag.CARDTYPE] : CardType.INVALID;

        Zone zone = entityExt.Tags.ContainsKey(GameTag.ZONE) ? (Zone)entityExt.Tags[GameTag.ZONE] : Zone.INVALID;

        switch (zone)
        {
            case Zone.INVALID:
                break;

            case Zone.PLAY:
                if (cardType == CardType.HERO_POWER)
                {
                    var heroPowerParent = _mainGame.transform.Find(GetParentObject("HeroPower", entityExt));
                    var heroPower = Instantiate(HeroPowerPrefab, heroPowerParent).gameObject.GetComponent<HeroPowerGen>();
                    heroPower.Generate(entityExt);
                    entityExt.GameObjectScript = heroPower;
                }
                break;

            case Zone.DECK:
                entityExt.GameObjectScript = createCardIn("Deck", CardPrefab, entityExt);
                break;

            case Zone.HAND:
                entityExt.GameObjectScript = createCardIn("Hand", CardPrefab, entityExt);
                break;
            case Zone.GRAVEYARD:
                break;
            case Zone.REMOVEDFROMGAME:
                break;
            case Zone.SETASIDE:
                break;
            case Zone.SECRET:
                break;
        }
    }

    private Card GetCardFromName(PowerHistoryEntity entity)
    {
        string cardId = entity.Name;

        if (entity.Name == "")
        {
            Debug.Log($"We got an unknown entity with id {entity.Id}.");
            return null;
        }

        if (cardId != null && cardId != string.Empty)
        {
            Card card = Cards.FromId(cardId);
            if (card != null)
            {
                return card;
            }
        }

        Debug.LogError($"Can't find this card id '{cardId}'!");
        return null;
    }
}

