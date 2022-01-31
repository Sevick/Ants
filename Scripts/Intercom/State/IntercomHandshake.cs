using Unity.MLAgents.Actuators;

public class IntercomHandshake : IntercomState {

    public IntercomHandshake(Context context) : base (context, IIntercomState.IntercomStateTypes.STATE_HANDSHAKE) {
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = (IIntercomState.IntercomCommands) mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE:                
                break; // Do nothing - connection from that partner already initialized
            case (int)IIntercomState.IntercomCommands.ACTION_DONOTHING:
                // TODO: wait a few cycles?
                break;
            default:
                //return base.onCommand(mlCommand);
                break;
        }

        IIntercomState.IntercomCommandResponse response = (IIntercomState.IntercomCommandResponse) mlCommand.getBranch(INTERCOM_RESPONSES_BRANCH);
        switch (response) {
            case IIntercomState.IntercomCommandResponse.ACCEPT:
                context.partner.onResponse(response, IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE, context);
                return new IntercomConnected(context);
            case IIntercomState.IntercomCommandResponse.REJECT:
                context.partner.onResponse(response, IIntercomState.IntercomCommands.ACTION_CONNECT_INITIATE, context);
                return null;
            default:
                return base.onCommand(mlCommand);
        }
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
        base.WriteDiscreteActionMask(actionMask);
        actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, (int) IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, (int) IIntercomState.IntercomCommandResponse.ACCEPT, true);
        actionMask.SetActionEnabled(INTERCOM_RESPONSES_BRANCH, (int) IIntercomState.IntercomCommandResponse.REJECT, true);
    }
}