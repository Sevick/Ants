using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomDisconnected : IntercomState {

    public IntercomDisconnected(Context context) : base (context, IIntercomState.IntercomStateTypes.STATE_DISCONNECTED) {
    }

    override
    public IntercomState onConnect() {
        return new IntercomConnected(context);
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:
                return new IntercomHandshake(context);
            default:
                return base.onIncomingCommand(command);
        }
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = (IIntercomState.IntercomCommands) mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_DONOTHING:
                // TODO: wait a few cycles?
                break;
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:
                context.partner.onIncomingCommand(command); // stay in current state waiting for response (accept/reject)
                break;
            default:
                return base.onCommand(mlCommand);
        }
        return this;
    }

    override
    public IntercomState onResponse(IIntercomState.IntercomCommandResponse response, IIntercomState.IntercomCommands command, Context remoteContext) {
        switch (response) { 
            case IIntercomState.IntercomCommandResponse.ACCEPT :
                return new IntercomConnected(context);
            case IIntercomState.IntercomCommandResponse.REJECT:
                //Debug.Log("Connection rejected");
                return null;    // terminate Intercom
            default:
                return base.onResponse(response, command, context);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        base.WriteDiscreteActionMask(actionMask);
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE, true);   
    }
}