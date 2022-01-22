using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomStateMachine : IIntercomState, ITickable {

    //private IIntercomState intercomState = new IntercomStateDisconnected();
    IntercomState currentState = null;
    IIntercomState partner;

    public IntercomStateMachine() {
    }

    public IntercomStateMachine(IIntercomState partner) {
        init(partner);
    }

    public void init(IIntercomState partner) {
        this.currentState = new IntercomDisconnected(partner);
        this.partner = partner;
    }


    public void setContext(Context context) {
        currentState.setContext(context);
    }

    public IntercomState onConnect() {
        currentState = currentState.onConnect();
        return currentState;
    }

    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        currentState = currentState.onIncomingCommand(command);
        return currentState;
    }

    public IntercomState onCommand(MLCommand mlCommand) {
        currentState = currentState.onCommand(mlCommand);
        return currentState;
    }

    public IntercomState onResponse(IIntercomState.IntercomCommands responseType) {
        currentState = currentState.onResponse(responseType);
        return currentState;
    }

    public IntercomState onDisconnect() {
        currentState = currentState.onDisconnect();
        return currentState;
    }

    public IntercomState onIncomingDisconnect() {
        currentState = currentState.onIncomingDisconnect();
        return currentState;
    }


    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        currentState.WriteDiscreteActionMask(actionMask);
    }

    public void tick() {
        if (currentState != null)
            this.currentState.tick();
    }
}