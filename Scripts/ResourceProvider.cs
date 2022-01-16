using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourceProvider : MonoBehaviour {

    public int currentAmount = 5000;
    public LifeResourceType lifeResource;

    public int volume = 5000;
    public int renewAmount = 5000;
    public int renewInterval = 5000;

    public long stats_Refilled = 0;

    ResourceProvider(int volume, int renewAmount, int renewInterval)
    {
        this.volume = volume;
        this.renewAmount = renewAmount;
        this.renewInterval = renewInterval;
        this.currentAmount = volume;
    }

    public int Consume(int limit) {
        int consumeAmount = limit;
        if (currentAmount<limit)
            consumeAmount = currentAmount;

        //currentAmount -= consumeAmount;
        stats_Refilled += consumeAmount;
        return consumeAmount;
    }

    void renew()
    {

    }

    public void reset() {
        this.currentAmount = volume;
        stats_Refilled = 0;
    }
}
