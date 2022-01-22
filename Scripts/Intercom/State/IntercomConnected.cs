using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomConnected : IntercomState, ITickable {

    public IntercomConnected(IIntercomState partner) : base(partner, "IntercomConnected") {
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE:
                return this;
            default:
                return base.onIncomingCommand(command);
        }        
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            default:
                return base.onCommand(mlCommand);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int)IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int)IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE, true);
    }
}