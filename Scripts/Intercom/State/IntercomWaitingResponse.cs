using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents.Actuators;

public class IntercomWaitingResponse : IntercomState {

    public IntercomWaitingResponse(Context context) : base(context, IIntercomState.IntercomStateTypes.STATE_WAITING_RESPONSE) {
    }

    override
    public IntercomState onResponse(IIntercomState.IntercomCommandResponse response, IIntercomState.IntercomCommands command, Context remoteContext) {
        switch (response) {
            case IIntercomState.IntercomCommandResponse.ACCEPT:
                processCommand(command, context);
                return new IntercomConnected(context);
            case IIntercomState.IntercomCommandResponse.REJECT:
                return new IntercomConnected(context);
            default:
                return base.onResponse(response, command, context);
        }
    }

    private void processCommand(IIntercomState.IntercomCommands command, Context remoteContext) {
        int pumpedAmount;
        int sumLevelBefore, sumLevelAfter;
        switch (command) {
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER:
                sumLevelBefore = context.resourceMap[LifeResourceType.Water].currentLevel() + remoteContext.resourceMap[LifeResourceType.Food].currentLevel();

                pumpedAmount = context.resourceMap[LifeResourceType.Water]
                    .pumpFrom(remoteContext.resourceMap[LifeResourceType.Food], context.resourceMap[LifeResourceType.Water].tankConsumeLimit());

                sumLevelAfter = context.resourceMap[LifeResourceType.Water].currentLevel() + remoteContext.resourceMap[LifeResourceType.Food].currentLevel();
                //if (sumLevelAfter == sumLevelBefore) 
                //    Debug.Log("ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER pumped " + pumpedAmount + " food to water");
                //else 
                //    Debug.LogWarning("ACTION_PROPOSE_EXCHANGE_FOOD_TO_WATER pumped " + pumpedAmount + " food to water "+ " sumLevelBefore="+sumLevelBefore+"   sumLevelAfter="+sumLevelAfter);

                if (pumpedAmount > 0)
                    context.addReward(0.01f);
                break;
            case IIntercomState.IntercomCommands.ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD:
                sumLevelBefore = context.resourceMap[LifeResourceType.Food].currentLevel() + remoteContext.resourceMap[LifeResourceType.Water].currentLevel();
                pumpedAmount = context.resourceMap[LifeResourceType.Food]
                    .pumpFrom(remoteContext.resourceMap[LifeResourceType.Water], context.resourceMap[LifeResourceType.Food].tankConsumeLimit());
                sumLevelAfter = context.resourceMap[LifeResourceType.Food].currentLevel() + remoteContext.resourceMap[LifeResourceType.Water].currentLevel();
                //if (sumLevelAfter == sumLevelBefore)
                //    Debug.Log("ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD pumped " + pumpedAmount + " food to water");
                //else
                //    Debug.LogWarning("ACTION_PROPOSE_EXCHANGE_WATER_TO_FOOD pumped " + pumpedAmount + " water to food " + " sumLevelBefore=" + sumLevelBefore + "   sumLevelAfter=" + sumLevelAfter);

                if (pumpedAmount > 0)
                    context.addReward(0.01f);
                break;
            default:
                Debug.LogWarning("Response to unexpected command. Command: "+command);
                break;
        }
    }


    override
    public void WriteDiscreteActionMask(IDiscreteActionMask actionMask) {
        base.WriteDiscreteActionMask(actionMask);
    }
}