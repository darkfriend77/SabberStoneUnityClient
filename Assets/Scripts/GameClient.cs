using System.Collections.Generic;
using UnityEngine;
using Grpc.Core;
using Newtonsoft.Json;
using SabberStoneContract.Model;
using SabberStoneCore.Kettle;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine.UI;

using static GameServerService;

public class GameClient : MonoBehaviour
{
    public int Port = 50051;

    public string AccountName = "TestClientX";

    public string AccountPassword = "xxxabc123";

    private string _target;

    private Channel _channel;

    private GameServerServiceClient _client;

    private GameClientState _gameClientState;

    public GameClientState GameClientState
    {
        get => _gameClientState;
        private set
        {
            Debug.Log($"SetClientState {value}");
            _gameClientState = value;
            DoUpdatedGfx = true;
        }
    }

    private IClientStreamWriter<GameServerStream> _writeStream;

    private int _sessionId;

    private string _sessionToken;

    private int _gameId;

    private int _playerId;

    public int PlayerId => _playerId;

    private List<UserInfo> _userInfos;

    private TaskCompletionSource<object> registerWaiter;

    public UserInfo MyUserInfo => _userInfos.FirstOrDefault(p => p.PlayerId == _playerId);

    public UserInfo OpUserInfo => _userInfos.FirstOrDefault(p => p.PlayerId != _playerId);

    public ConcurrentQueue<IPowerHistoryEntry> HistoryEntries { get; private set; }

    public PowerChoices PowerChoices { get; private set; }

    public List<PowerOption> PowerOptionList { get; private set; }
    
    // unity gui declaration stuff

    private Text clientStateText, connectButtonText, waitingTimeText;

    private Image connectButtonImage;

    private bool DoUpdatedGfx;

    private GameObject userWelcomeGrid, userAccountGrid, userMenuGrid, userQueueGrid, userInviteGrid, userPrepareGrid;

    private GameObject clientPanel, board, boardCanvas, menuCanvas;

    private Transform userDataContentParent, clientPanelParent;

    private InputField accountNameInputField;

    private Button loginButton;

    private float queuedTime;

    public GameObject UserDataEntry;

    private List<GameObject> userDataEntries;

    private GameController gameController;

    public bool DebugFlag = true;

