using UnityEngine;


public class MLCommand {

    public int[] dicreteActions = null;

    public MLCommand(int[] discreteActions) {
        this.dicreteActions = discreteActions;
    }

    public int getBranch(int branchIdx) {
        if (dicreteActions == null)
            return 0;
        else {
            int command = dicreteActions[branchIdx];
            return command;
        }
    }
}