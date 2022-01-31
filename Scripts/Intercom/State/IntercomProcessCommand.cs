using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomProcessCommand : IntercomState, ITickable {

    IIntercomState.IntercomCommands command;

    public IntercomProcessCommand(Context context, IIntercomState.IntercomCommands command) : base(context, IIntercomState.IntercomStateTypes.STATE_PROCESS_COMMAND) {
        this.command = command;
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            default:
                return base.onIncomingCommand(command);
        }        
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = (IIntercomState.IntercomCommands) mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            default:
                break;  // Ignore commands in ACTIONS branch
        }
        IIntercomState.IntercomCommandResponse response = (IIntercomState.IntercomCommandResponse) mlCommand.getBranch(INTERCOM_RESPONSES_BRANCH);
        switch (response) {
            case IIntercomState.IntercomCommandResponse.ACCEPT:
            case IIntercomState.IntercomCommandResponse.REJECT:
                context.partner.onResponse(response, this.command, context);
                return new IntercomConnected(context);
            default:
                return base.onCommand(mlCommand);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        base.WriteDiscreteActionMask(actionMask);
        actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, (int)IIntercomState.IntercomCommandResponse.ACCEPT, true);
        actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, (int)IIntercomState.IntercomCommandResponse.REJECT, true);
    }
}