using SabberStoneContract.Core;
using SabberStoneContract.Interface;
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

    private InputField _accountNameInputField;

    private Button _loginButton;

    private Text _clientStateText, _connectButtonText, _waitingTimeText;

    private Image _connectButtonImage;

    private float _queuedTime;

    public PowerInterpreter PowerInterpreter;

    private UnityGameController _gameController;

    private UnityGameClient _gameClient;

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
        _loginButton = userAccountGridParent.Find("LoginButton").GetComponent<Button>();
        _userMenuGrid = userInfoPanelParent.Find("UserMenuGrid").gameObject;
        var userQueueGridParent = userInfoPanelParent.Find("UserQueueGrid");
        _waitingTimeText = userQueueGridParent.Find("WaitingTimeText").GetComponent<Text>();
        _userQueueGrid = userQueueGridParent.gameObject;
        _userInviteGrid = userInfoPanelParent.Find("UserInviteGrid").gameObject;
        _userPrepareGrid = userInfoPanelParent.Find("UserPrepareGrid").gameObject;

        _clientPanel.gameObject.SetActive(true);
        _board.SetActive(false);
        _boardCanvas.SetActive(false);

        _gameController = new UnityGameController(new RandomAI());
        _gameClient = new UnityGameClient(this, "127.0.0.1", 50051, _gameController);

        // initial mocked state
        ProccessGameClientState(GameClientState.None, GameClientState.None);
    }

    internal void ProccessGameClientState(GameClientState oldState, GameClientState newState)
    {
        _clientStateText.text = newState.ToString();
        _connectButtonText.text = newState == GameClientState.None ? "CONNECT" : "DISCONNECT";
        _connectButtonText.color = newState == GameClientState.None ? Color.black : Color.white;
        _connectButtonImage.color = newState == GameClientState.None ? Color.white : Color.gray;

        _menuCanvas.SetActive(newState != GameClientState.InGame);

        _queuedTime = newState == GameClientState.Queued ? Time.time : 0;
        _userWelcomeGrid.SetActive(newState == GameClientState.None);
        _userAccountGrid.SetActive(newState == GameClientState.Connected);
        _loginButton.interactable = newState == GameClientState.Connected;
        _userMenuGrid.SetActive(newState == GameClientState.Registred);
        _userQueueGrid.SetActive(newState == GameClientState.Queued);
        _userInviteGrid.SetActive(newState == GameClientState.Invited);
        _board.SetActive(newState == GameClientState.InGame);
        _boardCanvas.SetActive(newState == GameClientState.InGame);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        _gameClient.Disconnect();
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


    public void OnClickDebugGame()
    {
        _clientPanel.gameObject.SetActive(false);
        _board.SetActive(true);
        _boardCanvas.SetActive(true);

        PowerInterpreter.InitializeDebug();
    }
}
