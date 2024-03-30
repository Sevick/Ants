using UnityEngine;
using System.Collections;

public class TankVis : ITank {

    private ITank tankImpl;

    private GameObject tanker;
    private Transform tankerLevel;


    public string getName() {
        return tanker.name;
    }

    public TankVis(ITank tankImpl, GameObject tanker) {
        this.tankImpl = tankImpl;
        this.tanker = tanker;
        this.tankerLevel = tanker.transform.Find("Level");
    }

    public void tick(){
        tankImpl.tick();
        updateStats();
    }

    private void updateStats() {
        float scale;
        if (tankCapacity() != 0)
            scale = (float)currentLevel() / tankCapacity();
        else
            scale = 1.0f;
        tankerLevel.localScale = new Vector3(tankerLevel.localScale.x, scale, tankerLevel.localScale.z);
        tankerLevel.localPosition = new Vector3(tankerLevel.localPosition.x, scale - 1, tankerLevel.localPosition.z);
    }


    public int consumeCheck(int amount, bool allOrNothing = false) {
        return tankImpl.consumeCheck(amount, allOrNothing);
    }
    public int consume(int amount, bool allOrNothing = false) {
        return tankImpl.consume(amount, allOrNothing);
    }

    public int fillCheck(int amount, bool allOrNothing) {
        return tankImpl.fillCheck(amount, allOrNothing);
    }
    public int fill(int amount, bool allOrNothing) {
        return tankImpl.fill(amount, allOrNothing);
    }

    public int currentLevel() {
        return tankImpl.currentLevel();
    }
    public int tankCapacity() {
        return tankImpl.tankCapacity();
    }

    public int spaceLeft() {
        return tankImpl.spaceLeft();
    }

    public void reset() {
        tankImpl.reset();
    }

    public int pumpFrom(ITank partner, int amount, bool allOrNothing) {
        return tankImpl.pumpFrom(partner, amount, allOrNothing);
    }

    public LifeResourceType tankResourceType() {
        return tankImpl.tankResourceType();
    }

    public int tankConsumeLimit() {
        return tankImpl.tankConsumeLimit();
    }
    public int tankFillLimit() {
        return tankImpl.tankFillLimit();
    }
}
