using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomStateMachine : IIntercomState, ITickable {

    //private IIntercomState intercomState = new IntercomStateDisconnected();
    IntercomState currentState = null;
    IIntercomState partner;
    Context context;

    public IntercomStateMachine() {
    }

    public IntercomStateMachine(Context context) {
        init(context);
    }

    public void init(Context context) {
        this.currentState = new IntercomDisconnected(context);
        this.context = context;
    }

    public IIntercomState.IntercomStateTypes type() {
        if (currentState != null)
            return currentState.type();
        else
            return IIntercomState.IntercomStateTypes.STATE_NULL;
    }

    public IntercomState onConnect() {
        if (this.currentState != null) {
            switchTo(currentState.onConnect());
        }
        return this.currentState;
    }

    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        if (this.currentState != null) {
            switchTo(currentState.onIncomingCommand(command));
        }
        return this.currentState;
    }

    public IntercomState onCommand(MLCommand mlCommand) {
        if (this.currentState != null) {
            switchTo(currentState.onCommand(mlCommand));
        }
        return this.currentState;
    }

    public IntercomState onResponse(IIntercomState.IntercomCommandResponse response, IIntercomState.IntercomCommands command, Context remoteContext) {
        if (this.currentState != null) {
            switchTo(currentState.onResponse(response, command, context));
        }
        return this.currentState;
    }

    public IntercomState onDisconnect() {
        if (this.currentState != null) {
            switchTo(currentState.onDisconnect());
        }
        return this.currentState;
    }

    public IntercomState onIncomingDisconnect() {
        if (this.currentState != null) {
            switchTo(currentState.onIncomingDisconnect());
        }
        
        return this.currentState;
    }

    private void switchTo(IntercomState newState) {
        //Debug.Log("-> " + newState.getStateName());
        currentState = newState;
    }


    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        if (this.currentState != null) {
            this.currentState.WriteDiscreteActionMask(actionMask);
        }
    }

    public void tick() {
        if (this.currentState != null)
            this.currentState.tick();
    }
}