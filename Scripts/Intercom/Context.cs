using UnityEngine;
using Unity.MLAgents.Actuators;
using System.Collections;
using System.Collections.Generic;

public class Context {

    public delegate void AddRewardType(float score);
    public delegate void LockMoveType(bool locked);

    public IIntercomState partner;
    public Dictionary<LifeResourceType, ITank> resourceMap;
    public AddRewardType addReward;
    public LockMoveType lockMove;

    public Context(IIntercomState partner, Dictionary<LifeResourceType, ITank> resourceMap, AddRewardType addReward, LockMoveType lockMove) {
        this.partner = partner;
        this.resourceMap = resourceMap;
        this.addReward = addReward;
        this.lockMove = lockMove;
    }
}