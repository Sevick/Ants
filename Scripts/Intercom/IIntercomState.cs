using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

public interface IIntercomState : ITickable {

    enum IntercomStateTypes { STATE_NULL, STATE_DISCONNECTED, STATE_CONNECTED, STATE_HANDSHAKE, STATE_PROCESS_COMMAND, STATE_WAITING_RESPONSE, STATE_LAST };

    // ACTION_DONOTHING_LAST must me a last element - used to determine number of elements
    enum IntercomCommands { ACTION_DONOTHING, 
        ACTION_CONNECT_INITIATE, 
        ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER, ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD, 
        ACTION_DONOTHING_LAST };

    enum IntercomCommandResponse { ACTION_DONOTHING, 
        ACCEPT, REJECT, 
        ACTION_DONOTHING_LAST };


    public IntercomStateTypes type();

    // called ml-agent
    public IntercomState onConnect();

    // called by external intercom
    public IntercomState onIncomingCommand(IntercomCommands command);

    public IntercomState onCommand(MLCommand command);
    public IntercomState onResponse(IntercomCommandResponse response, IntercomCommands command, Context remoteContext);

    public IntercomState onDisconnect();
    public IntercomState onIncomingDisconnect();

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);
}