    // Start is called before the first frame update
    void Start()
    {
        _target = $"127.0.0.1:{Port}";
        GameClientState = GameClientState.None;

        _gameId = -1;
        _playerId = -1;
        _userInfos = new List<UserInfo>();

        HistoryEntries = new ConcurrentQueue<IPowerHistoryEntry>();
        PowerOptionList = new List<PowerOption>();

        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        board = rootGameObjectList.Find(p => p.name == "Board");
        boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");
        menuCanvas = rootGameObjectList.Find(p => p.name == "MenuCanvas");
        gameController = rootGameObjectList.Find(p => p.name == "GameController").GetComponent<GameController>();
        gameController.SetGameClient(this);

        clientPanelParent = menuCanvas.transform.Find("ClientPanel");

        clientStateText = clientPanelParent.Find("ClientStateText").GetComponent<Text>();
        var connectButtonParent = clientPanelParent.Find("ConnectButton");
        connectButtonImage = connectButtonParent.GetComponent<Image>();
        connectButtonText = connectButtonParent.Find("ConnectButtonText").GetComponent<Text>();
        connectButtonText.text = "CONNECT";

        var userInfoPanelParent = clientPanelParent.Find("UserInfoPanel");
        userWelcomeGrid = userInfoPanelParent.Find("UserWelcomeGrid").gameObject;
        var userAccountGridParent = userInfoPanelParent.Find("UserAccountGrid");
        userAccountGrid = userAccountGridParent.gameObject;
        accountNameInputField = userAccountGridParent.Find("AccountNameInput").GetComponent<InputField>();
        loginButton = userAccountGridParent.Find("LoginButton").GetComponent<Button>();
        userMenuGrid = userInfoPanelParent.Find("UserMenuGrid").gameObject;
        var userQueueGridParent = userInfoPanelParent.Find("UserQueueGrid");
        waitingTimeText = userQueueGridParent.Find("WaitingTimeText").GetComponent<Text>();
        userQueueGrid = userQueueGridParent.gameObject;
        userInviteGrid = userInfoPanelParent.Find("UserInviteGrid").gameObject;
        userPrepareGrid = userInfoPanelParent.Find("UserPrepareGrid").gameObject;

        userDataContentParent = clientPanelParent.Find("StatsPanel").Find("Viewport").Find("UserDataContent");

        menuCanvas.SetActive(true);
        board.SetActive(false);
        boardCanvas.SetActive(false);

        userWelcomeGrid.SetActive(true);
        userAccountGrid.SetActive(false);
        userMenuGrid.SetActive(false);
        userQueueGrid.SetActive(false);
        userInviteGrid.SetActive(false);
        userPrepareGrid.SetActive(false);
        DoUpdatedGfx = true;

        queuedTime = 0;

        userDataEntries = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if (queuedTime > 0)
        {
            waitingTimeText.text = $"waiting .. {(int)(Time.time - queuedTime) % 60} sec";
        }

        if (DoUpdatedGfx)
        {
            clientStateText.text = _gameClientState.ToString();
            connectButtonText.text = _gameClientState == GameClientState.None ? "CONNECT" : "DISCONNECT";
            connectButtonImage.color = _gameClientState == GameClientState.None ? Color.green : Color.red;

            ClearChildren(userDataContentParent);
            
            // TODO
            //UserInfos.ForEach(user =>
            //{
            //    var userEntryGameObject = Instantiate(UserDataEntry, userDataContentParent);
            //    userEntryGameObject.transform.Find("Text").GetComponent<Text>().text = $"{user.Id},{user.AccountName},{user.UserState},{user.GameId}";
            //});

            if (DebugFlag)
            {
                menuCanvas.SetActive(false);
                board.SetActive(true);
                boardCanvas.SetActive(true);
            }
            else
            {
                //SetActiveChildren(clientPanelParent, clientState != _gameClientState.InGame);
                menuCanvas.SetActive(_gameClientState != GameClientState.InGame);

                queuedTime = _gameClientState == GameClientState.Queued ? Time.time : 0;
                userWelcomeGrid.SetActive(_gameClientState == GameClientState.None);
                userAccountGrid.SetActive(_gameClientState == GameClientState.Connected);
                loginButton.interactable = _gameClientState == GameClientState.Connected;
                userMenuGrid.SetActive(_gameClientState == GameClientState.Registred);
                userQueueGrid.SetActive(_gameClientState == GameClientState.Queued);
                userInviteGrid.SetActive(_gameClientState == GameClientState.Invited);
                board.SetActive(_gameClientState == GameClientState.InGame);
                boardCanvas.SetActive(_gameClientState == GameClientState.InGame);
            }

            DoUpdatedGfx = false;
        }

    }

    public void OnClickLogin()
    {
        Debug.Log($"OnClickLogin {accountNameInputField.text}");
        Register(AccountName, AccountPassword);
    }

    public void OnClickStats()
    {
        Debug.Log($"Not implemented: OnClickStats");
    }

    public void OnClickQueue()
    {
        Debug.Log($"OnClickQueue");
        Queue(GameType.Normal, DeckType.Random, null);
    }

    public void OnClickInvite()
    {
        Debug.Log($"Not implemented: OnClickInvite");
    }

    public void OnClickLogout()
    {
        Debug.Log($"Not implemented: OnClickLogout");
    }

    public void OnClickYesInvite()
    {
        Debug.Log($"OnClickYesInvite");
        SendInvitationReply(true);
    }

    public void OnClickNoInvite()
    {
        Debug.Log($"OnClickNoInvite");
        SendInvitationReply(false);
    }

