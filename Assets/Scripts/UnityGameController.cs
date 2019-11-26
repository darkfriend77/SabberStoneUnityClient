using SabberStoneContract.Client;
using SabberStoneContract.Core;
using SabberStoneContract.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class UnityGameController : GameController
{
    private UnityController _controller;

    public UnityGameController(UnityController controller, IGameAI gameAi) : base(gameAi)
    {
        _controller = controller;
    }

    protected override void CallInitialisation()
    {
        _controller.ProccessInitialisation();
    }

    protected override void CallPowerHistory()
    {
        _controller.ProccessPowerHistory();
    }

    protected override void CallPowerChoices()
    {
        _controller.ProccessPowerChoices();
    }

    protected override void CallPowerOptions()
    {
        _controller.ProccessPowerOptions();
    }

    public void SendBasePowerChoices()
    {
        base.CallPowerChoices();
    }

    public void SendBasePowerOptions()
    {
        base.CallPowerOptions();
    }

}