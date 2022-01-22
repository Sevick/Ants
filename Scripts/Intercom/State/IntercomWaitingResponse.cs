using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomWaitingResponse : IntercomState {

    public IntercomWaitingResponse(IIntercomState partner) : base(partner, "IntercomWaitingResponse") {

    }

    override
    public IntercomState onResponse(IIntercomState.IntercomCommands responseType) {
        switch (responseType) {
            default:
                return base.onResponse(responseType);
        }
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        return;
    }
}