using UnityEngine;
using Unity.MLAgents.Actuators;

public abstract class IntercomState : IIntercomState {

    public const int INTERCOM_ACTIONS_BRANCH = 1;
    public const int INTERCOM_RESPONSES_BRANCH = 2;

    protected IIntercomState.IntercomStateTypes stateType;
    protected Context context;

    protected long stateTime = 0;

    public IntercomState(Context context, IIntercomState.IntercomStateTypes stateType) {
        this.context = context;
        this.stateType = stateType;
    }

    public IIntercomState.IntercomStateTypes type() {
        return stateType;
    }

    virtual public IntercomState onConnect() {
        //Debug.Log(stateName + " Unexpected intercom onConnect");
        return this;
    }

    virtual public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        //Debug.Log(stateName + " Unexpected intercom onIncomingCommand: " + command);
        return this;
    }

    virtual public IntercomState onCommand(MLCommand mlCommand) {
        if (mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH) != (int) IIntercomState.IntercomCommands.ACTION_DONOTHING) {
            //Debug.Log(stateName + " Unexpected intercom onCommand INTERCOM_ACTIONS_BRANCH: " + mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH));
        }
        if (mlCommand.getBranch(INTERCOM_RESPONSES_BRANCH) != (int)IIntercomState.IntercomCommands.ACTION_DONOTHING) {
            //Debug.Log(stateName + " Unexpected intercom onCommand INTERCOM_RESPONSES_BRANCH: " + mlCommand.getBranch(INTERCOM_RESPONSES_BRANCH));
        }
        return this;
    }

    virtual public IntercomState onResponse(IIntercomState.IntercomCommandResponse responseType, IIntercomState.IntercomCommands comman, Context remoteContext) {
        //Debug.Log(stateName + " Unexpected intercom onResponse: " + responseType);
        return this;
    }

    virtual public IntercomState onDisconnect() {
        context.partner.onIncomingDisconnect();
        return null;
    }

    virtual public IntercomState onIncomingDisconnect() {
        return null;
    }

    virtual public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        // by default disable all actions (except first one - ACTION_DONOTHING (impossible to disable all action in branch)
        for (var i=1; i<=(int) IIntercomState.IntercomCommands.ACTION_DONOTHING_LAST; i++) {
            actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, i, false);
        }
        for (var i = 1; i <= (int)IIntercomState.IntercomCommandResponse.ACTION_DONOTHING_LAST; i++) {
            actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, i, false);
        }
    }

    virtual public void tick() {
        // TODO: Implement disconnect on timeout
    }
}