    public void OnClickConnect()
    {
        if (_gameClientState == GameClientState.None)
        {
            Connect();
        }
        else
        {
            Disconnect();
        }
    }

    public void SetActiveChildren(Transform transform, bool value)
    {
        for (int j = 0; j < transform.childCount; j++)
        {
            transform.GetChild(j).gameObject.SetActive(value);
        }
    }

    public void ClearChildren(Transform transformParent)
    {
        int i = 0;

        //Array to hold all child obj
        GameObject[] allChildren = new GameObject[transformParent.childCount];

        //Find all child obj and store to that array
        foreach (Transform child in transformParent)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        //Now destroy them
        foreach (GameObject child in allChildren)
        {
            DestroyImmediate(child.gameObject);
        }
    }

    #region GAMECLIENT PART

    public void Connect()
    {
        _channel = new Channel(_target, ChannelCredentials.Insecure);
        _client = new GameServerServiceClient(_channel);
        GameClientState = GameClientState.Connected;
    }

    public async Task Register(string accountName, string accountPsw)
    {
        if (GameClientState != GameClientState.Connected)
        {
            Debug.LogWarning("Client isn't connected.");
            return;
        }

        var authReply = _client.Authentication(new AuthRequest { AccountName = accountName, AccountPsw = accountPsw });

        if (!authReply.RequestState)
        {
            Debug.LogWarning("Bad RegisterRequest.");
            return;
        }

        _sessionId = authReply.SessionId;
        _sessionToken = authReply.SessionToken;

        GameServerChannel();

        registerWaiter = new TaskCompletionSource<object>();
        await registerWaiter.Task;

        Debug.Log($"Register done.");
    }

    //public void MatchGame()
    //{
    //    if (GameClientState != GameClientState.InGame)
    //    {
    //        Log.Warn("Client isn't in a game.");
    //        return;
    //    }

    //    var matchGameReply = _client.MatchGame(new MatchGameRequest { GameId = _gameId }, new Metadata { new Metadata.Entry("token", _sessionToken) });

    //    if (!matchGameReply.RequestState)
    //    {
    //        Log.Warn("Bad MatchGameRequest.");
    //        return;
    //    }

    //    // TODO do something with the game object ...
    //    Log.Info($"Got match game successfully.");
    //}

    public async void GameServerChannel()
    {
        using (var call = _client.GameServerChannel(headers: new Metadata { new Metadata.Entry("token", _sessionToken) }))
        {
            // listen to game server
            var response = Task.Run(async () =>
            {
                while (await call.ResponseStream.MoveNext(CancellationToken.None) && GameClientState != GameClientState.None)
                {
                    try
                    {
                        ProcessChannelMessage(call.ResponseStream.Current);
                    }
                    catch
                    {
                        ;
                    }

                };
            });

            _writeStream = call.RequestStream;
            await WriteGameServerStream(MsgType.Initialisation, true, string.Empty);

            //await call.RequestStream.CompleteAsync();
            await response;
        }
    }

    public async Task WriteGameServerStream(MsgType messageType, bool messageState, string message)
    {
        if (_writeStream == null)
        {
            Debug.Log($"There is no write stream currently.");
            return;
        }

        await _writeStream.WriteAsync(new GameServerStream
        {
            MessageType = messageType,
            MessageState = messageState,
            Message = message
        });

        Debug.Log($"{AccountName} sent [{messageType}]");
    }

    public void WriteGameData(MsgType messageType, bool messageState, GameData gameData)
    {
        Debug.Log($"writing gamedata {messageType}");
        WriteGameServerStream(messageType, messageState, JsonConvert.SerializeObject(gameData));
    }

    public void SendInvitationReply(bool accept)
    {
        WriteGameData(MsgType.Invitation, accept, new GameData { GameId = _gameId, PlayerId = _playerId, GameDataType = GameDataType.None });
    }

