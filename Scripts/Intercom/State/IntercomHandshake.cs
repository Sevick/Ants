using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomHandshake : IntercomState {

    public IntercomHandshake(IIntercomState partner) : base (partner, "IntercomHandshake") {
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:                
                break; // Do nothing - connection from that partner already initialized
            case (int)IIntercomState.IntercomCommands.ACTION_DONOTHING:
                // TODO: wait a few cycles?
                break;
            case IIntercomState.IntercomCommands.ACTION_CONNECT_ACCEPT:
                partner.onResponse(command);
                return new IntercomConnected(partner);
            case IIntercomState.IntercomCommands.ACTION_CONNECT_REJECT:
                partner.onResponse(command);
                return null;
            default:
                return base.onCommand(mlCommand);
        }
        return this;
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:
                return this; // Do nothing
            default:
                return base.onIncomingCommand(command);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_CONNECT_ACCEPT, true);
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_CONNECT_REJECT, true);
    }
}