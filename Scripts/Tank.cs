using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : ITickable {

    public delegate void TankEmptyActionType(string message);

    private LifeResourceType lifeResource { get; set; }
    public int capacity;
    public int currentVolume;
    private int tickLeak;
    public int refillAmount;
    private TankEmptyActionType onEmptyAction;

    public long stats_Refilled = 0;

    public Tank(LifeResourceType lifeResource, int capacity, int refillAmount, int tickLeak) {        
        this.capacity = capacity;
        this.currentVolume = capacity * 2 / 3;
        this.lifeResource = lifeResource;
        this.refillAmount = refillAmount;
        this.tickLeak = tickLeak;
    }

    public Tank(LifeResourceType lifeResource, int capacity, int refillAmount, int tickLeak, TankEmptyActionType onEmptyAction)
        : this(lifeResource, capacity, refillAmount, tickLeak) {
        this.onEmptyAction = onEmptyAction;
    }

    public int refill(ResourceProvider resourceProvider) {
        int consumeAmount = capacity - currentVolume >= refillAmount ? refillAmount : capacity - currentVolume;
        int consumedAmount = resourceProvider.Consume(consumeAmount);
        currentVolume += consumedAmount;
        stats_Refilled += consumedAmount;
        return consumedAmount;
    }

    // Start is called before the first frame update
    public void tick() {
        if (tickLeak != 0) {
            if (currentVolume <= tickLeak) {
                currentVolume = 0;
                if (onEmptyAction != null) {
                    onEmptyAction(lifeResource.ToString());
                }
            }
            else
                currentVolume = currentVolume - tickLeak;
        }
    }

    public void reset() {
        this.currentVolume = capacity * 2 / 3;
        stats_Refilled = 0;
    }

    public int spaceLeft() {
        return (capacity - currentVolume);
    }

}