    public void SendPowerChoicesChoice(PowerChoices powerChoices)
    {
        // clear before sent ...
        PowerChoices = null;
        WriteGameData(MsgType.InGame, true, new GameData() { GameId = _gameId, PlayerId = _playerId, GameDataType = GameDataType.PowerChoices, GameDataObject = JsonConvert.SerializeObject(powerChoices) });
    }

    public void SendPowerOptionChoice(PowerOptionChoice powerOptionChoice)
    {
        // clear before sent ...
        PowerOptionList.Clear();
        WriteGameData(MsgType.InGame, true, new GameData() { GameId = _gameId, PlayerId = _playerId, GameDataType = GameDataType.PowerOptions, GameDataObject = JsonConvert.SerializeObject(powerOptionChoice) });
    }


    public async void Disconnect()
    {
        if (_writeStream != null)
        {
            await _writeStream.CompleteAsync();
        }

        GameClientState = GameClientState.None;

        await _channel.ShutdownAsync();

        DoUpdatedGfx = true;
    }

    private void ProcessChannelMessage(GameServerStream current)
    {
        if (!current.MessageState)
        {
            Debug.Log($"Failed messageType {current.MessageType}, '{current.Message}'!");
            return;
        }

        GameData gameData = null;
        if (current.Message != string.Empty)
        {
            gameData = JsonConvert.DeserializeObject<GameData>(current.Message);
            //Debug.Log($"GameData[Id:{gameData.GameId},Player:{gameData.PlayerId}]: {gameData.GameDataType} received");
        }
        else
        {
            //Debug.Log($"Message[{current.MessageState},{current.MessageType}]: received.");
        }

        switch (current.MessageType)
        {
            case MsgType.Initialisation:
                GameClientState = GameClientState.Registred;
                break;

            case MsgType.Invitation:
                _gameId = gameData.GameId;
                _playerId = gameData.PlayerId;
                GameClientState = GameClientState.Invited;
                break;

            case MsgType.InGame:
                switch (gameData.GameDataType)
                {
                    case GameDataType.Initialisation:
                        _userInfos = JsonConvert.DeserializeObject<List<UserInfo>>(gameData.GameDataObject);
                        GameClientState = GameClientState.InGame;
                        Debug.Log($"Initialized game against account {OpUserInfo.AccountName}!");
                        break;

                    case GameDataType.PowerHistory:
                        List<IPowerHistoryEntry> powerHistoryEntries = JsonConvert.DeserializeObject<List<IPowerHistoryEntry>>(gameData.GameDataObject, new PowerHistoryConverter());
                        powerHistoryEntries.ForEach(p => HistoryEntries.Enqueue(p));
                        break;

                    case GameDataType.PowerChoices:
                        PowerChoices = JsonConvert.DeserializeObject<PowerChoices>(gameData.GameDataObject);
                        break;

                    case GameDataType.PowerOptions:
                        var powerOptions = JsonConvert.DeserializeObject<PowerOptions>(gameData.GameDataObject);
                        PowerOptionList = powerOptions.PowerOptionList;
                        break;

                    case GameDataType.Result:

                        //Log.Info($" ... ");
                        GameClientState = GameClientState.Registred;
                        break;
                }
                break;
        }
    }

    private void Queue(GameType gameType = GameType.Normal, DeckType deckType = DeckType.Random, string deckData = null)
    {
        if (_gameClientState != GameClientState.Registred)
        {
            Debug.Log("GameClient isn't registred.");
            return;
        }

        var queueReply = _client.GameQueue(
            new QueueRequest
            {
                GameType = gameType,
                DeckType = deckType,
                DeckData = deckData ?? string.Empty
            },
            new Metadata
            {
                new Metadata.Entry("token", _sessionToken)
            });

        if (!queueReply.RequestState)
        {
            Debug.Log("Bad QueueRequest.");
            return;
        }

        GameClientState = GameClientState.Queued;
    }

    #endregion

}
