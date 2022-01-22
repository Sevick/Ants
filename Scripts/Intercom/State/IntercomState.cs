using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents;

public abstract class IntercomState : IIntercomState {

    protected const int INTERCOM_ACTIONS_BRANCH = 1;

    protected string stateName;
    protected IIntercomState partner;
    protected Context context;

    protected long stateTime = 0;

    public IntercomState(IIntercomState partner, string stateName) {
        this.partner = partner;
        this.stateName = stateName;
    }

    public void setContext(Context context) {
        this.context = context;
    }

    virtual public IntercomState onConnect() {
        Debug.LogError(stateName + " Unexpected intercom onConnect");
        return this;
    }

    virtual public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        Debug.LogError(stateName + " Unexpected intercom onIncomingCommand: " + command);
        return this;
    }

    virtual public IntercomState onCommand(MLCommand mlCommand) {
        Debug.LogError(stateName + " Unexpected intercom onCommand: " + mlCommand);
        return this;
    }

    virtual public IntercomState onResponse(IIntercomState.IntercomCommands responseType) {
        Debug.LogError(stateName + " Unexpected intercom onResponse: " + responseType);
        return this;
    }

    virtual public IntercomState onDisconnect() {
        partner.onIncomingDisconnect();
        return null;
    }

    virtual public IntercomState onIncomingDisconnect() {
        return null;
    }

    abstract public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);

    virtual public void tick() {

    }
}
