using SabberStoneContract.Client;
using SabberStoneContract.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UnityGameClient : GameClient
{
    private UnityController _controller;

    private UnityGameController _gameController;

    public UnityGameClient(UnityController controller, string targetIp, int port, UnityGameController gameController) : base(targetIp, port, gameController)
    {
        _controller = controller;
        _gameController = gameController;
    }

    public override void CallGameClientState(GameClientState oldState, GameClientState newState)
    {
        base.CallGameClientState(oldState, newState);
        if (_controller != null)
        {
            _controller.ProccessGameClientState(oldState, newState);
        }
    }

    public override void CallInvitation()
    {
        _controller.ProccessInvitation();
    }
}

