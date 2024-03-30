using UnityEngine;
using System.Collections;

public interface ITank : ITickable {

    public delegate void TankEmptyActionType(ITank sender);

    public LifeResourceType tankResourceType();
    public int tankConsumeLimit();
    public int tankFillLimit();

    public int consumeCheck(int amount, bool allOrNothing = false);
    public int consume(int amount, bool allOrNothing = false);
    public int fillCheck(int amount, bool allOrNothing = false);
    public int fill(int amount, bool allOrNothing = false);

    public int pumpFrom(ITank partner, int amount, bool allOrNothing = false);

    public int currentLevel();
    public int tankCapacity();

    public int spaceLeft();

    public void reset();

    public string getName();
}
