using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;

public class IntercomDisconnected : IntercomState {
    override
    public IntercomState onConnect() {
        return new IntercomConnected();
    }
    override
    public IntercomState onIncomingConnect() {
        return new IntercomConnected();
    }

    override
    public IntercomState onCommand() {
        return this;
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
