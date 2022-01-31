using UnityEngine;
using System.Collections;

public class TankEndless : BaseTank {
    public TankEndless(LifeResourceType lifeResource, int capacity) : base(lifeResource, capacity) {
    }

    public TankEndless(LifeResourceType lifeResource, int capacity, int initLevel = 0, int consumeLimit = -1, int fillLimit = -1)
        : base(lifeResource, capacity, initLevel, consumeLimit, fillLimit, null) {
    }

    override
    public int consume(int amount, bool allOrNothing = false) {
        return amount;
    }
}