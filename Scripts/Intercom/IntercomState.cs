using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

using Unity.MLAgents;

public abstract class IntercomState : IIntercomState {

    abstract public IntercomState onConnect();
    abstract public IntercomState onIncomingConnect();

    abstract public IntercomState onCommand();
    abstract public IntercomState onResponse(IIntercomState.IntercomCommandResponseType responseType);

    abstract public IntercomState onDisconnect();
    abstract public IntercomState onIncomingDisconnect();

    abstract public void WriteDiscreteActionMask(IDiscreteActionMask actionMask);
}
