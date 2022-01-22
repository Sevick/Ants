using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomDisconnected : IntercomState {

    public IntercomDisconnected(IIntercomState partner) : base (partner, "IntercomDisconnected") {
    }

    override
    public IntercomState onConnect() {
        return new IntercomConnected(partner);
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:
                return new IntercomHandshake(partner);
            default:
                return base.onIncomingCommand(command);
        }
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_DONOTHING:
                // TODO: wait a few cycles?
                break;
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:
                partner.onIncomingCommand(command); // stay in current state waiting for response (accept/reject)
                break;
            default:
                return base.onCommand(mlCommand);
        }
        return this;
    }

    override
    public IntercomState onResponse(IIntercomState.IntercomCommands responseType) {
        switch (responseType) { 
            case IIntercomState.IntercomCommands.ACTION_CONNECT_ACCEPT :
                return new IntercomConnected(partner);
            case IIntercomState.IntercomCommands.ACTION_CONNECT_REJECT:
                return null;    // terminate Intercom
            default:
                return base.onResponse(responseType);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE, true);   
    }
}