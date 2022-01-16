using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

public class IntercomWaitingCommand : IntercomState {

    override
    public IntercomState onConnect() {
        return this;
    }
    override
    public IntercomState onIncomingConnect() {
        return this;
    }
    override
    public IntercomState onCommand() { 
        return new IntercomWaitingResponse();
    }
    override
    public IntercomState onResponse(IIntercomState.IntercomCommandResponseType responseType) {
        return this;
    }
    override
    public IntercomState onDisconnect() {
        return new IntercomDisconnected();
    }
    override
    public IntercomState onIncomingDisconnect() {
        return new IntercomDisconnected();
    }
    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {

    }
}
