using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;
using Unity.MLAgents;

public class CommandExchangeResources : Command {

    public CommandExchangeResources() : base((int) IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER, 1) {
    }

    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) { 
        switch (currentParameter) {
            case 0:
                break;
        }
    }
}