using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

public interface IIntercomState {

    enum IntercomStateType { DISCONNECTED, WAITING_COMMAND, WAITING_RESPONSE };
    enum IntercomCommandResponseType { ACCEPT, REJECT };
    enum IntercomReply { ACK, NACK };

    public IntercomState onConnect();
    public IntercomState onIncomingConnect();

    public IntercomState onCommand();
    public IntercomState onResponse(IntercomCommandResponseType responseType);

    public IntercomState onDisconnect();
    public IntercomState onIncomingDisconnect();

    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);
}


