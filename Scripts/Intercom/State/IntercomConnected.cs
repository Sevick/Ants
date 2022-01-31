using Unity.MLAgents.Actuators;

public class IntercomConnected : IntercomState, ITickable {

    public IntercomConnected(Context context) : base(context, IIntercomState.IntercomStateTypes.STATE_CONNECTED) {
    }

    override
    public IntercomState onIncomingCommand(IIntercomState.IntercomCommands command) {
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER:
                return new IntercomProcessCommand(context, command);
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD:
                return new IntercomProcessCommand(context, command);
            default:
                return base.onIncomingCommand(command);
        }        
    }

    override
    public IntercomState onCommand(MLCommand mlCommand) {
        IIntercomState.IntercomCommands command = (IIntercomState.IntercomCommands) mlCommand.getBranch(INTERCOM_ACTIONS_BRANCH);
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER:
                context.partner.onIncomingCommand(command);
                return new IntercomWaitingResponse(context);
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD:
                context.partner.onIncomingCommand(command);
                return new IntercomWaitingResponse(context);
            default:
                return base.onCommand(mlCommand);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        base.WriteDiscreteActionMask(actionMask);
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int)IIntercomState.IntercomCommands.ACTION_DONOTHING, true);   // cannot disable whole branch
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int)IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER, true);
        actionMask.SetActionEnabled(INTERCOM_ACTIONS_BRANCH, (int)IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD, true);

        // enable disconnect after delay?
    }
}