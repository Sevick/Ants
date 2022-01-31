using UnityEngine;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class Context {

    public delegate void AddRewardType(float score);

    public IIntercomState partner;
    public Dictionary<LifeResourceType, ITank> resourceMap;
    public AddRewardType addReward;

    public Context(IIntercomState partner, Dictionary<LifeResourceType, ITank> resourceMap, AddRewardType addReward) {
        this.partner = partner;
        this.resourceMap = resourceMap;
        this.addReward = addReward;
    }
}