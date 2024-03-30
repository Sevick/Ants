using UnityEngine;
using System.Collections;

public class BaseTank : ITank {

    private ITank.TankEmptyActionType onEmptyAction = null;

    public LifeResourceType lifeResource;
    public int capacity;
    public int initVolume;          // Volume to be set on create & reset 
    public int currentVolume = 0;
    public int consumeLimit = -1;   // -1 - no limit
    public int fillLimit = -1;


    public BaseTank(LifeResourceType lifeResource, int capacity) {
        this.lifeResource = lifeResource;
        this.capacity = capacity;        
    }

    public string getName() {
        return "BaseTank";
    }

    public BaseTank(LifeResourceType lifeResource, int capacity, int initLevel = 0, int consumeLimit = -1, int fillLimit = -1, ITank.TankEmptyActionType onEmptyAction = null)
        : this(lifeResource, capacity) {

        this.consumeLimit = consumeLimit;
        this.fillLimit = fillLimit;
        this.initVolume = initLevel;
        this.onEmptyAction = onEmptyAction;
        this.currentVolume = initLevel;
    }

    public LifeResourceType tankResourceType() {
        return lifeResource;
    }


    protected int limitCheck(int limit, int amount, bool allOrNothing = false) {
        int checkedAmount = amount;
        if (limit == 0)
            checkedAmount = 0;
        if (limit>0 && amount>limit) {
            if (allOrNothing)
                return -1;
            else
                checkedAmount = limit;
        }
        return checkedAmount;
    }

    virtual public int consumeCheck(int amount, bool allOrNothing = false) {
        int checkedAmount = limitCheck(consumeLimit, amount, allOrNothing);
        if (checkedAmount == -1)
            return checkedAmount;

        int volumeCheck = limitCheck(currentVolume, checkedAmount, allOrNothing);
        if (volumeCheck == -1)
            return volumeCheck;

        return volumeCheck;
    }
    
    virtual public int consume(int amount, bool allOrNothing = false) {
        int checkedAmount = consumeCheck(amount, allOrNothing);
        if (checkedAmount > 0) {
            currentVolume -= checkedAmount;
            if (currentVolume == 0 && onEmptyAction != null)
                onEmptyAction(this);
        }
        return checkedAmount;
    }

    virtual public int fillCheck(int amount, bool allOrNothing = false) {
        int checkedAmount = limitCheck(fillLimit, amount, allOrNothing);
        if (checkedAmount == -1)
            return checkedAmount;

        int volumeCheck = limitCheck(spaceLeft(), checkedAmount, allOrNothing);
        return volumeCheck;
    }

    virtual public int fill(int amount, bool allOrNothing = false) {
        int checkedAmount = fillCheck(amount, allOrNothing);
        if (checkedAmount > 0)
            currentVolume += checkedAmount;
        return checkedAmount;
    }

    virtual public int pumpFrom(ITank partner, int amount, bool allOrNothing = false) {
        int wantedAmount = fillCheck(amount, allOrNothing);
        int availableAmount = partner.consumeCheck(amount, allOrNothing);
        int amountToPump = wantedAmount > availableAmount ? availableAmount : wantedAmount;        

        if (partner.consume(amountToPump, true) == -1) {
            Debug.LogError("Unable to consume contracted amount");
            return -1;
        }
        if (fill(amountToPump, true) == -1) {
            Debug.LogError("Unable to fill pumped out amount");
            return -1;
        }
        return amountToPump;
    }


    virtual public int spaceLeft() {
        return (tankCapacity() - currentLevel());
    }


    virtual public int currentLevel() {
        return currentVolume;
    }

    virtual public int tankCapacity() {
        return capacity;
    }


    virtual public void tick() {
    }


    virtual public void reset() {
        currentVolume = initVolume;
    }

    public int tankConsumeLimit() {
        return consumeLimit;
    }
    public int tankFillLimit() {
        return fillLimit;
    }
}