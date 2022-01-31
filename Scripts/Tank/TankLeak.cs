using UnityEngine;
using System.Collections;

public class TankLeak : BaseTank {

    private int tickLeak;

    public TankLeak(LifeResourceType lifeResource, int capacity, int tickLeak) 
        : base(lifeResource, capacity) {

        this.tickLeak = tickLeak;
    }

    public TankLeak(LifeResourceType lifeResource, int capacity, int tickLeak, int initLevel = 0, int consumeLimit = -1, int fillLimit = -1, ITank.TankEmptyActionType onEmptyAction = null)
        : base(lifeResource, capacity, initLevel, consumeLimit, fillLimit, onEmptyAction) {

        this.tickLeak = tickLeak;
    }

    override
    public void tick() {
        consume(tickLeak);
    }
}