using SabberStoneContract.Model;
using SabberStoneCore.Config;
using SabberStoneCore.Enums;
using SabberStoneCore.Kettle;
using SabberStoneCore.Model;
using SabberStoneCore.Tasks.PlayerTasks;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public partial class PowerInterpreter : MonoBehaviour
{
    private Dictionary<int, EntityExt> EntitiesExt = new Dictionary<int, EntityExt>();

    public GameObject HeroPrefab;

    public GameObject HeroPowerPrefab;

    public GameObject HeroWeaponPrefab;

    public GameObject CardPrefab;

    public GameObject MinionPrefab;

    public GameObject ManaPrefab;

    private EntityExt attackingEntity;

    private EntityExt defendingEntity;

    private Transform gridParent, _mainGame, _myChoices, _myChoicesPanel;

    public bool AllAnimStatesAreNone => EntitiesExt.Values.ToList().TrueForAll(p => p.GameObjectScript == null || p.GameObjectScript.AnimState == AnimationState.NONE);

    private int _playerId;

    public void SetPlayerId(int playerId)
    {
        _playerId = playerId;
    }

    private Button _endTurnButton;

    private Game _game;

    private bool DoUpdatedOptions;

    private int _stepper;

    private ConcurrentQueue<IPowerHistoryEntry> _historyEntries;

    public void AddHistoryEntry(IPowerHistoryEntry historyEntry)
    {
        _historyEntries.Enqueue(historyEntry);
    }

    private PowerEntityChoices _powerEntityChoices;

    public void AddPowerEntityChoices(PowerEntityChoices entityChoices)
    {
        _powerEntityChoices = entityChoices;
    }

    private int _currentPowerEntityChoicesIndex;

    private PowerOptions _powerOptions;

    public void AddPowerOptions(PowerOptions powerOptions)
    {
        _powerOptions = powerOptions;
    }

    private int _currentPowerOptionsIndex;

    public Text PowerHistoryText, PowerOptionsText, PowerChoicesText, PlayerStateText;

    public EntityExt MyPlayer => EntitiesExt.Values.FirstOrDefault(p => p.Tags.TryGetValue(GameTag.PLAYER_ID, out int value) && value == _playerId);

    private Func<int, Game, bool> _gameStepper;

    private Button _btnStepper;

    public bool DebugFlag = false;

    public long Seed = 1111;

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
    }

    private void OnDisable()
    {

    }

    public void Start()
    {
        PlayerState = PlayerClientState.None;

        //consoleScrollRect = transform.parent.Find("Console").GetComponent<ScrollRect>();
        //gridParent = transform.parent.Find("Console").Find("Viewport").Find("Grid");
        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        var menuCanvas = rootGameObjectList.Find(p => p.name == "MenuCanvas");

        var boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");
        _mainGame = boardCanvas.transform.Find("MainGame");

        //_mainGame.transform.Find("MyHand").gameObject.SetActive(false);
        //_mainGame.transform.Find("OpHand").gameObject.SetActive(false);

        _myChoices = _mainGame.transform.Find("MyChoices");
        _myChoicesPanel = _myChoices.transform.Find("MyChoicesPanel");
        _myChoices.gameObject.SetActive(false);
        _endTurnButton = _mainGame.transform.Find("EndTurnButton").GetComponent<Button>();
        _endTurnButton.interactable = false;

        _currentPowerEntityChoicesIndex = int.MaxValue;
        _currentPowerOptionsIndex = int.MaxValue;

        _historyEntries = new ConcurrentQueue<IPowerHistoryEntry>();

        _btnStepper = boardCanvas.transform.Find("Panel").Find("Buttons").Find("BtnStepper").GetComponent<Button>();

    }

    public void InitializeDebug()
    {
        SetPlayerId(1);
        _game = new Game(DruidVsWarrior(Seed));
        _gameStepper = DruidVsWarriorMoves;
    }

    public void OnClickStepByStep()
    {
        if (_gameStepper == null || _game == null)
        {
            Debug.Log("no stepper or game initialized");
            return;
        }

        _btnStepper.interactable = false;

        _gameStepper(_stepper, _game);
        _game.PowerHistory.Last.ForEach(p =>
        {
            _historyEntries.Enqueue(p);
            PowerHistoryText.text = _historyEntries.Count().ToString();
        });
        PowerHistoryText.text = _historyEntries.Count().ToString();

        _powerEntityChoices = PowerChoicesBuilder.EntityChoices(_game, _game.Player1.Choice);

        var powerOptions = PowerOptionsBuilder.AllOptions(_game, _game.Player1.Options());
        _powerOptions = new PowerOptions()
        {
            Index = powerOptions.Index,
            PowerOptionList = powerOptions.PowerOptionList
        };

        _stepper++;

        _btnStepper.interactable = true;
    }

    public void Update()
    {
        ReadHistory();
    }

    public void OnClickEndTurn()
    {

    }

    //public void ReadHistory(List<IPowerHistoryEntry> powerHistoryEntries)
    public void ReadHistory()
    {
        if (AllAnimStatesAreNone)
        {

            if (!_historyEntries.IsEmpty)
            {
                PlayerState = PlayerClientState.Wait;

                if (_historyEntries.TryDequeue(out IPowerHistoryEntry historyEntry))
                {
                    ReadHistoryEntry(historyEntry);

                    PowerHistoryText.text = _historyEntries.Count().ToString();
                }

                PlayerState = PlayerClientState.Wait;
                PowerChoicesText.text = "0";
                PowerOptionsText.text = "0";
            }
            else
            {
                if (PlayerState != PlayerClientState.Choice && ReadPowerChoices())
                {
                    PlayerState = PlayerClientState.Choice;
                }
                else if (PlayerState != PlayerClientState.Option && ReadPowerOptions())
                {
                    PlayerState = PlayerClientState.Option;
                }
            }
        }
    }

    public bool ReadPowerChoices()
    {
        if (_powerEntityChoices == null || _powerEntityChoices.Entities == null || _powerEntityChoices.Entities.Count == 0)
        {
            _myChoices.gameObject.SetActive(false);
            _myChoicesPanel.gameObject.GetComponent<CardContainer>().Clear();
            return false;
        }
        //else if (_powerEntityChoices.Index <= _currentPowerEntityChoicesIndex)
        //{
        //    return false;
        //}

        Debug.Log($"Current PowerChoices: {_powerEntityChoices.ChoiceType} with {_powerEntityChoices.Entities.Count} entities, {_powerEntityChoices.Index}");

        PowerChoicesText.text = _powerEntityChoices != null ? "1" : "0";

        _myChoices.gameObject.SetActive(true);

        _powerEntityChoices.Entities.ForEach(p =>
        {

            if (!EntitiesExt.TryGetValue(p, out EntityExt entityExt))
            {
                throw new Exception($"Can't find entity with the id {p} in our dictionary!");
            }

            if (entityExt.CardId != "GAME_005")
            {
                var gameObject = Instantiate(CardPrefab, _mainGame.transform).gameObject;
                var cardGen = gameObject.GetComponent<CardGen>();
                cardGen.Generate(entityExt);
                _myChoicesPanel.GetComponent<CardContainer>().Add(gameObject);
            }
        });

        _currentPowerEntityChoicesIndex = _powerEntityChoices.Index;
        return true;
    }

    //public void ReadPowerOptions(List<PowerOption> powerOptions)
    public bool ReadPowerOptions()
    {
        if (_powerOptions == null || _powerOptions.PowerOptionList == null || _powerOptions.PowerOptionList.Count == 0)
        {
            _endTurnButton.interactable = false;
            return false;
        }
        //else if (_powerOptions.Index <= _currentPowerOptionsIndex)
        //{
        //    return false;
        //}

        PowerOptionsText.text = _powerOptions.PowerOptionList.Count.ToString();

        //var endTurnOption = PowerOptionList.Find(p => p.OptionType == OptionType.END_TURN);
        //_endTurnButton.interactable = endTurnOption != null;

        //// display the other power options ...
        //foreach (var powerOption in PowerOptionList)
        //{

        //}

        _currentPowerOptionsIndex = _powerOptions.Index;
        return true;
    }

    public void OnClickRandomMove()
    {

    }

    private void ReadHistoryEntry(IPowerHistoryEntry historyEntry)
    {
        if (DebugFlag)
        {
            Debug.Log(historyEntry.Print());
        }

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
        Debug.Log(blockStart.Print());
        if (blockStart.BlockType == BlockType.PLAY && blockStart.Target != 0)
        {
            if (!EntitiesExt.TryGetValue(blockStart.Source, out EntityExt sourceEntityExt))
            {
                throw new Exception($"Can't find entity with the source id {blockStart.Source} in our dictionary!");
            }

            if (!EntitiesExt.TryGetValue(blockStart.Target, out EntityExt targetEntityExt))
            {
                throw new Exception($"Can't find entity with the target id {blockStart.Target} in our dictionary!");
            }

            Debug.Log($"Source {sourceEntityExt.Name} is targeting {targetEntityExt.Name} !!!");

            var animationGen = sourceEntityExt.GameObjectScript.GetComponent<AnimationGen>();
            if (animationGen != null)
            {
                animationGen.TargetingAnim(targetEntityExt.GameObjectScript.gameObject);
            }
        }
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
            case GameTag.NEXT_STEP:
                if ((Step)tagChange.Value == Step.MAIN_BEGIN)
                {
                    _mainGame.transform.Find("MyHand").gameObject.SetActive(true);
                    _mainGame.transform.Find("OpHand").gameObject.SetActive(true);
                }
                break;

            case GameTag.MULLIGAN_STATE:
                Debug.Log($"MULLIGAN_STATE = {(Mulligan)tagChange.Value}");
                break;

            case GameTag.STATE:
                // TODO Implement game end screen .... completet
                Debug.Log($"STATE = {(State)tagChange.Value}");
                switch ((State)tagChange.Value)
                {
                    case State.INVALID:
                        break;
                    case State.LOADING:
                        break;
                    case State.RUNNING:
                        break;
                    case State.COMPLETE:
                        var playState = (PlayState)MyPlayer.Tags[GameTag.PLAYSTATE];
                        Debug.Log($"MyPlayer playState = {playState}");
                        GameFinished(playState);
                        break;
                }
                break;

            case GameTag.PLAYSTATE:
                // TODO Implement game end screen ....
                Debug.Log($"PLAYSTATE = {(PlayState)tagChange.Value}");
                break;

            //case GameTag.HERO_ENTITY:
            //    var heroParent = transform.Find(GetParentObject("Hero", entityExt));
            //    var hero = Instantiate(HeroPrefab, heroParent).gameObject.GetComponent<HeroGen>();
            //    hero.Generate(EntitiesExt[tagChange.Value]);
            //    EntitiesExt[tagChange.Value].GameObjectScript = hero;
            //    break;

            case GameTag.ZONE:
                //Debug.Log($"UPDATE_ENTITY_ZONE {(Zone)tagChange.Value}");
                DoZoneChange(entityExt, preValue != null ? (Zone)preValue : Zone.INVALID, (Zone)tagChange.Value);
                break;

            case GameTag.ZONE_POSITION:
                DoZonePositionChange(entityExt);
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
                break;

            case GameTag.ATTACKING:
                attackingEntity = tagChange.Value == 1 ? entityExt : null;
                break;
            case GameTag.DEFENDING:
                defendingEntity = tagChange.Value == 1 ? entityExt : null;
                break;

            case GameTag.DAMAGE:
                Debug.Log($"Doing damage ... to {entityExt.Name}");
                if (entityExt == defendingEntity && attackingEntity.Zone == Zone.PLAY) // && attackingEntity.CardType == CardType.MINION)
                {
                    Debug.Log(".. attack animation now !!!");
                    //attackingEntity.GameObjectScript.transform.GetComponent<MinionAnimation>().AnimAttack(defendingEntity.GameObjectScript.gameObject);
                    attackingEntity.GameObjectScript.transform.GetComponent<AnimationGen>().MinionAttackAnim(defendingEntity.GameObjectScript.gameObject);
                }

                var characterGen = entityExt.GameObjectScript.gameObject.GetComponent<AnimationGen>();
                if (characterGen != null)
                {
                    characterGen.DamageOrHealAnim(oldValue - tagChange.Value);
                }
                else
                {
                    Debug.LogError("Something bad happened, damage to a non-character?!");
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
            case GameTag.UNTOUCHABLE:
            case GameTag.EXHAUSTED:
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
                //Debug.Log(tagChange.Print());
                break;
        }

    }

    private void GameFinished(PlayState playState)
    {
        _mainGame.transform.Find("GameInfo").GetComponent<GameInfo>().GameInfoAnim(playState);
    }

    private void DoZoneChange(EntityExt entityExt, Zone prevZone, Zone nextZone)
    {
        Debug.Log($"{entityExt.Name} from {prevZone} to {nextZone}!");

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
                    case Zone.PLAY:
                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                CreateMinion(ref entityExt);
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

            case Zone.HAND:
                _mainGame.transform.Find(GetParentObject("Hand", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                switch (nextZone)
                {
                    case Zone.DECK:
                        // MULLIGAN
                        entityExt.GameObjectScript.gameObject.GetComponent<CardGen>().Show(false);
                        _mainGame.transform.Find(GetParentObject("Deck", entityExt)).GetComponent<CardContainer>().Add(entityExt.GameObjectScript.gameObject);
                        break;
                    case Zone.GRAVEYARD:
                        // DISCARD
                        entityExt.GameObjectScript.gameObject.GetComponent<CardGen>().DestroyAnim();
                        break;

                    case Zone.PLAY:
                        _mainGame.transform.Find(GetParentObject("Play", entityExt)).GetComponent<CardContainer>().Add(entityExt.GameObjectScript.gameObject);

                        // destroying old object, as we are building a new one.
                        entityExt.GameObjectScript.gameObject.GetComponent<CardGen>().DestroyAnim();
                        //Destroy(entityExt.GameObjectScript.gameObject);

                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                CreateMinion(ref entityExt);
                                break;

                            case CardType.WEAPON:
                                CreateWeapon(ref entityExt);
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
                    case Zone.HAND:
                        _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                        Destroy(entityExt.GameObjectScript.gameObject);
                        entityExt.GameObjectScript = createCardIn("Hand", CardPrefab, entityExt);
                        break;

                    case Zone.GRAVEYARD:
                        switch (entityExt.CardType)
                        {
                            case CardType.MINION:
                                _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Remove(entityExt.GameObjectScript.gameObject);
                                entityExt.GameObjectScript.gameObject.GetComponent<MinionGen>().DestroyAnim();
                                //Destroy(entityExt.GameObjectScript.gameObject);
                                break;

                            case CardType.WEAPON:
                                entityExt.GameObjectScript.gameObject.GetComponent<HeroWeaponGen>().DestroyAnim();
                                //Destroy(entityExt.GameObjectScript.gameObject);
                                break;

                            case CardType.SPELL:
                                // spells move to graveyard ...
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
                switch (nextZone)
                {
                    case Zone.HAND:
                        // Cards generated by spells
                        entityExt.GameObjectScript = createCardIn("Hand", CardPrefab, entityExt);
                        break;

                    default:
                        Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                        break;
                }
                break;
            case Zone.SECRET:
            default:
                Debug.Log($"Not implemented! {entityExt.Name} - {prevZone} => {nextZone}, for {entityExt.CardType}!");
                break;
        }

    }

    private void CreateWeapon(ref EntityExt entityExt)
    {
        var heroWeaponParent = _mainGame.transform.Find(GetParentObject("HeroWeapon", entityExt));
        var gameObject = Instantiate(HeroWeaponPrefab, heroWeaponParent.transform).gameObject;
        var heroWeaponGen = gameObject.GetComponent<HeroWeaponGen>();
        heroWeaponGen.Generate(entityExt);
        entityExt.GameObjectScript = heroWeaponGen;
    }

    private void CreateMinion(ref EntityExt entityExt)
    {
        var gameObject = Instantiate(MinionPrefab, _mainGame.transform).gameObject;
        var minionGen = gameObject.GetComponent<MinionGen>();
        minionGen.Generate(entityExt);
        entityExt.GameObjectScript = minionGen;

        _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Add(gameObject);
    }

    private void DoZonePositionChange(EntityExt entityExt)
    {
        switch ((Zone)entityExt.Tags[GameTag.ZONE])
        {
            case Zone.PLAY:
            case Zone.HAND:
                _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Order();
                break;
            default:
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
        if (_playerId == playerId)
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
            entity.Origin = card.Tags;
            entity.Name = card.Name;
            entity.Description = card.Text;
        }

        if (entity.GameObjectScript is CardGen cardGen)
        {
            cardGen.ShowEntity(entity);
        }

    }

    private void UpdateTags(EntityExt value, string name, IDictionary<GameTag, int> tags)
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
                    if (keyValue.Key == GameTag.ZONE || keyValue.Key == GameTag.ZONE_POSITION)
                    {
                        Debug.Log($"Ignoring[{value.Name}] => [CHANGE_TAG] {keyValue.Key}: {oldValue} => {keyValue.Value}");
                    }
                    else
                    {
                        Debug.Log($"[CHANGE_TAG] {keyValue.Key}: {oldValue} => {keyValue.Value}");
                        oldTags[keyValue.Key] = keyValue.Value;
                    }
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
        var card = GetCardFromName(fullEntity.Entity);

        var entityExt = new EntityExt()
        {
            Origin = card != null ? card.Tags : null,
            Id = fullEntity.Entity.Id,
            CardId = fullEntity.Entity.Name,
            Name = card != null ? card.Name : "missing",
            Description = card != null ? card.Text : "missing",
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
                switch (cardType)
                {
                    case CardType.HERO_POWER:
                        var heroPowerParent = _mainGame.transform.Find(GetParentObject("HeroPower", entityExt));
                        var heroPower = Instantiate(HeroPowerPrefab, heroPowerParent).gameObject.GetComponent<HeroPowerGen>();
                        heroPower.Generate(entityExt);
                        entityExt.GameObjectScript = heroPower;
                        break;

                    // Summon Minion
                    case CardType.MINION:
                        var gameObject = Instantiate(MinionPrefab, _mainGame.transform).gameObject;
                        var minionGen = gameObject.GetComponent<MinionGen>();
                        minionGen.Generate(entityExt);
                        entityExt.GameObjectScript = minionGen;

                        _mainGame.transform.Find(GetParentObject("Board", entityExt)).GetComponent<CardContainer>().Add(gameObject);
                        break;
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

        if (cardId == null || cardId == string.Empty)
        {
            Debug.Log($"We got an unknown entity with id {entity.Id}.");
            return null;
        }

        Card card = Cards.FromId(cardId);
        if (card != null)
        {
            return card;
        }

        Debug.LogError($"Can't find this card id '{cardId}' {entity.Print()}!");
        return null;
    }
}

