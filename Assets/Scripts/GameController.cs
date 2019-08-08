using Newtonsoft.Json;
using SabberStoneContract.Model;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model;
using System;
using System.Collections.Generic;
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

    private GameClient _gameClient;

    private Button _endTurnButton;

    private bool DoUpdatedOptions;

    private int _updatingIndex;

    private System.Random _random;

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

    public PlayerClientState PlayerState { 
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

        //InvokeRepeating("ReadHistory", 0f, 0.1f);
        //InvokeRepeating("ReadPowerOptions", 0f, 2f);
    }

    public void Update()
    {
        _updatingIndex++;
        if (_updatingIndex > 3)
        {
            ReadHistory();
            _updatingIndex = 0;
        }
    }

    public void OnClickEndTurn()
    {
        var endTurnOption = _gameClient.PowerOptionList.Find(p => p.OptionType == OptionType.END_TURN);
        if (endTurnOption != null)
        {
            _endTurnButton.interactable = false;
            PlayerState = PlayerClientState.Wait;

            _gameClient.SendPowerOptionChoice(
                new PowerOptionChoice()
                {
                    PowerOption = endTurnOption,
                    Target = 0,
                    Position = 0,
                    SubOption = 0
                });
        }
        else
        {
            throw new Exception("No endturn option available, something bad happened!");
        }
    }

    public void SetGameClient(GameClient gameClient)
    {
        _gameClient = gameClient;
    }

    //public void ReadHistory(List<IPowerHistoryEntry> powerHistoryEntries)
    public void ReadHistory()
    {
        if (_gameClient == null || _gameClient.GameClientState != GameClientState.InGame)
        {
            return;
        }

        var myStats = _mainGame.transform.Find(GetParentObject("Stats", _gameClient.MyUserInfo.PlayerId));
        myStats.transform.Find("AccountName").GetComponent<Text>().text = $"{_gameClient.MyUserInfo.AccountName}[{_gameClient.MyUserInfo.PlayerId}]";

        var opStats = _mainGame.transform.Find(GetParentObject("Stats", _gameClient.OpUserInfo.PlayerId));
        opStats.transform.Find("AccountName").GetComponent<Text>().text = $"{_gameClient.OpUserInfo.AccountName}[{_gameClient.OpUserInfo.PlayerId}]";

        //    await Task.Run(() => {

        //        while (!AllAnimStatesAreNone)
        //        {
        //            Debug.Log("Waiting for animations to stop!");
        //            Thread.Sleep(1000);
        //        }

        //        while (!_gameClient.HistoryEntries.IsEmpty)
        //        {
        //            while (_gameClient.HistoryEntries.TryDequeue(out IPowerHistoryEntry historyEntry))
        //            {
        //                ReadHistoryEntry(historyEntry);
        //                Thread.Sleep(50);
        //            }

        //            PowerHistoryText.text = _gameClient.HistoryEntries.Count.ToString();
        //        }
        //    });

        if (AllAnimStatesAreNone)
        {
            if (_gameClient.HistoryEntries.TryDequeue(out IPowerHistoryEntry historyEntry))
            {
                PlayerState = PlayerClientState.Wait;
                ReadHistoryEntry(historyEntry);
            }

            if (_gameClient.HistoryEntries.Count() == 0 && PlayerState == PlayerClientState.Wait)
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
        if (_gameClient.PowerChoices == null)
        {
            _myChoices.gameObject.SetActive(false);
            return false;
        }

        Debug.Log($"Current PowerChoices: {_gameClient.PowerChoices.ChoiceType} with {_gameClient.PowerChoices.Entities.Count} entities");

        PowerChoicesText.text = $"{_gameClient.PowerChoices.Entities.Count}{_gameClient.PowerChoices.ChoiceType.ToString().Substring(0, 1)}";

        _myChoices.gameObject.SetActive(true);

        _gameClient.PowerChoices.Entities.ForEach(p => {

            if (!EntitiesExt.TryGetValue(p, out EntityExt entityExt))
            {
                throw new Exception($"Can't find entity with the id {p} in our dictionary!");
            }

            createCardIn(_myChoicesPanel, CardPrefab, entityExt);
        });

        return true;
    }

    //public void ReadPowerOptions(List<PowerOption> powerOptions)
    public bool ReadPowerOptions()
    {
        if (_gameClient.PowerOptionList.Count() == 0)
        {
            _endTurnButton.interactable = false;
            return false;
        }

        //Debug.Log($"Current PowerOptions: {_gameClient.PowerOptionList.Count()}");

        PowerOptionsText.text = _gameClient.PowerOptionList.Count().ToString();

        var endTurnOption = _gameClient.PowerOptionList.Find(p => p.OptionType == OptionType.END_TURN);
        _endTurnButton.interactable = endTurnOption != null;

        // display the other power options ...
        foreach (var powerOption in _gameClient.PowerOptionList)
        {

        }

        return true;
    }

    public void OnClickRandomMove()
    {

        if (_gameClient.PowerChoices != null)
        {
            var powerChoicesChoice = _gameClient.PowerChoices;
            var powerChoicesId = _random.Next(powerChoicesChoice.Entities.Count);

            _myChoicesPanel.gameObject.SetActive(false);

            Debug.Log($"powerChoicesChoice => choices:{powerChoicesId}");
            PlayerState = PlayerClientState.Wait;
            _gameClient.SendPowerChoicesChoice(new PowerChoices() { ChoiceType = powerChoicesChoice.ChoiceType, Entities = new List<int>() { powerChoicesChoice.Entities[powerChoicesId] } });
            return;
        }

        if (_gameClient.PowerOptionList.Count() == 0)
        {
            Debug.Log("No poweroptions to choose from!");
            return;
        }

        var powerOptionChoice = RandomPowerOption(_gameClient.PowerOptionList);

        // retry to get sme movements
        if (_gameClient.PowerOptionList.Count() > 1 && powerOptionChoice.PowerOption.OptionType == OptionType.END_TURN)
        {
            powerOptionChoice = RandomPowerOption(_gameClient.PowerOptionList);
        }

        Debug.Log($"powerOptionChoice => " +
                  $"target:{powerOptionChoice.Target}, " +
                  $"position: 0, " +
                  $"suboption: {powerOptionChoice.SubOption} " +
                  $"{powerOptionChoice.PowerOption.Print()}");

        _endTurnButton.interactable = powerOptionChoice.PowerOption.OptionType != OptionType.END_TURN;
        PlayerState = PlayerClientState.Wait;
        _gameClient.SendPowerOptionChoice(powerOptionChoice);
    }

    public PowerOptionChoice RandomPowerOption(List<PowerOption> powerOptionsList)
    {
        var powerOptionId = _random.Next(powerOptionsList.Count());
        var powerOption = powerOptionsList.ElementAt(powerOptionId);
        var target = powerOption.MainOption?.Targets != null && powerOption.MainOption.Targets.Count > 0
            ? powerOption.MainOption.Targets.ElementAt(_random.Next(powerOption.MainOption.Targets.Count))
            : 0;
        var subOption = powerOption.SubOptions != null && powerOption.SubOptions.Count > 0
            ? _random.Next(powerOption.SubOptions.Count)
            : 0;
        return new PowerOptionChoice() { PowerOption = powerOption, Target = target, Position = 0, SubOption = subOption };
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

        //var stats1 = _mainGame.transform.Find(GetParentObject("Stats", 1));
        //stats1.Find("AccountName").GetComponent<Text>().text = _gameClient.Player1.AccountName;
        //var stats2 = _mainGame.transform.Find(GetParentObject("Stats", 2));
        //stats2.Find("AccountName").GetComponent<Text>().text = _gameClient.Player2.AccountName;

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
            case GameTag.PLAYSTATE:
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
                CardGen cardGen;
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

    private string GetParentObject(string parentObjectName, int PlayerId)
    {
        if (_gameClient.PlayerId == PlayerId)
        {
            return $"My{parentObjectName}";
        }
        else
        {
            return $"Op{parentObjectName}";
        }

        //switch (PlayerId)
        //{
        //    case 1:
        //        return $"My{parentObjectName}";
        //    case 2:
        //        return $"Op{parentObjectName}";
        //    default:
        //        return null;
        //}
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
        // Debug.Log(showEntity.Print());
        if (!EntitiesExt.TryGetValue(showEntity.Entity.Id, out EntityExt entity))
        {
            throw new Exception($"Can't find entity with the id {showEntity.Entity.Id} in our dictionary!");
        }

        // checking for same same
        UpdateTags(entity, showEntity.Entity.Name, showEntity.Entity.Tags);

        var card = GetCardFromName(showEntity.Entity.Name);
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

        var card = GetCardFromName(fullEntity.Entity.Name);
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

    private Card GetCardFromName(string CardId)
    {
        if (CardId != null && CardId != string.Empty)
        {
            Card card = Cards.FromId(CardId);
            if (card != null)
            {
                return card;
            }
        }

        Debug.Log($"Can't find this card id '{CardId}'!");
        return null;
    }
}

