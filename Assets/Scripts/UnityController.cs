using SabberStoneContract.Core;
using SabberStoneContract.Interface;
using SabberStoneCore.Kettle;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UnityController : MonoBehaviour
{
    private GameObject _menuCanvas, _board, _boardCanvas,
        _userWelcomeGrid, _userAccountGrid, _userMenuGrid,
        _userQueueGrid, _userInviteGrid, _userPrepareGrid;

    private Transform _clientPanel;

    private InputField _accountNameInputField, _accountPasswordInputField, _deckStringInput;

    private Dropdown _deckTypeDropDown;

    private Button _loginButton;

    private Text _clientStateText, _connectButtonText, _waitingTimeText;

    private Image _connectButtonImage;

    private float _queuedTime;

    public PowerInterpreter PowerInterpreter;

    private UnityGameController _gameController;

    private UnityGameClient _gameClient;

    private bool _newClientState;

    // Start is called before the first frame update
    void Start()
    {
        var rootGameObjectList = UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects().ToList();
        _menuCanvas = rootGameObjectList.Find(p => p.name == "MenuCanvas");
        _board = rootGameObjectList.Find(p => p.name == "Board");
        _boardCanvas = rootGameObjectList.Find(p => p.name == "BoardCanvas");

        _clientPanel = _menuCanvas.transform.Find("ClientPanel");
        _clientStateText = _clientPanel.Find("ClientStateText").GetComponent<Text>();

        var connectButton = _clientPanel.Find("ConnectButton");
        _connectButtonImage = connectButton.GetComponent<Image>();
        _connectButtonText = connectButton.Find("ConnectButtonText").GetComponent<Text>();

        var userInfoPanelParent = _clientPanel.Find("UserInfoPanel");
        _userWelcomeGrid = userInfoPanelParent.Find("UserWelcomeGrid").gameObject;
        var userAccountGridParent = userInfoPanelParent.Find("UserAccountGrid");
        _userAccountGrid = userAccountGridParent.gameObject;
        _accountNameInputField = userAccountGridParent.Find("AccountNameInput").GetComponent<InputField>();
        _accountPasswordInputField = userAccountGridParent.Find("AccountPasswordInput").GetComponent<InputField>();
        _loginButton = userAccountGridParent.Find("LoginButton").GetComponent<Button>();
        _userMenuGrid = userInfoPanelParent.Find("UserMenuGrid").gameObject;
        var userQueueGridParent = userInfoPanelParent.Find("UserQueueGrid");
        _waitingTimeText = userQueueGridParent.Find("WaitingTimeText").GetComponent<Text>();
        _userQueueGrid = userQueueGridParent.gameObject;
        _userInviteGrid = userInfoPanelParent.Find("UserInviteGrid").gameObject;
        _userPrepareGrid = userInfoPanelParent.Find("UserPrepareGrid").gameObject;
        _deckStringInput = _userPrepareGrid.transform.Find("DeckStringInput").GetComponent<InputField>();

        _clientPanel.gameObject.SetActive(true);
        _board.SetActive(false);
        _boardCanvas.SetActive(false);

        _gameController = new UnityGameController(this, new RandomAI());
        _gameClient = new UnityGameClient(this, "127.0.0.1", 50051, _gameController);

        // initial mocked state
        ProccessGameClientState(GameClientState.None, GameClientState.None);
    }

    // Update is called once per frame
    void Update()
    {
        if (_queuedTime > 0)
        {
            _waitingTimeText.text = $"waiting .. {(int)(Time.time - _queuedTime) % 60} sec";
        }

        if (_newClientState)
        {
            _newClientState = false;

            var clientState = _gameClient.GameClientState;

            _clientStateText.text = clientState.ToString();
            _connectButtonText.text = clientState == GameClientState.None ? "CONNECT" : "DISCONNECT";
            _connectButtonText.color = clientState == GameClientState.None ? new Color(0.195f, 0.195f, 0.195f) : Color.white;
            _connectButtonImage.color = clientState == GameClientState.None ? Color.white : Color.gray;

            _menuCanvas.SetActive(clientState != GameClientState.InGame);

            // prep queue is unity specific
            _userPrepareGrid.SetActive(false);

            _queuedTime = clientState == GameClientState.Queued ? Time.time : 0;
            _userWelcomeGrid.SetActive(clientState == GameClientState.None);
            _userAccountGrid.SetActive(clientState == GameClientState.Connected);
            _loginButton.interactable = clientState == GameClientState.Connected;
            _userMenuGrid.SetActive(clientState == GameClientState.Registered);
            _userQueueGrid.SetActive(clientState == GameClientState.Queued);
            _userInviteGrid.SetActive(clientState == GameClientState.Invited);
            _board.SetActive(clientState == GameClientState.InGame);
            _boardCanvas.SetActive(clientState == GameClientState.InGame);

        }
    }

    void OnApplicationQuit()
    {
        _gameClient.Disconnect();
    }

    internal void ProccessGameClientState(GameClientState oldState, GameClientState newState)
    {
        Debug.Log($"{oldState} -> {newState}");

        _newClientState = true;
    }

    internal void ProccessInvitation()
    {
        Debug.Log($"ProccessInvitation()");
    }

    internal void ProccessInitialisation()
    {
        PowerInterpreter.SetPlayerAndUserInfo(_gameController.PlayerId, _gameController.MyUserInfo, _gameController.OpUserInfo);
    }

    internal void ProccessPowerHistory()
    {
        while (!_gameController.HistoryEntries.IsEmpty)
        {
            if (_gameController.HistoryEntries.TryDequeue(out IPowerHistoryEntry historyEntry))
            {
                PowerInterpreter.AddHistoryEntry(historyEntry);
            }
        }
    }

    internal void ProccessPowerChoices()
    {
        PowerInterpreter.AddPowerEntityChoices(
            new PowerEntityChoices(_gameController.PowerChoices.Index, _gameController.PowerChoices.ChoiceType, _gameController.PowerChoices.Entities));
    }

    internal void ProccessPowerOptions()
    {
        PowerInterpreter.AddPowerOptions(_gameController.PowerOptions);
    }

    public void OnClickConnect()
    {
        if (_gameClient.GameClientState == GameClientState.None)
        {
            _gameClient.Connect();
        }
        else
        {
            _gameClient.Disconnect();
        }
    }

    public void OnClickDisconnect()
    {
        _gameClient.Disconnect();
    }

    public void OnClickLogin()
    {
        var accountName = _accountNameInputField.text == string.Empty ? "Test123" : _accountNameInputField.text;
        var accountPassword = _accountPasswordInputField.text == string.Empty ? "abcdef1234" : _accountPasswordInputField.text;
        _gameClient.Register(accountName, accountPassword);
    }

    public void OnClickDebugGame()
    {
        _clientPanel.gameObject.SetActive(false);
        _board.SetActive(true);
        _boardCanvas.SetActive(true);

        PowerInterpreter.InitializeDebug();
    }

    public void OnClickPreparedOkay()
    {
        if (_deckStringInput.text != string.Empty)
        {
            Debug.Log($"DeckString: {_deckStringInput.text}");
            _gameClient.Queue(GameType.Normal, _deckStringInput.text);
        }
        else
        {
            OnClickPrepareBack();
        }

    }

    public void OnClickPrepareBack()
    {
        _userPrepareGrid.SetActive(false);
        _userMenuGrid.SetActive(true);
    }

    public void OnClickQueue()
    {
        _userPrepareGrid.SetActive(true);
        _userMenuGrid.SetActive(false);
    }

    public void OnClickInviteAccept()
    {
        _gameController.SendInvitationReply(true);
    }

    public void OnClickInviteDecline()
    {
        _gameController.SendInvitationReply(false);
    }

    public void OnClickSendPowerChoices()
    {
        _gameController.SendBasePowerChoices();
    }

    public void OnClickSendPowerOptions()
    {
        _gameController.SendBasePowerOptions();
    }
}
