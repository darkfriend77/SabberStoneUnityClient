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
    public UnityGameController(IGameAI gameAi) : base(gameAi)
    {

    }

    public override void CallInitialisation()
    {
        base.CallInitialisation();
    }

    public override void CallPowerHistory()
    {
        base.CallPowerHistory();
    }

    public override void CallPowerChoices()
    {
        base.CallPowerChoices();
    }

    public override void CallPowerOptions()
    {
        base.CallPowerOptions();
    }
}