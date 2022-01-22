using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

public interface IIntercomState : ITickable {

    enum IntercomCommandResponseType { ACCEPT, REJECT };
    enum IntercomReply { ACK, NACK };

    enum IntercomCommands { ACTION_DONOTHING, ACTION_CONNECT_INITIATE, ACTION_CONNECT_ACCEPT, ACTION_CONNECT_REJECT, ACTION_PROPOSE_EXCHANGE };

    public void setContext(Context context);
    public IntercomState onConnect();
    public IntercomState onIncomingCommand(IntercomCommands command);

    public IntercomState onCommand(MLCommand command);
    public IntercomState onResponse(IntercomCommands responseType);

    public IntercomState onDisconnect();
    public IntercomState onIncomingDisconnect();

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);
}